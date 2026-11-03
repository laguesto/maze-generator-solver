
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MazeGeneratorSolver.MODELS;
using MazeGeneratorSolver.ALGORITHMS;

namespace MazeGeneratorSolver
{
    public partial class MainWindow : Window
    {
        // Поточний лабіринт
        private Maze? _maze;

        // Клітинки, які вже підсвічені під час пошуку
        private HashSet<Cell> _shownVisited = new HashSet<Cell>();

        // Клітинки фінального маршруту
        private HashSet<Cell> _shownPath = new HashSet<Cell>();

        // Послідовність відвіданих клітинок
        private List<Cell> _visitedAnimation = new List<Cell>();

        // Знайдений маршрут
        private List<Cell> _pathAnimation = new List<Cell>();

        // Індекси поточної анімації
        private int _visitedIndex = 0;
        private int _pathIndex = 0;

        // Таймер анімації
        private DispatcherTimer? _animationTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Підключення кнопок
            GenerateButton.Click += GenerateButton_Click;
            SolveButton.Click += SolveButton_Click;
            ResetButton.Click += ResetButton_Click;

            // Підключення регулятора швидкості
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;

            // Встановлення початкового значення швидкості
            SpeedValueText.Text = ((int)SpeedSlider.Value).ToString() + "x";

            // Оновлення малюнка при зміні розміру поля
            MazeCanvas.SizeChanged += MazeCanvas_SizeChanged;
        }

        // Створення нового лабіринту
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            StopAnimation();

            if (SizeComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            // Отримуємо вибраний розмір
            string sizeText = selectedItem.Content?.ToString() ?? "15 x 15";
            int size = int.Parse(sizeText.Split('x')[0].Trim());

            // Створюємо лабіринт
            _maze = new Maze(size, size);

            // Запускаємо генерацію
            MazeGenerator generator = new MazeGenerator();
            generator.Generate(_maze);

            // Встановлюємо початкову та кінцеву точки
            _maze.Cells[0, 0].IsStart = true;
            _maze.Cells[size - 1, size - 1].IsFinish = true;

            // Очищаємо попередні результати
            ClearSearchResults();

            // Малюємо новий лабіринт
            DrawMaze();
        }

