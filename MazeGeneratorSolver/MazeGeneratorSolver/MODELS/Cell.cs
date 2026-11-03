
namespace MazeGeneratorSolver.MODELS
{
    public class Cell
    {
        // Координати клітинки в лабіринті
        public int Row { get; set; }
        public int Column { get; set; }

        // Стіни клітинки
        public bool TopWall { get; set; } = true;
        public bool RightWall { get; set; } = true;
        public bool BottomWall { get; set; } = true;
        public bool LeftWall { get; set; } = true;

        // Стани клітинки
        public bool IsVisited { get; set; } = false;
        public bool IsStart { get; set; } = false;
        public bool IsFinish { get; set; } = false;

        // Конструктор клітинки
        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
