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

        private Sides _side;
        public Sides Side { 
            get => _side; 
            set {
                if (Parent!=null)
                {
                    Parent.SidesFullness[(int)_side]--;
                    Parent.SidesFullness[(int)value]++;
                }
                _side = value;
            } 
        }
        
        public int[] SidesFullness { get; } = new int[(int)Sides.Last];

        public void AddChild(LogicBlockNode node)
        {
            if (node.Parent != null)
            {
                throw new InvalidOperationException("Node already has a parent.");
            }
            node.Parent = this;
            Children.Add(node);
        }

        public List<Sides> MinimalSides(AvailableSides searchMask)
        {
            List<Sides> result = new();

            int minFullness = int.MaxValue;

            foreach (var side in searchMask.AsSides())
            {
                int count = SidesFullness[(int)side];
                if (count < minFullness)
                {
                    result.Clear();
                    result.Add(side);
                    minFullness = count;
                }
                else if(count == minFullness)
                {
                    result.Add(side);
                }
            }

            return result;
        }
    }

    public enum Sides
    {
        First = 0,

        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3,

        Last = 3
    }
}