        // Пошук виходу з лабіринту
        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_maze == null)
            {
                MessageBox.Show(
                    "Спочатку згенеруйте лабіринт.",
                    "Maze Generator",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            StopAnimation();
            ClearSearchResults();

            // Отримуємо початкову та кінцеву клітинки
            Cell start = _maze.Cells[0, 0];
            Cell finish = _maze.Cells[_maze.Rows - 1, _maze.Columns - 1];

            MazeSolver solver = new MazeSolver();

            // Вимірюємо час виконання алгоритму
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Отримуємо вибраний алгоритм
            ComboBoxItem? selectedAlgorithm =
                AlgorithmComboBox.SelectedItem as ComboBoxItem;

            string algorithm = selectedAlgorithm?.Content?.ToString() ?? "BFS";

            // Запускаємо вибраний алгоритм
            if (algorithm == "DFS")
            {
                _pathAnimation = solver.FindPathDFS(_maze, start, finish);
            }
            else
            {
                _pathAnimation = solver.FindPath(_maze, start, finish);
            }

            stopwatch.Stop();

            // Зберігаємо порядок відвіданих клітинок
            _visitedAnimation = new List<Cell>(solver.VisitedCells);

            // Оновлюємо статистику
            if (_pathAnimation.Count > 0)
            {
                PathLengthText.Text = (_pathAnimation.Count - 1).ToString();
            }
            else
            {
                PathLengthText.Text = "Not found";
            }

            VisitedCellsText.Text = _visitedAnimation.Count.ToString();

            ExecutionTimeText.Text =
                stopwatch.Elapsed.TotalMilliseconds.ToString("F2") + " ms";

            // Запускаємо анімацію пошуку
            if (_visitedAnimation.Count > 0)
            {
                StartAnimation();
            }
            else
            {
                DrawMaze();
            }
        }

        // Запуск анімації
        private void StartAnimation()
        {
            _visitedIndex = 0;
            _pathIndex = 0;

            _animationTimer = new DispatcherTimer();

            // Інтервал між оновленнями анімації
            _animationTimer.Interval = TimeSpan.FromMilliseconds(30);

            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        // Послідовне підсвічування клітинок
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Кількість клітинок залежить від вибраної швидкості
            int cellsPerTick = (int)SpeedSlider.Value;

            // Спочатку показуємо процес пошуку
            if (_visitedIndex < _visitedAnimation.Count)
            {
                for (int i = 0; i < cellsPerTick &&
                     _visitedIndex < _visitedAnimation.Count; i++)
                {
                    _shownVisited.Add(_visitedAnimation[_visitedIndex]);
                    _visitedIndex++;
                }

                DrawMaze();
                return;
            }

            // Потім показуємо фінальний маршрут
            if (_pathIndex < _pathAnimation.Count)
            {
                for (int i = 0; i < cellsPerTick &&
                     _pathIndex < _pathAnimation.Count; i++)
                {
                    _shownPath.Add(_pathAnimation[_pathIndex]);
                    _pathIndex++;
                }

                DrawMaze();
                return;
            }

            // Завершуємо анімацію
            StopAnimation();
        }

        // Оновлення швидкості анімації
        private void SpeedSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            SpeedValueText.Text = ((int)SpeedSlider.Value).ToString() + "x";
        }

        // Зупинка анімації
        private void StopAnimation()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer = null;
            }
        }

        // Очищення результатів попереднього пошуку
        private void ClearSearchResults()
        {
            _shownVisited.Clear();
            _shownPath.Clear();

            _visitedAnimation.Clear();
            _pathAnimation.Clear();

            _visitedIndex = 0;
            _pathIndex = 0;

            PathLengthText.Text = "—";
            VisitedCellsText.Text = "—";
            ExecutionTimeText.Text = "—";
        }

        // Скидання результатів
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            StopAnimation();
            ClearSearchResults();

            DrawMaze();
        }

        // Оновлення малюнка при зміні розміру поля
        private void MazeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_maze != null)
            {
                DrawMaze();
            }
        }

        // Малювання лабіринту
        private void DrawMaze()
        {
            if (_maze == null)
                return;

            double cellWidth = MazeCanvas.ActualWidth / _maze.Columns;
            double cellHeight = MazeCanvas.ActualHeight / _maze.Rows;

            if (cellWidth <= 0 || cellHeight <= 0)
                return;

            MazeCanvas.Children.Clear();

            // Малюємо клітинки
            for (int row = 0; row < _maze.Rows; row++)
            {
                for (int column = 0; column < _maze.Columns; column++)
                {
                    Cell cell = _maze.Cells[row, column];

                    Rectangle rectangle = new Rectangle();

                    rectangle.Width = cellWidth;
                    rectangle.Height = cellHeight;

                    // Визначаємо колір клітинки
                    if (cell.IsStart)
                    {
                        rectangle.Fill = new SolidColorBrush(
                            Color.FromRgb(34, 197, 94));
                    }
                    else if (cell.IsFinish)
                    {
                        rectangle.Fill = new SolidColorBrush(
                            Color.FromRgb(239, 68, 68));
                    }
                    else if (_shownPath.Contains(cell))
                    {
                        rectangle.Fill = new SolidColorBrush(
                            Color.FromRgb(59, 130, 246));
                    }
                    else if (_shownVisited.Contains(cell))
                    {
                        rectangle.Fill = new SolidColorBrush(
                            Color.FromRgb(71, 85, 105));
                    }
                    else
                    {
                        rectangle.Fill = new SolidColorBrush(
                            Color.FromRgb(17, 24, 39));
                    }

                    Canvas.SetLeft(rectangle, column * cellWidth);
                    Canvas.SetTop(rectangle, row * cellHeight);

                    MazeCanvas.Children.Add(rectangle);
                }
            }

            // Малюємо стіни клітинок
            for (int row = 0; row < _maze.Rows; row++)
            {
                for (int column = 0; column < _maze.Columns; column++)
                {
                    Cell cell = _maze.Cells[row, column];

                    double x = column * cellWidth;
                    double y = row * cellHeight;

                    // Верхня стіна
                    if (cell.TopWall)
                    {
                        DrawWall(x, y, x + cellWidth, y);
                    }

                    // Ліва стіна
                    if (cell.LeftWall)
                    {
                        DrawWall(x, y, x, y + cellHeight);
                    }

                    // Нижня зовнішня стіна
                    if (row == _maze.Rows - 1 && cell.BottomWall)
                    {
                        DrawWall(x, y + cellHeight,
                                 x + cellWidth, y + cellHeight);
                    }

                    // Права зовнішня стіна
                    if (column == _maze.Columns - 1 && cell.RightWall)
                    {
                        DrawWall(x + cellWidth, y,
                                 x + cellWidth, y + cellHeight);
                    }
                }
            }
        }

        // Малювання окремої стіни
        private void DrawWall(double x1, double y1, double x2, double y2)
        {
            Line line = new Line();

            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;

            // Колір та товщина стін
            line.Stroke = new SolidColorBrush(
                Color.FromRgb(148, 163, 184));

            line.StrokeThickness = 2.5;
            line.SnapsToDevicePixels = true;

            MazeCanvas.Children.Add(line);
        }
    }
}
