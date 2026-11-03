
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using MazeGeneratorSolver.MODELS;
using MazeGeneratorSolver.ALGORITHMS;
using MazeGeneratorSolver.SERVICES;

namespace MazeGeneratorSolver
{
    public partial class MainWindow : Window
    {
        // Поточний лабіринт
        private Maze? _maze;

        // Об'єкт генератора лабіринту
        private MazeGenerator? _mazeGenerator;

        // Кроки генерації
        private List<MazeStep> _generationSteps = new List<MazeStep>();

        // Поточний крок генерації
        private int _generationIndex = 0;

        // Клітинки, які вже підсвічені під час пошуку
        private HashSet<Cell> _shownVisited = new HashSet<Cell>();

        // Клітинки фінального маршруту
        private HashSet<Cell> _shownPath = new HashSet<Cell>();

        // Послідовність відвіданих клітинок
        private List<Cell> _visitedAnimation = new List<Cell>();

        // Знайдений маршрут
        private List<Cell> _pathAnimation = new List<Cell>();

        // Індекси поточної анімації пошуку
        private int _visitedIndex = 0;
        private int _pathIndex = 0;

        // Таймери анімації
        private DispatcherTimer? _animationTimer;
        private DispatcherTimer? _generationTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Підключення кнопок
            GenerateButton.Click += GenerateButton_Click;
            SolveButton.Click += SolveButton_Click;
            ResetButton.Click += ResetButton_Click;
            CompareButton.Click += CompareButton_Click;
            ExportButton.Click += ExportButton_Click;

            // Підключення регулятора швидкості
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;

            // Початкове значення швидкості
            SpeedValueText.Text = ((int)SpeedSlider.Value).ToString() + "x";

            // Оновлення малюнка при зміні розміру поля
            MazeCanvas.SizeChanged += MazeCanvas_SizeChanged;
        }

        // Створення нового лабіринту
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            StopAnimation();
            StopGenerationAnimation();

            if (SizeComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            // Отримуємо вибраний розмір
            string sizeText = selectedItem.Content?.ToString() ?? "15 x 15";
            int size = int.Parse(sizeText.Split('x')[0].Trim());

            // Створюємо лабіринт
            _maze = new Maze(size, size);

            // Створюємо генератор
            _mazeGenerator = new MazeGenerator();

            // Отримуємо послідовність кроків генерації
            _generationSteps = _mazeGenerator.Generate(_maze);

            // Закриваємо всі стіни для початку анімації
            _mazeGenerator.CloseAllWalls(_maze);

            // Встановлюємо початкову та кінцеву точки
            _maze.Cells[0, 0].IsStart = true;
            _maze.Cells[size - 1, size - 1].IsFinish = true;

            // Очищаємо попередні результати
            ClearSearchResults();

            _generationIndex = 0;

            // Малюємо початковий лабіринт
            DrawMaze();

            // Блокуємо кнопки під час генерації
            GenerateButton.IsEnabled = false;
            SolveButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            CompareButton.IsEnabled = false;
            ExportButton.IsEnabled = false;

            // Запускаємо анімацію генерації
            StartGenerationAnimation();
        }

        // Запуск анімації генерації
        private void StartGenerationAnimation()
        {
            if (_generationSteps.Count == 0)
            {
                FinishGenerationAnimation();
                return;
            }

            _generationTimer = new DispatcherTimer();
            _generationTimer.Interval = TimeSpan.FromMilliseconds(25);

            _generationTimer.Tick += GenerationTimer_Tick;
            _generationTimer.Start();
        }

        // Покрокове створення проходів
        private void GenerationTimer_Tick(object? sender, EventArgs e)
        {
            if (_mazeGenerator == null)
                return;

            // Кількість проходів залежить від швидкості
            int stepsPerTick = (int)SpeedSlider.Value * 2;

            for (int i = 0; i < stepsPerTick &&
                 _generationIndex < _generationSteps.Count; i++)
            {
                _mazeGenerator.ApplyStep(
                    _generationSteps[_generationIndex]);

                _generationIndex++;
            }

            DrawMaze();

            if (_generationIndex >= _generationSteps.Count)
            {
                FinishGenerationAnimation();
            }
        }

        // Завершення генерації
        private void FinishGenerationAnimation()
        {
            StopGenerationAnimation();

            GenerateButton.IsEnabled = true;
            SolveButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            CompareButton.IsEnabled = true;
            ExportButton.IsEnabled = true;

            DrawMaze();
        }

