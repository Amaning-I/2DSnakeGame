using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clone
{
    public class Direction
    {
        public readonly static Direction Left = new Direction(0, -1);
        public readonly static Direction Right = new Direction(0, 1);
        public readonly static Direction Up = new Direction(-1, 0);
        public readonly static Direction Down = new Direction(1, 0);
        private object HashCode;

        public int Rowoffset { get; }
        public int Columnoffset { get; }

        private Direction(int rowoffset, int columnoffset)
        {
            Rowoffset = rowoffset;
            Columnoffset = columnoffset;
        }

        public Direction Opposite()
        {
            return new Direction(-Rowoffset, -Columnoffset);
        }


        public override bool Equals(object obj)
        {
            return obj is Direction dir &&
                   Rowoffset == dir.Rowoffset &&
                   Columnoffset == dir.Columnoffset;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Rowoffset, Columnoffset);
        }

        public static bool operator ==(Direction left, Direction right)
        {
            return EqualityComparer<Direction>.Default.Equals(left, right);
        }

        public static bool operator !=(Direction left, Direction right)
        {
            return !(left == right);
        }
    }
}