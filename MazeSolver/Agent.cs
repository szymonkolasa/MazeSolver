using System.Collections.Generic;

namespace MazeSolver
{
    public class Agent
    {
        public HashSet<Cell> VisitedCells { get; set; }
        public Cell CurrentCell { get; set; }

        public Agent()
        {
            VisitedCells = new HashSet<Cell>();
        }
    }
}
