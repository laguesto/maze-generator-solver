

namespace MazeGeneratorSolver.MODELS
{
    public class Maze
    {
        // Кількість рядків і стовпців лабіринту
        public int Rows { get; }
        public int Columns { get; }

        // Двовимірний масив клітинок
        public Cell[,] Cells { get; }

        // Конструктор лабіринту
        public Maze(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
            {
                throw new ArgumentException("Розміри лабіринту повинні бути більшими за нуль.");
            }

            Rows = rows;
            Columns = columns;

            // Створення масиву клітинок
            Cells = new Cell[Rows, Columns];

            // Заповнення лабіринту клітинками
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    Cells[row, column] = new Cell(row, column);
                }
            }
        }
    }
}