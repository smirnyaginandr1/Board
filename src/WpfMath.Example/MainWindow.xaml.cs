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
        }
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              ������ ������ � ���������
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */
        




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

        private void imgTL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            newFormula = new FormulaControl();
            ParseFormula(InputTextBox.Text, newFormula);
            */ 
        }

        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         * 
         *                                              ��������� ������� �� ������
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

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
            ic.Children.Clear();
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

        /*
         * ������� �� �������������
         */
        private void rectangle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Rectangle);
        }
        /*
         * ������� �� �������� ������
         */
        private void polar_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.PolarGraph);
        }
        /*
         * ������� �� �����
         */
        private void line_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Line);
        }
        /*
         * ������� �� ������
         */
        private void circle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Ellipse);
        }
        /*
         * ������� �� ������
         */
        private void dec_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Graph);
        }
        /*
         * ������� �� ������ �����
         */
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
                    MathBoard.FormulaParserWindow pars = new MathBoard.FormulaParserWindow();
                    pars.Owner = this;
                    pars.ShowDialog();

                    if (pars.imageCanvas.Source == null)
                    {
                        setStateCursor(State.None);
                        return;
                    }

                    if (pars.saveFlag)
                    {
                        setStateCursor(State.None);
                        pars.imageCanvas = null;
                        pars.saveFlag = false;
                        return;
                    }
                    Image image = new Image()
                    {
                        Source = pars.savePng,
                        Margin = new Thickness(mousePoint.X, mousePoint.Y - 25, 100, 100)
                    };
                    ic.Children.Add(image);
                    setStateCursor(State.None);
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
                    //TODO: ��� ������� ������� ������� �������������� ��������� ������ (��������)

                    if (widthEllipse >= 0 && heightEllipse >= 0)
                    {
                        ellipse.Width = widthEllipse;
                        ellipse.Height = heightEllipse;
                        return;
                    }

                    ellipse.Margin = new Thickness(
                        (widthEllipse < 0) ? pointMouse.X : figureStart.X,
                        (heightEllipse < 0) ? pointMouse.Y - 25 : figureStart.Y - 25,
                        (widthEllipse < 0) ? figureStart.X : pointMouse.X,
                        (heightEllipse < 0) ? figureStart.Y - 25 : pointMouse.Y - 25);
                    ellipse.Width = (widthEllipse < 0) ? figureStart.X - pointMouse.X : widthEllipse;
                    ellipse.Height = (heightEllipse < 0) ? figureStart.Y - pointMouse.Y : heightEllipse;
                   
                    break;

                case State.Rectangle:
                    double widthRectangle = pointMouse.X - figureStart.X;
                    double heightRectangle = pointMouse.Y - figureStart.Y;

                    if (widthRectangle >= 0 && heightRectangle >= 0)
                    {
                        rectangle.Width = widthRectangle;
                        rectangle.Height = heightRectangle;
                        return;
                    }

                    rectangle.Margin = new Thickness(
                         (widthRectangle < 0) ? pointMouse.X : figureStart.X,
                         (heightRectangle < 0) ? pointMouse.Y - 25 : figureStart.Y - 25,
                         (widthRectangle < 0) ? figureStart.X : pointMouse.X,
                         (heightRectangle < 0) ? figureStart.Y - 25 : pointMouse.Y - 25);
                    rectangle.Width = (widthRectangle < 0) ? figureStart.X - pointMouse.X : widthRectangle;
                    rectangle.Height = (heightRectangle < 0) ? figureStart.Y - pointMouse.Y : heightRectangle;
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
