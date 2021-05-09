using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{
    public partial class MainWindow
    {
        private const int CellSize = 10;
        private const int SpaceSize = 2;
        private const int CanvasSize = 60;

        private static readonly Brush Dead = Brushes.LightGray;
        private static readonly Brush Alive = Brushes.Red;

        private int _generationsNo;

        public MainWindow()
        {
            InitializeComponent();
            AliveCells.Content = 0;
            IterationNo.Content = 0;
            InitializeUniverseCanvas();
        }

        private int CurrentAliveCells
        {
            get { return Canvas.Children.OfType<Rectangle>().Count(r => r.Fill == Alive); }
        }

        private void InitializeUniverseCanvas()
        {
            for (var i = 0; i < CanvasSize; i++)
            for (var j = 0; j < CanvasSize; j++)
            {
                var rectangle = new Rectangle
                {
                    Height = CellSize,
                    Width = CellSize,
                    Fill = Dead
                };

                Canvas.Children.Add(rectangle);

                Canvas.SetLeft(rectangle, i * (CellSize + SpaceSize));
                Canvas.SetTop(rectangle, j * (CellSize + SpaceSize));
            }
        }

        private void NextGeneration(object sender, RoutedEventArgs e)
        {
            IterationNo.Content = ++_generationsNo;

            var canvasList = new List<Rectangle>(Canvas.Children.OfType<Rectangle>());

            var currentGeneration = canvasList.Select(r => r.Fill == Alive).ToList();
            var nextGeneration = canvasList.Select(_ => false).ToList();

            for (var i = 0; i < CanvasSize; i++)
            for (var j = 0; j < CanvasSize; j++)
            {
                var currentIndex = i * CanvasSize + j;
                var current = currentGeneration[currentIndex];

                var aliveNeighbours = 0;

                var northNeighbour = false;
                var northWestNeighbour = false;
                var northEastNeighbour = false;
                var southNeighbour = false;
                var southWestNeighbour = false;
                var southEastNeighbour = false;
                var westNeighbour = false;
                var eastNeighbour = false;

                // Calculate neighbours
                if (j - 1 >= 0) northNeighbour = currentGeneration[i * CanvasSize + (j - 1)];
                if (j + 1 < CanvasSize) southNeighbour = currentGeneration[i * CanvasSize + j + 1];
                if (i - 1 >= 0) westNeighbour = currentGeneration[(i - 1) * CanvasSize + j];
                if (i + 1 < CanvasSize) eastNeighbour = currentGeneration[(i + 1) * CanvasSize + j];
                if (j - 1 >= 0 && i - 1 >= 0) northWestNeighbour = currentGeneration[(i - 1) * CanvasSize + (j - 1)];
                if (j - 1 >= 0 && i + 1 < CanvasSize)
                    northEastNeighbour = currentGeneration[(i + 1) * CanvasSize + (j - 1)];
                if (j + 1 < CanvasSize && i - 1 >= 0)
                    southWestNeighbour = currentGeneration[(i - 1) * CanvasSize + j + 1];
                if (j + 1 < CanvasSize && i + 1 < CanvasSize)
                    southEastNeighbour = currentGeneration[(i + 1) * CanvasSize + j + 1];

                // Calculate alive neighbours
                if (northNeighbour) aliveNeighbours++;
                if (southNeighbour) aliveNeighbours++;
                if (westNeighbour) aliveNeighbours++;
                if (eastNeighbour) aliveNeighbours++;
                if (northWestNeighbour) aliveNeighbours++;
                if (northEastNeighbour) aliveNeighbours++;
                if (southWestNeighbour) aliveNeighbours++;
                if (southEastNeighbour) aliveNeighbours++;

                // Calculate next generation
                if (current && aliveNeighbours < 2) nextGeneration[currentIndex] = false;
                else if (current && aliveNeighbours > 3) nextGeneration[currentIndex] = false;
                else if (current == false && aliveNeighbours == 3) nextGeneration[currentIndex] = true;
                else nextGeneration[currentIndex] = current;
            }

            // Render new generation
            for (var i = 0; i < CanvasSize; i++)
            for (var j = 0; j < CanvasSize; j++)
            {
                var currentIndex = i * CanvasSize + j;
                canvasList[currentIndex].Fill = nextGeneration[currentIndex] ? Alive : Dead;
            }

            AliveCells.Content = CurrentAliveCells;
        }

        private void GiveOrTakeLife(object sender, MouseEventArgs e)
        {
            if (sender is not Canvas canvas) return;
            var element = VisualTreeHelper.HitTest(canvas, e.GetPosition(canvas)).VisualHit;
            if (element is not Rectangle rectangle) return;

            if (e.LeftButton == MouseButtonState.Pressed) rectangle.Fill = Alive;
            if (e.RightButton == MouseButtonState.Pressed) rectangle.Fill = Dead;

            AliveCells.Content = CurrentAliveCells;
        }

        private void ResetGame(object sender, RoutedEventArgs e)
        {
            IterationNo.Content = _generationsNo = 0;
            AliveCells.Content = 0;

            var aliveCells = Canvas.Children
                .OfType<Rectangle>()
                .Where(r => r.Fill == Alive);

            foreach (var rectangle in aliveCells)
                rectangle.Fill = Dead;
        }
    }
}