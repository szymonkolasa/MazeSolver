using System.Collections.Generic;

namespace MazeSolver
{
    public class Cell
    {
        public bool IsVisited { get; set; }
        public bool NorthWall { get; set; }
        public bool SouthWall { get; set; }
        public bool EastWall { get; set; }
        public bool WestWall { get; set; }
        public List<Cell> Neighbours { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public List<double> QValues { get; set; }
        public int Reward { get; set; }

        public Cell()
        {
            Neighbours = new List<Cell>();
            NorthWall = true;
            SouthWall = true;
            EastWall = true;
            WestWall = true;

            QValues = new List<double>(new double[] { 0.0d, 0.0d, 0.0d, 0.0d });
        }
    }
}
