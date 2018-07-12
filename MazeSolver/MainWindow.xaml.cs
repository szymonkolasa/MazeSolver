using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MazeSolver
{
    public partial class MainWindow : Window
    {
        private string _controlPrefix = "border";
        private Brush _controlBorder = Brushes.Black;
        private int _controlThickness = 1;

        private Cell[,] _cells;

        private Stack<Cell> _stack = new Stack<Cell>();
        private Cell _current;
        private Random _rand = new Random();

        private Agent _agent = new Agent();
        private int _e = 10;
        private double _a = 0.2;
        private double _y = 0.9;
        private double _moveCost = 0.04; // 0.04 by default
        private HashSet<Cell> _shortestPath;

        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,0,1);
            timer.Tick += FindPath;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            MazeGrid.Children.Clear();
            MazeGrid.ColumnDefinitions.Clear();
            MazeGrid.RowDefinitions.Clear();
            _agent = new Agent();
            GC.Collect();

            if (int.TryParse(XSize.Text, out int x) && int.TryParse(YSize.Text, out int y))
            {
                _cells = new Cell[x, y];

                GenerateGrid(x, y);

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        var border = new Border();
                        border.Name = _controlPrefix + $"{i}_{j}";
                        border.BorderThickness = new Thickness(_controlThickness);
                        border.BorderBrush = _controlBorder;

                        MazeGrid.Children.Add(border);
                        Grid.SetColumn(border, i);
                        Grid.SetRow(border, j);

                        var cell = new Cell() { X = i, Y = j };
                        _cells[i, j] = cell;
                    }
                }

                AddNeighbours();
                GenerateMaze();
            }
            else
            {
                MessageBox.Show("Rozmiar labiryntu musi być liczbą całkowitą", "Błąd parsowania", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            }
        }

        private void GenerateMaze_Click(object sender, RoutedEventArgs e)
        {
            //var border = (Border)MazeGrid.FindName("border00");
            //var border = (Border) System.Windows.LogicalTreeHelper.FindLogicalNode(MazeGrid, "border00");
            //border.Background = Brushes.Red;

            if (_current is null)
            {
                _current = _cells[0, 0];
                _current.IsVisited = true;
            }

            while (HasUnvisited())
            {
                var unvisited = _current.Neighbours
                .Where(x => !x.IsVisited);

                if (unvisited.Count() > 0)
                {
                    var next = unvisited.ElementAt(_rand.Next(unvisited.Count()));
                    var cell = _cells[next.X, next.Y];

                    var currentControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{_current.X}_{_current.Y}");
                    currentControl.Background = Brushes.AliceBlue;

                    var nextControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{next.X}_{next.Y}");

                    var currentThickness = currentControl.BorderThickness;
                    var nextThickness = nextControl.BorderThickness;

                    if (_current.X - next.X < 0)
                    {
                        currentControl.BorderThickness = new Thickness(currentThickness.Left, currentThickness.Top, 0, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(0, nextThickness.Top, nextThickness.Right, nextThickness.Bottom);
                    }

                    if (_current.X - next.X > 0)
                    {
                        currentControl.BorderThickness = new Thickness(0, currentThickness.Top, currentThickness.Right, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, nextThickness.Top, 0, nextThickness.Bottom);
                    }

                    if (_current.Y - next.Y < 0)
                    {
                        currentControl.BorderThickness = new Thickness(currentThickness.Left, currentThickness.Top, currentThickness.Right, 0);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, 0, nextThickness.Right, nextThickness.Bottom);
                    }

                    if (_current.Y - next.Y > 0)
                    {
                        currentControl.BorderThickness = new Thickness(currentThickness.Left, 0, currentThickness.Right, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, nextThickness.Top, nextThickness.Right, 0);
                    }

                    _stack.Push(_current);
                    currentControl.Background = Brushes.White;

                    _current = next;
                    _current.IsVisited = true;
                }
                else
                {
                    _current = _stack.Pop();
                }
            }
        }

        private void AddNeighbours()
        {
            var x = _cells.GetLength(0);
            var y = _cells.GetLength(1);

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    var cell = _cells[i, j];

                    if (i > 0)
                        cell.Neighbours.Add(_cells[i - 1, j]);

                    if (i < x-1)
                        cell.Neighbours.Add(_cells[i + 1, j]);

                    if (j > 0)
                        cell.Neighbours.Add(_cells[i, j - 1]);

                    if (j < y-1)
                        cell.Neighbours.Add(_cells[i, j + 1]);
                }
            }
        }

        private void GenerateGrid(int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                MazeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < y; i++)
            {
                MazeGrid.RowDefinitions.Add(new RowDefinition());
            }
        }

        //private void GenerateMaze()
        //{
        //    Stack<Cell> stack = new Stack<Cell>();
        //    Random rand = new Random();
        //    Cell current = _cells[0, 0];

        //    current.IsVisited = true;

        //    while (HasUnvisited())
        //    {
        //        var unvisitedNeighbours = current.Neighbours
        //            .Where(x => !x.IsVisited);

        //        var unvisitedCount = unvisitedNeighbours.Count();

        //        if (unvisitedCount > 0)
        //        {
        //            var next = unvisitedNeighbours.ElementAt(rand.Next(unvisitedCount));
        //            var nextCell = _cells[next.X, next.Y];

        //            var currentControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{current.X}{current.Y}");
        //            var currentControlThickness = currentControl.BorderThickness;

        //            var nextControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{next.X}{next.Y}");
        //            var nextControlThickness = nextControl.BorderThickness;

        //            if (current.X - next.X < 0)
        //            {
        //                current.EastWall = false;
        //                next.WestWall = false;

        //                currentControl.BorderThickness = new Thickness(currentControlThickness.Left, currentControlThickness.Top, 0, currentControlThickness.Bottom);
        //                nextControl.BorderThickness = new Thickness(0, nextControlThickness.Top, nextControlThickness.Right, nextControlThickness.Bottom);
        //            }

        //            if (current.X - next.X > 0)
        //            {
        //                current.WestWall = false;
        //                next.EastWall = false;

        //                currentControl.BorderThickness = new Thickness(0, currentControlThickness.Top, currentControlThickness.Right, currentControlThickness.Bottom);
        //                nextControl.BorderThickness = new Thickness(nextControlThickness.Left, nextControlThickness.Top, 0, nextControlThickness.Bottom);
        //            }

        //            if (current.Y - next.Y < 0)
        //            {
        //                current.SouthWall = false;
        //                next.NorthWall = false;

        //                currentControl.BorderThickness = new Thickness(currentControlThickness.Left, currentControlThickness.Top, currentControlThickness.Right, 0);
        //                nextControl.BorderThickness = new Thickness(nextControlThickness.Left, 0, nextControlThickness.Right, nextControlThickness.Bottom);
        //            }

        //            if (current.Y - next.Y > 0)
        //            {
        //                current.NorthWall = false;
        //                next.SouthWall = false;

        //                currentControl.BorderThickness = new Thickness(currentControlThickness.Left, 0, currentControlThickness.Right, currentControlThickness.Bottom);
        //                nextControl.BorderThickness = new Thickness(nextControlThickness.Left, nextControlThickness.Top, nextControlThickness.Right, 0);
        //            }

        //            stack.Push(current);

        //            current = next;
        //            current.IsVisited = true;

        //            Thread.Sleep(1000);
        //        }
        //        else
        //        {
        //            current = stack.Pop();
        //        }
        //    }
        //}

        private void GenerateMaze()
        {
            Stack<Cell> stack = new Stack<Cell>();
            Cell current = _cells[0, 0];
            current.IsVisited = true;

            while (HasUnvisited())
            {
                var unvisited = current.Neighbours
                .Where(x => !x.IsVisited);

                if (unvisited.Count() > 0)
                {
                    var next = unvisited.ElementAt(_rand.Next(unvisited.Count()));
                    var cell = _cells[next.X, next.Y];

                    var currentControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{current.X}_{current.Y}");
                    currentControl.Background = Brushes.AliceBlue;

                    var nextControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{next.X}_{next.Y}");

                    var currentThickness = currentControl.BorderThickness;
                    var nextThickness = nextControl.BorderThickness;

                    if (current.X - next.X < 0)
                    {
                        current.EastWall = false;
                        next.WestWall = false;

                        currentControl.BorderThickness = new Thickness(currentThickness.Left, currentThickness.Top, 0, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(0, nextThickness.Top, nextThickness.Right, nextThickness.Bottom);
                    }

                    if (current.X - next.X > 0)
                    {
                        current.WestWall = false;
                        next.EastWall = false;

                        currentControl.BorderThickness = new Thickness(0, currentThickness.Top, currentThickness.Right, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, nextThickness.Top, 0, nextThickness.Bottom);
                    }

                    if (current.Y - next.Y < 0)
                    {
                        current.SouthWall = false;
                        next.NorthWall = false;

                        currentControl.BorderThickness = new Thickness(currentThickness.Left, currentThickness.Top, currentThickness.Right, 0);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, 0, nextThickness.Right, nextThickness.Bottom);
                    }

                    if (current.Y - next.Y > 0)
                    {
                        current.NorthWall = false;
                        next.SouthWall = false;

                        currentControl.BorderThickness = new Thickness(currentThickness.Left, 0, currentThickness.Right, currentThickness.Bottom);
                        nextControl.BorderThickness = new Thickness(nextThickness.Left, nextThickness.Top, nextThickness.Right, 0);
                    }

                    stack.Push(current);
                    currentControl.Background = Brushes.White;

                    current = next;
                    current.IsVisited = true;
                }
                else
                {
                    current = stack.Pop();
                }
            }
        }

        private bool HasUnvisited()
        {
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    var cell = _cells[i, j];

                    if (!cell.IsVisited)
                        return true;
                }
            }

            return false;
        }

        private void FindPath_Click(object sender, RoutedEventArgs e)
        {
            //var x = MazeGrid.ColumnDefinitions.Count - 1;
            //var y = MazeGrid.RowDefinitions.Count - 1;

            //var startControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}0_0");
            //startControl.Background = Brushes.Green;

            //var endControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{x}_{y}");
            //endControl.Background = Brushes.Red;

            if (!timer.IsEnabled)
            {
                if (_agent.CurrentCell is null)
                {
                    _agent.CurrentCell = _cells[0, 0];
                    var x = MazeGrid.ColumnDefinitions.Count;
                    var y = MazeGrid.RowDefinitions.Count;

                    _cells[x - 1, y - 1].Reward = x * y;

                    var startControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}0_0");
                    startControl.Background = Brushes.LightGreen;

                    var endControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{x - 1}_{y - 1}");
                    endControl.Background = Brushes.IndianRed;
                }

                timer.Start();
                findPathButton.Content = "Zatrzymaj";
            }
            else
            {
                timer.Stop();
                findPathButton.Content = "Znajdź drogę";
            }
        }

        private void FindPath(object sender, EventArgs e)
        {
            var x = MazeGrid.ColumnDefinitions.Count - 1;
            var y = MazeGrid.RowDefinitions.Count - 1;
            Cell nextCell;

            var currentControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{_agent.CurrentCell.X}_{_agent.CurrentCell.Y}");

            if (_agent.CurrentCell != _cells[0, 0] && _agent.CurrentCell != _cells[x, y])
                currentControl.Background = Brushes.Aqua;

            var rand = _rand.Next(100);

            int direction = 0;

            if (rand < _e)
                direction = _rand.Next(4);
            else
                direction = _agent.CurrentCell
                    .QValues.IndexOf(_agent.CurrentCell.QValues.Max());

            Update(_agent.CurrentCell, direction, out nextCell);

            //if (_agent.CurrentCell != _cells[0, 0] && _agent.CurrentCell != _cells[x, y])
            //    currentControl.Background = Brushes.White;

            if (nextCell == _cells[x, y])
            {
                nextCell = _cells[0, 0];

                for (int i = 0; i <= x; i++)
                {
                    for (int j = 0; j <= y; j++)
                    {
                        var control = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{i}_{j}");
                        control.Background = Brushes.White;
                    }
                }

                var startControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}0_0");
                startControl.Background = Brushes.LightGreen;

                var endControl = (Border)LogicalTreeHelper.FindLogicalNode(MazeGrid, $"{_controlPrefix}{x}_{y}");
                endControl.Background = Brushes.IndianRed;
            }

            _agent.CurrentCell = nextCell;
        }

        private void Update(Cell current, int direction, out Cell next)
        {
            Cell nextCell = null;

            // Move Top
            if (direction == 0)
            {
                if (current.NorthWall)
                    nextCell = _agent.CurrentCell;
                else
                    nextCell = _cells[current.X, current.Y - 1];
            }

            // Move right
            if (direction == 1)
            {
                if (current.EastWall)
                    nextCell = _agent.CurrentCell;
                else
                    nextCell = _cells[current.X + 1, current.Y];
            }

            // Move bottom
            if (direction == 2)
            {
                if (current.SouthWall)
                    nextCell = _agent.CurrentCell;
                else
                    nextCell = _cells[current.X, current.Y + 1];
            }

            if (direction == 3)
            {
                if (current.WestWall)
                    nextCell = _agent.CurrentCell;
                else
                    nextCell = _cells[current.X - 1, current.Y];
            }

            var wallPenalty = 0.0;

            if (current == nextCell)
                wallPenalty = 20;

            var r = nextCell.Reward;
            var max = nextCell.QValues.Max();
            var actual = current.QValues[direction];

            current.QValues[direction] += _a * (r + _y * max - actual) - _moveCost - wallPenalty;

            next = nextCell;
        }
    }
}
