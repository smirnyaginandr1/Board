using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WpfMath.Converters;
using WpfMath.Controls;
using System.Windows.Media;
using System.Windows.Forms;

//��� ���������� ������ ��� ���������
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace WpfMath.Example
{
    public partial class MainWindow : Window
    {
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              ���������� �����
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */
        private readonly TexFormulaParser _formulaParser = new TexFormulaParser();
        public FormulaControl newFormula = new FormulaControl();
        private Color allColor = new Color();

        private Point prev;
        private bool isPaint = false;

        private Point figureStart;
        private Rectangle rectangle;
        private Ellipse ellipse;
        private Line line;


        private double stroke = 0;
        private State stateCursor = State.Pen;
        private enum State{ 
          Ellipse,
          Rectangle,
          Line,
          CurvedLine,
          Pen,
          Eraser,
          Graph,
          PolarGraph,
          None,
          Formula
        };

        /*
         * �����������
         */
        public MainWindow()
        {
            InitializeComponent();
            newFormula.Height = 100;
        }
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              ������ ������ � ���������
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */
        /*
         * ���� ������
         */
        private TexFormula? ParseFormula(string input, FormulaControl fc)
        {
            // Create formula object from input text.
            TexFormula? formula = null;
            try
            {
                formula = this._formulaParser.Parse(input);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occurred while parsing the given input:" + Environment.NewLine +
                    Environment.NewLine + ex.Message, "WPF-Math Example", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return formula;
        }

        /*
         * �������� ������� ��� ������� � XAML
         */
        private string AddSVGHeader(string svgText)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>")
                .AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" >")
                .AppendLine(svgText)
                .AppendLine("</svg>");

            return builder.ToString();
        }



        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              ������ � �������� ����
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */
        //TODO: add save box
        private void Window_Closed(object sender, EventArgs e)
        {
            /*  �������� � �������� ����� �� ����� �����
            Properties.Settings ps = Properties.Settings.Default;
            ps.Top = this.Top;
            ps.Left = this.Left;
            ps.Save();*/
        }

        //�������� ����
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var mySliderSize = (Slider)this.FindName("sizeSlider");
            mySliderSize.Value = ic.DefaultDrawingAttributes.Width;
            setStroke(mySliderSize.Value);
            setAllColor(Color.FromRgb(0, 0, 0));
            ic.EditingMode = InkCanvasEditingMode.None;
            /*Properties.Settings ps = Properties.Settings.Default;
            this.Top = ps.Top;
            this.Left = ps.Left;*/
        }

        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                  �������� �� ���������� ��������, �� ����������. ��� ����� - ��� ����� ����� ������
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        private void inputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Formula.SelectionStart = InputTextBox.SelectionStart;
            Formula.SelectionLength = InputTextBox.SelectionLength;
        }
        private void FormulaTextBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            InputTextBox.Text = (string)item.DataContext;
        }

        private void imgTL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            newFormula = new FormulaControl();
            ParseFormula(InputTextBox.Text, newFormula);
        }

        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         * 
         *                                              ��������� ������� �� ������
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * ���� �� ����������
         */
        private void saveButton_Click(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Choose file
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "SVG Files (*.svg)|*.svg|PNG Files (*.png)|*.png"
                };
                var result = saveFileDialog.ShowDialog();
                if (result == false) return;

                // Create formula object from input text.
                var formula = ParseFormula(InputTextBox.Text, Formula);
                if (formula == null) return;
                var renderer = formula.GetRenderer(TexStyle.Display, this.newFormula.Scale, "Arial");

                // Open stream
                var filename = saveFileDialog.FileName;
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    switch (saveFileDialog.FilterIndex)
                    {
                        case 1:
                            var geometry = renderer.RenderToGeometry(0, 0);
                            var converter = new SVGConverter();
                            var svgPathText = converter.ConvertGeometry(geometry);
                            var svgText = AddSVGHeader(svgPathText);
                            using (var writer = new StreamWriter(stream))
                                writer.WriteLine(svgText);
                            break;

                        case 2:
                            var bitmap = renderer.RenderToBitmap(0, 0, 300);
                            var encoder = new PngBitmapEncoder
                            {
                                Frames = { BitmapFrame.Create(bitmap) }
                            };
                            encoder.Save(stream);
                            break;

                        default:
                            return;
                    }
                }
            }
        }

        /*
         * ���� �� ������
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Eraser);
        }

        /*
         * ���� �� ��������
         */
        private void pen_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Pen);
        }

        /*
         * ���� �� ����� �����
         */
        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (stateCursor == State.Eraser)
                return;

            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                setAllColor(Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B));
            }
        }

        /*
         * ���� �� ������� ��������
         */
        private void del_Click(object sender, RoutedEventArgs e)
        {
            this.ic.Strokes.Clear();
            InputTextBox.Clear();
        }

        /*
         * ���� �� File
         */
        private void ContexMenuFile_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        /*
         * ���� �� Info
         */
        private void ContexMenuInfo_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        /*
         * ���� �� Help
         */
        private void ContexMenuHelp_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        /*
         * ���� �� �������
         */

        private void formula_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Formula);
            //TODO: add formula box
        }

        /*
         * ��������� �������� � �������� �������
         */
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 0.1)
                setStroke(0.1);

            setStroke(e.NewValue);
        }

        private void rectangle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Rectangle);
        }

        private void polar_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.PolarGraph);
        }

        private void line_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Line);
        }

        private void circle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Ellipse);
        }

        private void dec_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Graph);
        }

        private void curved_line_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.CurvedLine);
        }


        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         * 
         *                                              ��������� ������� �� ������
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * ������� ��� �� ������
         */
        private void ic_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePoint = Mouse.GetPosition(this);

            switch (stateCursor)
            {
                case State.Ellipse:
                    figureStart = mousePoint;
                    ellipse = getNewEllpse(mousePoint, mousePoint, getAllColor());
                    ic.Children.Add(ellipse);
                    break;

                case State.Rectangle:
                    figureStart = mousePoint;
                    rectangle = getNewRectangle(mousePoint, mousePoint, getAllColor());
                    ic.Children.Add(rectangle);
                    break;

                case State.Line:
                    line = getNewLine(mousePoint, mousePoint, getAllColor());
                    ic.Children.Add(line);
                    break;

                case State.Pen:
                    if (isPaint) return;

                    var linePen = getNewLine(mousePoint, mousePoint, getAllColor());
                    prev = mousePoint;
                    ic.Children.Add(linePen);
                    break;

                case State.Eraser:
                    if (isPaint) return;

                    var lineEraser = getNewLine(mousePoint, mousePoint, Color.FromRgb(255,255,255));
                    prev = mousePoint;
                    ic.Children.Add(lineEraser);
                    break;

                case State.Graph:
                    break;

                case State.Formula:
                    break;
            }
            isPaint = true;
        }

        /*
         * �������� ��� � �������
         */
        private void ic_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPaint = false;
            switch (stateCursor)
            {
                case State.Ellipse:
                    ellipse = null;
                    break;

                case State.Rectangle:
                    rectangle = null;
                    break;

                case State.Line:
                    line = null;
                    break;

                case State.Pen:
                    break;

                case State.Eraser:
                    break;

                case State.Graph:
                    break;

                case State.Formula:
                    break;
            }
        }

        /*
         * ������� ������� ��� �� �������
         */
        private void ic_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isPaint) return;
            var pointMouse = Mouse.GetPosition(this);
            switch (stateCursor)
            {
                case State.Ellipse:
                    double widthEllipse = pointMouse.X - figureStart.X;
                    double heightEllipse = pointMouse.Y - figureStart.Y;

                    //TODO: �������� ���������� � �������� ������� (���� ��� ������)
                    //TODO: ��� ������� ������� ������� �������������� ��������� ������ (��������)
                    if (widthEllipse < 0)
                        widthEllipse = figureStart.X - pointMouse.X;

                    if (heightEllipse < 0)
                        heightEllipse = figureStart.Y - pointMouse.Y;

                    ellipse.Width = widthEllipse;
                    ellipse.Height = heightEllipse;
                    break;

                case State.Rectangle:
                    double widthRectangle = pointMouse.X - figureStart.X;
                    double heightRectangle = pointMouse.Y - figureStart.Y;

                    //TODO: �������� ���������� � �������� ������� (���� ��� ������)
                    if (widthRectangle < 0)
                        widthRectangle = figureStart.X - pointMouse.X;

                    if (heightRectangle < 0)
                        heightRectangle = figureStart.Y - pointMouse.Y;
                    
                    rectangle.Width = widthRectangle;
                    rectangle.Height = heightRectangle;
                    break;

                case State.Line:
                    line.X2 = pointMouse.X;
                    line.Y2 = pointMouse.Y - 25;
                    break;

                case State.Pen:
                    var linePen = getNewLine(prev, pointMouse, getAllColor());
                    prev = pointMouse;
                    ic.Children.Add(linePen);
                    break;

                case State.Eraser:
                    var lineEraser = getNewLine(prev, pointMouse, Color.FromRgb(255, 255, 255));
                    prev = pointMouse;
                    ic.Children.Add(lineEraser);
                    break;

                case State.Graph:
                    break;

                case State.Formula:
                    break;
            }
        }

        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         * 
         *                                              ������� � ������� ������
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * ������ ���������� �����
         */
        private void setAllColor(Color color)
        {
            allColor = color;
        }

        /*
         * ������ ���������� �����
         */
        public Color getAllColor()
        {
            return allColor;
        }

        /*
        * ��������� ��������� ������� (��� ��������). ��������� � enum State
        */
        private void setStateCursor(State state)
        {
            this.stateCursor = state;
        }

        /*
         * ������ �� ��������� ������� �������
         */
        private void setStroke(double value)
        {
            stroke = value;
        }


        /*
         * ������ ������� �������
         */
        public double getStroke()
        {
            return stroke;
        }

        private Line getNewLine(Point pointOne, Point pointTwo, Color color)
        {
            return new Line
            {
                X1 = pointOne.X,
                Y1 = pointOne.Y - 25,
                X2 = pointTwo.X,
                Y2 = pointTwo.Y - 25,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = getStroke()
            };
        }
        private Rectangle getNewRectangle(Point pointOne, Point pointTwo, Color color)
        {
            return new Rectangle
            {
                Stroke = new SolidColorBrush(getAllColor()),
                Height = 0,
                Width = 0,
                Margin = new Thickness(pointOne.X, pointOne.Y - 25, pointTwo.X, pointTwo.Y - 25),
                StrokeThickness = getStroke()
                
            };
        }

        private Ellipse getNewEllpse(Point pointOne, Point pointTwo, Color color)
        {
            return new Ellipse
            {
                Stroke = new SolidColorBrush(color),
                Height = 0,
                Width = 0,
                Margin = new Thickness(pointOne.X, pointOne.Y - 25, pointTwo.X, pointTwo.Y - 25),
                StrokeThickness = getStroke()
            };
        }
    }
}
