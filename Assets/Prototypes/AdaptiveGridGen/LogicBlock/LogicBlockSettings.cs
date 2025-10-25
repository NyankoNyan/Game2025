using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockSpawnLogic
{
    [Serializable]
    public class LogicBlockSettings
    {
        public string Id = "";
        public float Weight = 1f;
        public int MaxRoomsCount = -1;
        public Vector2Int SpawnDepth = new Vector2Int(0, 100);
        public Vector2Int OutputRoomsCount = new Vector2Int(0, 2);
        public string[] CompatibleOutput = new string[0];
        public string[] CompatibleInput = new string[0];

        public AvailableSides Sides = new(){
            Left = true,
            Right = true,
            Top = true,
            Bottom = true
        };

        public GridBlockSettings GridSettings = new();
    }

    [Serializable]
    public struct AvailableSides
    {
        public bool Left;
        public bool Right;
        public bool Top;
        public bool Bottom;

        public IEnumerable<Sides> AsSides()
        {
            if (Left) yield return Sides.Left;
            if (Right) yield return Sides.Right;
            if (Top) yield return Sides.Top;
            if (Bottom) yield return Sides.Bottom;
        }
    }

    [Serializable]
    public class GridBlockSettings
    {
        public Vector2Int SizeX = new(5, 5); // In Units
        public Vector2Int SizeY = new(5, 5); // In Units
        public bool LockSize = false;
        public int DistanceToBlock = 5; // In Blocks
        public int PlaceSearchRadius = 3; // In Blocks
        public int FreeZoneRadius = 1; // In Blocks

        public Vector2Int SizeAxis(int axis)
        {
            switch (axis)
            {
                case 0: return SizeX;
                case 1: return SizeY;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}