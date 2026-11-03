using MazeGeneratorSolver.MODELS;



namespace MazeGeneratorSolver.ALGORITHMS
{
    public class MazeSolver
    {
        // Список клітинок, які алгоритм перевірив
        public List<Cell> VisitedCells { get; private set; } = new List<Cell>();

        // Пошук найкоротшого шляху за допомогою BFS
        public List<Cell> FindPath(Maze maze, Cell start, Cell finish)
        {
            // Очищаємо попередній результат
            VisitedCells.Clear();

            // Черга для збереження клітинок
            Queue<Cell> queue = new Queue<Cell>();

            // Зберігаємо попередню клітинку
            Dictionary<Cell, Cell> previous = new Dictionary<Cell, Cell>();

            // Додаємо початкову клітинку
            queue.Enqueue(start);
            previous[start] = start;

            // Поки черга не порожня
            while (queue.Count > 0)
            {
                // Отримуємо першу клітинку з черги
                Cell current = queue.Dequeue();

                // Записуємо відвідану клітинку
                VisitedCells.Add(current);

                // Перевіряємо, чи досягли фінішу
                if (current == finish)
                {
                    return ReconstructPath(previous, start, finish);
                }

                // Отримуємо доступних сусідів
                List<Cell> neighbors = GetPassableNeighbors(maze, current);

                foreach (Cell neighbor in neighbors)
                {
                    // Перевіряємо, чи відвідували цю клітинку
                    if (!previous.ContainsKey(neighbor))
                    {
                        previous[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Якщо шлях не знайдено
            return new List<Cell>();
        }

        // Пошук шляху за допомогою DFS
        public List<Cell> FindPathDFS(Maze maze, Cell start, Cell finish)
        {
            // Очищаємо попередній результат
            VisitedCells.Clear();

            // Стек для збереження клітинок
            Stack<Cell> stack = new Stack<Cell>();

            // Зберігаємо попередню клітинку
            Dictionary<Cell, Cell> previous = new Dictionary<Cell, Cell>();

            // Додаємо початкову клітинку
            stack.Push(start);
            previous[start] = start;

            // Поки стек не порожній
            while (stack.Count > 0)
            {
                // Отримуємо останню додану клітинку
                Cell current = stack.Pop();

                // Записуємо відвідану клітинку
                VisitedCells.Add(current);

                // Перевіряємо, чи досягли фінішу
                if (current == finish)
                {
                    return ReconstructPath(previous, start, finish);
                }

                // Отримуємо доступних сусідів
                List<Cell> neighbors = GetPassableNeighbors(maze, current);

                foreach (Cell neighbor in neighbors)
                {
                    // Перевіряємо, чи відвідували цю клітинку
                    if (!previous.ContainsKey(neighbor))
                    {
                        previous[neighbor] = current;
                        stack.Push(neighbor);
                    }
                }
            }

            // Якщо шлях не знайдено
            return new List<Cell>();
        }

        // Пошук сусідніх клітинок, між якими є прохід
        private List<Cell> GetPassableNeighbors(Maze maze, Cell cell)
        {
            List<Cell> neighbors = new List<Cell>();

            int row = cell.Row;
            int column = cell.Column;

            // Перевіряємо клітинку зверху
            if (row > 0 && !cell.TopWall)
            {
                neighbors.Add(maze.Cells[row - 1, column]);
            }

            // Перевіряємо клітинку справа
            if (column < maze.Columns - 1 && !cell.RightWall)
            {
                neighbors.Add(maze.Cells[row, column + 1]);
            }

            // Перевіряємо клітинку знизу
            if (row < maze.Rows - 1 && !cell.BottomWall)
            {
                neighbors.Add(maze.Cells[row + 1, column]);
            }

            // Перевіряємо клітинку зліва
            if (column > 0 && !cell.LeftWall)
            {
                neighbors.Add(maze.Cells[row, column - 1]);
            }

            return neighbors;
        }

        // Відновлення знайденого маршруту
        private List<Cell> ReconstructPath(
            Dictionary<Cell, Cell> previous,
            Cell start,
            Cell finish)
        {
            List<Cell> path = new List<Cell>();

            Cell current = finish;

            // Рухаємося від фінішу до початку
            while (current != start)
            {
                path.Add(current);
                current = previous[current];
            }

            // Додаємо початкову клітинку
            path.Add(start);

            // Розвертаємо маршрут у правильному напрямку
            path.Reverse();

            return path;
        }
    }
}