        // Зупинка анімації генерації
        private void StopGenerationAnimation()
        {
            if (_generationTimer != null)
            {
                _generationTimer.Stop();
                _generationTimer.Tick -= GenerationTimer_Tick;
                _generationTimer = null;
            }
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

            Cell start = _maze.Cells[0, 0];
            Cell finish = _maze.Cells[_maze.Rows - 1, _maze.Columns - 1];

            MazeSolver solver = new MazeSolver();

            // Вимірюємо час виконання алгоритму
            Stopwatch stopwatch = Stopwatch.StartNew();

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

            // Створюємо сервіс статистики
            MazeStatisticsService statistics = new MazeStatisticsService();

            // Оновлюємо статистику
            PathLengthText.Text = statistics.GetPathLength(_pathAnimation);

            VisitedCellsText.Text =
                statistics.GetVisitedCount(_visitedAnimation).ToString();

            ExecutionTimeText.Text =
                statistics.FormatExecutionTime(stopwatch.Elapsed);

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

        // Порівняння алгоритмів BFS та DFS
        private void CompareButton_Click(object sender, RoutedEventArgs e)
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

            // Отримуємо початкову та кінцеву клітинки
            Cell start = _maze.Cells[0, 0];
            Cell finish = _maze.Cells[_maze.Rows - 1, _maze.Columns - 1];

            // Запускаємо BFS
            MazeSolver bfsSolver = new MazeSolver();

            Stopwatch bfsStopwatch = Stopwatch.StartNew();

            List<Cell> bfsPath = bfsSolver.FindPath(_maze, start, finish);

            bfsStopwatch.Stop();

            // Запускаємо DFS
            MazeSolver dfsSolver = new MazeSolver();

            Stopwatch dfsStopwatch = Stopwatch.StartNew();

            List<Cell> dfsPath = dfsSolver.FindPathDFS(_maze, start, finish);

            dfsStopwatch.Stop();

            // Створюємо сервіс статистики
            MazeStatisticsService statistics = new MazeStatisticsService();

            int bfsCount = statistics.GetVisitedCount(bfsSolver.VisitedCells);
            int dfsCount = statistics.GetVisitedCount(dfsSolver.VisitedCells);

            // Виводимо результати BFS
            BFSPathText.Text = statistics.GetPathLength(bfsPath);

            BFSVisitedText.Text = bfsCount.ToString();

            BFSTimeText.Text =
                statistics.FormatExecutionTime(bfsStopwatch.Elapsed);

            // Виводимо результати DFS
            DFSPathText.Text = statistics.GetPathLength(dfsPath);

            DFSVisitedText.Text = dfsCount.ToString();

            DFSTimeText.Text =
                statistics.FormatExecutionTime(dfsStopwatch.Elapsed);

            // Визначаємо максимальну кількість відвіданих клітинок
            int maxVisited = Math.Max(bfsCount, dfsCount);

            if (maxVisited == 0)
                maxVisited = 1;

            // Оновлюємо графічні смужки
            ComparisonPanel.UpdateLayout();

            BFSVisitedBar.Width =
                BFSVisitedTrack.ActualWidth * bfsCount / maxVisited;

            DFSVisitedBar.Width =
                DFSVisitedTrack.ActualWidth * dfsCount / maxVisited;

            // Формуємо висновок
            if (bfsCount < dfsCount)
            {
                ComparisonSummaryText.Text =
                    $"BFS visited {dfsCount - bfsCount} fewer cells in this run.";
            }
            else if (dfsCount < bfsCount)
            {
                ComparisonSummaryText.Text =
                    $"DFS visited {bfsCount - dfsCount} fewer cells in this run.";
            }
            else
            {
                ComparisonSummaryText.Text =
                    "Both algorithms visited the same number of cells.";
            }
        }

        // Експорт лабіринту у формат PNG
        private void ExportButton_Click(object sender, RoutedEventArgs e)
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

            // Вікно вибору місця збереження
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export Maze as PNG",
                Filter = "PNG Image (*.png)|*.png",
                DefaultExt = ".png",
                AddExtension = true,
                FileName = $"maze_{_maze.Rows}x{_maze.Columns}.png"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                // Створюємо об'єкт сервісу експорту
                MazeExportService exportService = new MazeExportService();

                // Зберігаємо лабіринт у PNG
                exportService.ExportToPng(MazeCanvas, dialog.FileName);

                MessageBox.Show(
                    "Лабіринт успішно експортовано!",
                    "Export PNG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка експорту: " + ex.Message,
                    "Export PNG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Запуск анімації пошуку
        private void StartAnimation()
        {
            _visitedIndex = 0;
            _pathIndex = 0;

            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(30);

            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        // Послідовне підсвічування клітинок
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
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

            StopAnimation();
        }

        // Оновлення тексту швидкості
        private void SpeedSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            SpeedValueText.Text = ((int)SpeedSlider.Value).ToString() + "x";
        }

        // Зупинка анімації пошуку
        private void StopAnimation()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer = null;
            }
        }

        // Очищення результатів пошуку
        private void ClearSearchResults()
        {
            _shownVisited.Clear();
            _shownPath.Clear();

            _visitedAnimation.Clear();
            _pathAnimation.Clear();

            _visitedIndex = 0;
            _pathIndex = 0;

            // Очищення основної статистики
            PathLengthText.Text = "—";
            VisitedCellsText.Text = "—";
            ExecutionTimeText.Text = "—";

            // Очищення результатів BFS
            BFSPathText.Text = "—";
            BFSVisitedText.Text = "—";
            BFSTimeText.Text = "—";

            // Очищення результатів DFS
            DFSPathText.Text = "—";
            DFSVisitedText.Text = "—";
            DFSTimeText.Text = "—";

            // Очищення графічних смужок
            BFSVisitedBar.Width = 0;
            DFSVisitedBar.Width = 0;

            // Очищення висновку
            ComparisonSummaryText.Text =
                "Generate a maze and compare algorithms.";
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
                DrawMaze();
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

                    Rectangle rectangle = new Rectangle
                    {
                        Width = cellWidth,
                        Height = cellHeight
                    };

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

                    if (cell.TopWall)
                        DrawWall(x, y, x + cellWidth, y);

                    if (cell.LeftWall)
                        DrawWall(x, y, x, y + cellHeight);

                    if (row == _maze.Rows - 1 && cell.BottomWall)
                    {
                        DrawWall(x, y + cellHeight,
                                 x + cellWidth, y + cellHeight);
                    }

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

            line.Stroke = new SolidColorBrush(
                Color.FromRgb(148, 163, 184));

            line.StrokeThickness = 2.5;
            line.SnapsToDevicePixels = true;

            MazeCanvas.Children.Add(line);
        }
    }
}
