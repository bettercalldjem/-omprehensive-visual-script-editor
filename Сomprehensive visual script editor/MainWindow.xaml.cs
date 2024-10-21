using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptEditor
{
    public partial class MainWindow : Window
    {
        private bool _isDragging;
        private Point _dragStartPoint;
        private Rectangle _selectedBlock;
        private bool _isConnecting;
        private List<Line> _lines;
        private Dictionary<string, object> _variables;

        public MainWindow()
        {
            InitializeComponent();
            _lines = new List<Line>();
            _variables = new Dictionary<string, object>();
        }

        private void AddBlock_Click(object sender, RoutedEventArgs e)
        {
            string blockType = (string)((Button)sender).CommandParameter;
            AddBlock(blockType);
        }

        private void AddBlock(string blockName)
        {
            var block = new Rectangle
            {
                Width = 100,
                Height = 50,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Tag = blockName
            };

            var textBlock = new TextBlock
            {
                Text = blockName,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            double blockY = 50 * (ScriptCanvas.Children.Count / 2 + 1);
            Canvas.SetLeft(block, 50);
            Canvas.SetTop(block, blockY);
            ScriptCanvas.Children.Add(block);

            Canvas.SetLeft(textBlock, 50 + 20);
            Canvas.SetTop(textBlock, blockY + 15);
            ScriptCanvas.Children.Add(textBlock);

            block.MouseMove += Block_MouseMove;
            block.MouseUp += Block_MouseUp;
            block.MouseDown += Block_MouseDown;

            if (blockName == "Ввод переменной")
            {
                CreateInputVariableBlock(block);
            }
        }

        private void CreateInputVariableBlock(Rectangle block)
        {
            var inputTextBox = new TextBox
            {
                Width = 80,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Canvas.SetLeft(inputTextBox, Canvas.GetLeft(block) + 10);
            Canvas.SetTop(inputTextBox, Canvas.GetTop(block) + 60);
            ScriptCanvas.Children.Add(inputTextBox);

            block.Tag = inputTextBox;
        }

        private void Block_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle block)
            {
                if (_isConnecting)
                {
                    if (_selectedBlock == null)
                    {
                        _selectedBlock = block;
                    }
                    else
                    {
                        DrawLine(_selectedBlock, block);
                        _selectedBlock = null;
                        _isConnecting = false;
                    }
                }
                else
                {
                    _selectedBlock = block;
                }
            }
        }

        private void DrawLine(Rectangle fromBlock, Rectangle toBlock)
        {
            var line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            double fromX = Canvas.GetLeft(fromBlock) + fromBlock.Width / 2;
            double fromY = Canvas.GetTop(fromBlock) + fromBlock.Height;
            double toX = Canvas.GetLeft(toBlock) + toBlock.Width / 2;
            double toY = Canvas.GetTop(toBlock);

            line.X1 = fromX;
            line.Y1 = fromY;
            line.X2 = toX;
            line.Y2 = toY;

            ScriptCanvas.Children.Add(line);
            _lines.Add(line);
        }

        private void Block_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Rectangle block)
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                    _dragStartPoint = e.GetPosition(ScriptCanvas);
                    Mouse.Capture(block);
                }

                var currentPoint = e.GetPosition(ScriptCanvas);
                double offsetX = currentPoint.X - _dragStartPoint.X;
                double offsetY = currentPoint.Y - _dragStartPoint.Y;

                double newLeft = Canvas.GetLeft(block) + offsetX;
                double newTop = Canvas.GetTop(block) + offsetY;

                Canvas.SetLeft(block, newLeft);
                Canvas.SetTop(block, newTop);

                foreach (var child in ScriptCanvas.Children)
                {
                    if (child is TextBlock textBlock && textBlock.Text == block.Tag.ToString())
                    {
                        Canvas.SetLeft(textBlock, newLeft + 20);
                        Canvas.SetTop(textBlock, newTop + 15);
                        break;
                    }
                }

                _dragStartPoint = currentPoint;
            }
        }

        private void Block_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
        }

        private void AddConnection_Click(object sender, RoutedEventArgs e)
        {
            _isConnecting = true;
            _selectedBlock = null;
        }

        private void RemoveSelectedBlock_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBlock != null)
            {
                foreach (var line in _lines)
                {
                    if (line.X1 == Canvas.GetLeft(_selectedBlock) + _selectedBlock.Width / 2 &&
                        line.Y1 == Canvas.GetTop(_selectedBlock) + _selectedBlock.Height ||
                        line.X2 == Canvas.GetLeft(_selectedBlock) + _selectedBlock.Width / 2 &&
                        line.Y2 == Canvas.GetTop(_selectedBlock))
                    {
                        ScriptCanvas.Children.Remove(line);
                    }
                }

                foreach (var child in ScriptCanvas.Children)
                {
                    if (child is TextBlock textBlock && textBlock.Text == _selectedBlock.Tag.ToString())
                    {
                        ScriptCanvas.Children.Remove(textBlock);
                        break;
                    }
                }

                ScriptCanvas.Children.Remove(_selectedBlock);
                _selectedBlock = null;
            }
        }

        private void ExecuteScript_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript(sender, e);
        }

        private async void ExecuteScript(object sender, RoutedEventArgs e)
        {
            foreach (var child in ScriptCanvas.Children)
            {
                if (child is Rectangle block)
                {
                    if (block.Tag is TextBox inputTextBox && inputTextBox.Visibility == Visibility.Visible)
                    {
                        string variableName = inputTextBox.Text; _variables[variableName] = 42;
                    }

                    if (block.Tag.ToString() == "Условие")
                    {
                        if (_variables.ContainsKey("myVariable") && (int)_variables["myVariable"] > 10)
                        {
                        }
                    }
                    else if (block.Tag.ToString() == "Цикл")
                    {
                        while (_variables.ContainsKey("myVariable") && (int)_variables["myVariable"] > 0)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
            }
        }
    }
}
