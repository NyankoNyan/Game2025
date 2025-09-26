using System;
using System.Collections.Generic;

namespace BlockSpawnLogic
{
    public class LogicBlockNode
    {
        public LogicBlockNode Parent { get; private set; }
        public List<LogicBlockNode> Children { get; private set; } = new();
        public int Depth { get; set; }
        public LogicBlockSettings Settings { get; set; }

        public void AddChild(LogicBlockNode node)
        {
            if (node.Parent != null)
            {
                throw new InvalidOperationException("Node already has a parent.");
            }
            node.Parent = this;
            Children.Add(node);
        }
    }
}
