using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Размер для трёхмерной сетки с целочисленными параметрами.
    /// </summary>
    public class Size3 : ParameterVector3
    {
        public Size3() : base( 0, 0, 0 ) { }

        public Size3(int x, int y, int z) : base( x, y, z ) { }
    }
}