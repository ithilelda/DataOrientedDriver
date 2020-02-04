using System;
using System.Xml;
using System.Xml.Schema;

namespace DataOrientedDriver
{
    public class BadBuilderUseException : Exception
    {
        public BadBuilderUseException(string m) : base(m) {}
        public BadBuilderUseException() {}
    }

    public abstract class BehaviorTreeBuilder<TBuilder>
        where TBuilder : BehaviorTreeBuilder<TBuilder>
    {
        protected abstract TBuilder BuilderInstance { get; }
        protected Behavior currentNode;
        protected BlackBoard bb;
        protected BehaviorSystem ss;
        protected Behavior root;

        public BehaviorTreeBuilder(BehaviorSystem ss, BlackBoard bb)
        {
            this.ss = ss;
            this.bb = bb;
        }

        public virtual Behavior BuildFromXml(string fullpath)
        {
            var setting = new XmlReaderSettings();
            setting.ValidationType = ValidationType.Schema;
            var assembly = typeof(Behavior).Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".xsd"))
                {
                    var schema = XmlSchema.Read(assembly.GetManifestResourceStream(resource), null);
                    setting.Schemas.Add(schema);
                }
            }
            
            using(var reader = XmlReader.Create(fullpath, setting))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "treeDef") Root();
                        else if (reader.Name == "nodes") Selector();
                        else if (reader.Name == "node") HandleNode(reader);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "treeDef") return Build();
                    }
                }
            }
            return null;
        }


        // the public interfaces for constructing the tree by hand.
        public virtual Behavior Build()
        {
            if (root == null) throw new BadBuilderUseException("Root node cannot be null!");
            else 
            {
                ss.PostSchedule(root);
                return root;
            }
        }
        public TBuilder Root()
        {
            root = new InfiniteDecorator(ss);
            currentNode = root;
            currentNode.Parent = null;
            ss.AddNode(root);
            return BuilderInstance;
        }
        public TBuilder Selector()
        {
            AddBranch(new Selector(ss));
            return BuilderInstance;
        }
        public TBuilder Sequence()
        {
            AddBranch(new Sequence(ss));
            return BuilderInstance;
        }
        public TBuilder Filter()
        {
            AddBranch(new Filter(ss));
            return BuilderInstance;
        }
        public TBuilder Repeat(int times)
        {
            AddBranch(new RepeatDecorator(ss, times));
            return BuilderInstance;
        }
        public TBuilder AlwaysTrueCondition()
        {
            Condition(new AlwaysTrueCondition(ss));
            return BuilderInstance;
        }
        public TBuilder AlwaysFalseCondition()
        {
            Condition(new AlwaysFalseCondition(ss));
            return BuilderInstance;
        }
        public TBuilder FakeSuccessAction()
        {
            Action(new FakeSuccessAction(ss));
            return BuilderInstance;
        }
        public TBuilder FakeFailureAction()
        {
            Action(new FakeFailureAction(ss));
            return BuilderInstance;
        }
        public TBuilder OneFailParallel()
        {
            AddBranch(new Parallel(ss));
            return BuilderInstance;
        }
        public TBuilder OneSuccessParallel()
        {
            AddBranch(new Parallel(ss, Parallel.Policy.One, Parallel.Policy.Ignore));
            return BuilderInstance;
        }
        public TBuilder Monitor()
        {
            AddBranch(new Parallel(ss, Parallel.Policy.All, Parallel.Policy.All));
            return BuilderInstance;
        }
        public TBuilder End()
        {
            currentNode = currentNode.Parent;
            if (currentNode == null) return BuilderInstance;
            while (currentNode is Decorator dec)
            {
                if (currentNode.Parent == null) return BuilderInstance;
                currentNode = currentNode.Parent;
            }
            return BuilderInstance;
        }

        // the protected methods for house keeping.
        protected TBuilder Action(Action a)
        {
            AddLeaf(a);
            return BuilderInstance;
        }
        protected TBuilder Condition(Condition c)
        {
            AddLeaf(c);
            return BuilderInstance;
        }
        protected void AddBranch(Behavior e)
        {
            if (currentNode is Filter fil)
            {
                fil.AddAction(e);
            }
            else if (currentNode is Decorator dec)
            {
                dec.SetChild(e);
            }
            else if (currentNode is Composite com)
            {
                com.AddChild(e);
            }
            else
            {
                throw new BadBuilderUseException($"Cannot add branch to node of type {currentNode.GetType()}.");
            }
            currentNode = e;
            ss.AddNode(e);
        }
        protected void AddLeaf(Behavior e)
        {
            if (currentNode is Filter fil)
            {
                if (e is Action) fil.AddAction(e);
                else if (e is Condition) fil.AddCondition(e);
                else throw new BadBuilderUseException($"Cannot add behavior of type {e.GetType()} to filter.");
            }
            else if (currentNode is Decorator dec)
            {
                dec.SetChild(e);
                End();
            }
            else if (currentNode is Composite com)
            {
                com.AddChild(e);
            }
            else
            {
                throw new BadBuilderUseException($"Cannot add leaf to node of type {currentNode.GetType()}.");
            }
            ss.AddNode(e);
        }

        protected void HandleNode(XmlReader reader)
        {
            // we are at the <node> tag when this function is called.
            reader.ReadStartElement("node");
            reader.ReadStartElement("type"); // now we read pass the <type> tag, and we can get the string type after this.
            var method_name = reader.ReadContentAsString();
            // the content of the <type> tag precisely describes the method name that we should use, so we use reflection to call that.
            var method = BuilderInstance.GetType().GetMethod(method_name);
            method?.Invoke(BuilderInstance, null);
            reader.ReadEndElement(); // read past </type>.
            while (reader.Read()) // read the next token.
            {
                // if we encounter another <node>, we have children nodes, we should recursively handle it.
                if (reader.NodeType == XmlNodeType.Element)
                {
                    HandleNode(reader);
                    
                }
                // otherwise, we break the loop and return to the previous recursion level.
                else break;
            }
            // after we recursively built our children, we call End() on branch nodes.
            if (!method_name.EndsWith("Action") && !method_name.EndsWith("Condition")) 
            {
                End();
            }
            // we need to read past the </node> element after this node is done, so that our next read can be the next start tag.
            reader.ReadEndElement();
        }
    }
}
