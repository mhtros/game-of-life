using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
        private const int CanvasSize = 50;

        private static readonly Brush Alive = Brushes.Red;
        private static readonly Brush Dead = Brushes.LightGray;

        private TimeSpan _simulationDelay;

        #region Binding Properties

        public static readonly DependencyProperty GenerationNumberProperty = DependencyProperty
            .Register(nameof(GenerationNumber), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int GenerationNumber
        {
            get => (int)GetValue(GenerationNumberProperty);
            set => SetValue(GenerationNumberProperty, value);
        }

        public static readonly DependencyProperty IsSimulationStoppedProperty = DependencyProperty
            .Register(nameof(IsSimulationStopped), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public bool IsSimulationStopped
        {
            get => (bool)GetValue(IsSimulationStoppedProperty);
            set => SetValue(IsSimulationStoppedProperty, value);
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas();
        }

        private void AliveNumberChanged(object sender, EventArgs e) => AliveCellsLabel.Content =
            Canvas.Children.OfType<Rectangle>().Count(r => r.Fill == Alive);

        private void InitializeCanvas()
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

                // Register change color event on each rectangle
                DependencyPropertyDescriptor.FromProperty(Shape.FillProperty, typeof(Shape))
                    .AddValueChanged(rectangle, AliveNumberChanged);

                Canvas.Children.Add(rectangle);

                Canvas.SetLeft(rectangle, i * (CellSize + SpaceSize));
                Canvas.SetTop(rectangle, j * (CellSize + SpaceSize));
            }
        }

        private void NextGeneration(object sender, RoutedEventArgs e) => DrawNextGeneration();

        private void ModifySpeed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newSimulationDelay = SpeedSlider.Maximum + SpeedSlider.Minimum - e.NewValue;
            _simulationDelay = TimeSpan.FromMilliseconds(newSimulationDelay);
        }

        private void DrawNextGeneration()
        {
            GenerationNumber++;

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
        }

        private async void ToggleSimulation(object sender, RoutedEventArgs e)
        {
            IsSimulationStopped = !IsSimulationStopped;
            ToggleSimulationBtn.Content = IsSimulationStopped ? "Start" : "Stop";

            while (IsSimulationStopped == false)
            {
                await Task.Run(async () => await Dispatcher.InvokeAsync(DrawNextGeneration));
                await Task.Delay(_simulationDelay);
            }
        }

        private void GiveOrTakeLife(object sender, MouseEventArgs e)
        {
            if (sender is not Canvas canvas) return;
            var element = VisualTreeHelper.HitTest(canvas, e.GetPosition(canvas)).VisualHit;
            if (element is not Rectangle rectangle) return;

            if (e.LeftButton == MouseButtonState.Pressed) rectangle.Fill = Alive;
            if (e.RightButton == MouseButtonState.Pressed) rectangle.Fill = Dead;
        }

        private void ResetSimulation(object sender, RoutedEventArgs e)
        {
            GenerationNumber = 0;

            var aliveCells = Canvas.Children
                .OfType<Rectangle>()
                .Where(r => r.Fill == Alive);

            foreach (var rectangle in aliveCells)
                rectangle.Fill = Dead;
        }
    }
}