
using System;
using System.Collections.Generic;
using MazeGeneratorSolver.MODELS;

namespace MazeGeneratorSolver.SERVICES
{
    public class MazeStatisticsService
    {
        // Обчислення довжини знайденого маршруту
        public string GetPathLength(List<Cell> path)
        {
            if (path.Count == 0)
            {
                return "Not found";
            }

            return (path.Count - 1).ToString();
        }

        // Отримання кількості відвіданих клітинок
        public int GetVisitedCount(List<Cell> visitedCells)
        {
            return visitedCells.Count;
        }

        // Форматування часу виконання алгоритму
        public string FormatExecutionTime(TimeSpan time)
        {
            return time.TotalMilliseconds.ToString("F3") + " ms";
        }
    }
}
