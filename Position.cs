using System;

namespace TicTacBoom
{
    [Serializable()]
    public struct Position
    {
        public int x, y;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Position p = (Position)obj;
            return x.Equals(p.x) && y.Equals(p.y);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(x, y).GetHashCode();
        }
        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
        public static Position FromString(string positionString)
        {
            // Remove opening bracket
            positionString = positionString.Remove(0, 1);
            // Remove closing bracket
            positionString = positionString.Remove(positionString.Length - 1);
            // Remove comma
            positionString = positionString.Replace(",", null);
            
            var splitPosition = positionString.Split(' ');

            return new Position{x = Int32.Parse(splitPosition[0]), y = Int32.Parse(splitPosition[1])};
        }
    }
}