using System.Collections.Generic;


namespace DataOrientedDriver
{
    public sealed class BehaviorSystem : IScheduler
    {
        private Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        private Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        private bool CurrentIsFirst = true;
        private List<ISchedulable> nodes = new List<ISchedulable>();

        private ref Queue<ISchedulable> getCurrentQueue() { if (CurrentIsFirst) return ref firstQueue; else return ref secondQueue; }

        public BehaviorSystem() {}

        public int NodeCount
        {
            get { return nodes.Count; }
        }
        public void AddNode(ISchedulable s) { nodes.Add(s); }
        public ISchedulable GetNode(int i) 
        { 
            if (i >= 0 && i < nodes.Count) return nodes[i];
            else return null;
        }
        // will remove it and the entire subtree.
        public void RemoveNode(ISchedulable s)
        {
            if(!nodes.Contains(s)) return;
            nodes.Remove(s);
            if(s is Decorator d)
            {
                RemoveNode(d.Child);
            }
            if(s is Composite c)
            {
                foreach(var child in c.GetChildren()) RemoveNode(child);
            }
        }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
        public void Step(float dt)
        {
            ref var currentQueue = ref getCurrentQueue();
            while (currentQueue.Count > 0)
            {
                var currentNode = currentQueue.Dequeue();
                var s = currentNode.Status;
                // if the node is not aborted, we step it.
                // note that we already dequeue the node, so if the node is aborted, it will not remain on the queue anymore.
                if (s != NodeStatus.ABORTED) currentNode.Step(dt);
                s = currentNode.Status;
                if (s == NodeStatus.RUNNING) PostSchedule(currentNode); // we post it to be executed next frame if it is running.
            }
            CurrentIsFirst = !CurrentIsFirst;
        }
    }
}
