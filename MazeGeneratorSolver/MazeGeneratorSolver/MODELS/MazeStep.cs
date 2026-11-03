namespace MazeGeneratorSolver.MODELS
{
    public class MazeStep
    {
        // Поточна та наступна клітинки
        public Cell Current { get; }
        public Cell Next { get; }

        // Конструктор кроку генерації
        public MazeStep(Cell current, Cell next)
        {
            Current = current;
            Next = next;
        }
    }
}
