using System;
using System.Xml;
using System.Xml.Schema;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

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
            var resources = assembly.GetManifestResourceNames().Where(r => r.EndsWith(".xsd"));
            foreach (var resource in resources)
            {
                var schema = XmlSchema.Read(assembly.GetManifestResourceStream(resource), null);
                setting.Schemas.Add(schema);
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
            else return root;
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
            //Console.WriteLine(currentNode);
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
            MethodInfo method = null;
            object[] pars = null;
            Type[] types = null;
            var index = 0;
            var invoked = false;
            while (reader.Read()) // read the next token.
            {
                //Console.WriteLine($"{reader.NodeType}, {reader.Name}");
                if (reader.NodeType == XmlNodeType.Element)
                {
                    // the content of the <type> tag precisely describes the method name that we should use, so we use reflection to get that.
                    if (reader.Name == "type")
                    {
                        method = HandleType(reader);
                        types = method.GetParameters().Select(p => p.ParameterType).ToArray();
                        pars = new object[types.Length];
                    }
                    // if we encounter the <param> element, we parse them into a parameter array by passing in the types array.
                    else if (reader.Name == "param")
                    {
                        pars[index] = HandleParams(reader, types[index]);
                        index++;
                    }
                    // if we encounter another <node>, we have children nodes, we should recursively handle it.
                    else if (reader.Name == "node")
                    {
                        // another place where we need to invoke the method and build the node is when we found that we have children.
                        // we need to first create the node, then we can create the children.
                        if (!invoked)
                        {
                            method.Invoke(BuilderInstance, pars);
                            invoked = true;
                        }
                        HandleNode(reader);
                    }
                }
                // otherwise, we break the loop and return to the previous recursion level.
                else
                {
                    // if we reached the end element, where only </node> is possible because we handled </type> and </param>.
                    // we know that the necessary data is all get, and we can invoke the builder method, if it is not invoked before.
                    if(!invoked)
                    {
                        method.Invoke(BuilderInstance, pars);
                        invoked = true;
                    }
                    // we need to read past the </node> element after this node is done, so that our next read can be the next start tag.
                    // if not doing so, whitespace element would show up. No idea why.
                    reader.ReadEndElement();
                    // if we encountered the </node> element, we have to break out of the reading loop and return to last level of recursion.
                    break;
                }
            }
            // after we recursively built our children, we call End() on branch nodes.
            if (!method.Name.EndsWith("Action") && !method.Name.EndsWith("Condition")) 
            {
                End();
            }
        }

        protected object HandleParams(XmlReader reader, Type type)
        {
            reader.ReadStartElement("param");
            var value = reader.ReadContentAsString();
            reader.ReadEndElement();
            var converter = TypeDescriptor.GetConverter(type);
            return converter.ConvertFromString(value);
        }
        protected MethodInfo HandleType(XmlReader reader)
        {
            reader.ReadStartElement("type");
            var method = reader.ReadContentAsString();
            reader.ReadEndElement();
            return BuilderInstance.GetType().GetMethod(method);
        }
    }
}
