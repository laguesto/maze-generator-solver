using MazeGeneratorSolver.MODELS;

namespace MazeGeneratorSolver.ALGORITHMS
{
    public class MazeGenerator
    {
        // Генерація випадкового лабіринту
        public void Generate(Maze maze)
        {
            Random random = new Random();

            // Стек для збереження шляху генерації
            Stack<Cell> stack = new Stack<Cell>();

            // Початкова клітинка
            Cell start = maze.Cells[0, 0];

            start.IsVisited = true;
            stack.Push(start);

            // Поки стек не порожній
            while (stack.Count > 0)
            {
                // Отримуємо поточну клітинку
                Cell current = stack.Peek();

                // Знаходимо невідвіданих сусідів
                List<Cell> neighbors = GetUnvisitedNeighbors(maze, current);

                if (neighbors.Count > 0)
                {
                    // Випадково вибираємо сусідню клітинку
                    int index = random.Next(neighbors.Count);
                    Cell next = neighbors[index];

                    // Відкриваємо прохід між клітинками
                    RemoveWalls(current, next);

                    // Позначаємо клітинку відвіданою
                    next.IsVisited = true;

                    // Додаємо її до стеку
                    stack.Push(next);
                }
                else
                {
                    // Повертаємося до попередньої клітинки
                    stack.Pop();
                }
            }

            // Скидаємо стан відвідування для подальшого пошуку
            foreach (Cell cell in maze.Cells)
            {
                cell.IsVisited = false;
            }
        }

        // Пошук невідвіданих сусідніх клітинок
        private List<Cell> GetUnvisitedNeighbors(Maze maze, Cell cell)
        {
            List<Cell> neighbors = new List<Cell>();

            int row = cell.Row;
            int column = cell.Column;

            // Сусід зверху
            if (row > 0 &&
                !maze.Cells[row - 1, column].IsVisited)
            {
                neighbors.Add(maze.Cells[row - 1, column]);
            }

            // Сусід справа
            if (column < maze.Columns - 1 &&
                !maze.Cells[row, column + 1].IsVisited)
            {
                neighbors.Add(maze.Cells[row, column + 1]);
            }

            // Сусід знизу
            if (row < maze.Rows - 1 &&
                !maze.Cells[row + 1, column].IsVisited)
            {
                neighbors.Add(maze.Cells[row + 1, column]);
            }

            // Сусід зліва
            if (column > 0 &&
                !maze.Cells[row, column - 1].IsVisited)
            {
                neighbors.Add(maze.Cells[row, column - 1]);
            }

            return neighbors;
        }

        // Видалення стін між сусідніми клітинками
        private void RemoveWalls(Cell current, Cell next)
        {
            // Якщо клітинки знаходяться в одному рядку
            if (current.Row == next.Row)
            {
                if (current.Column < next.Column)
                {
                    current.RightWall = false;
                    next.LeftWall = false;
                }
                else
                {
                    current.LeftWall = false;
                    next.RightWall = false;
                }
            }
            // Якщо клітинки знаходяться в одному стовпці
            else
            {
                if (current.Row < next.Row)
                {
                    current.BottomWall = false;
                    next.TopWall = false;
                }
                else
                {
                    current.TopWall = false;
                    next.BottomWall = false;
                }
            }
        }
    }
}
