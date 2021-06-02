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

//Тут содержатся фигуры для рисования
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MathBoard;

namespace WpfMath.Example
{
    public partial class MainWindow : Window
    {
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              Объявление полей
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        private Color allColor = new Color();
        private Point prev;
        private bool isPaint = false;

        const double STEP = 0.1d;
        const double START_X = -10d;
        const double END_X = 10d;
        private static string MathExpression { get; set; }
        private Point mousePoint1;

        private Point figureStart;
        private Rectangle rectangle;
        private Ellipse ellipse;
        private Line line;

        private double stroke = 0;
        private State stateCursor = State.Pen;
        private enum State
        {
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
         * Конструктор
         */
        public MainWindow()
        {
            InitializeComponent();
        }
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              Методы работы с формулами
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */


        private void DrawFunctionGraph(List<Point> points)
        {
            points = PerformTransformation(points);

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point firstPoint = points[i];
                Point secondPoint = points[i + 1];

                if (!IsValid(firstPoint) || !IsValid(secondPoint)) { continue; }

                SolidColorBrush color = new SolidColorBrush(System.Windows.Media.Colors.Green);

                AddNewLineToCanvas(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y, color, 4);
            }
        }

        private List<Point> PerformTransformation(List<Point> points)
        {
            double canvasHalfWidth = 400 / 2;
            double canvasHalfHeight = 400 / 2;

            Point scalingPoint = Settings.Plane.GetDimensions();

            double horizontalScale = 400 / scalingPoint.X;
            double vertcalScale = 400 / scalingPoint.Y;

            MathBoard.Matrix.InitialiseTranslation(canvasHalfWidth, canvasHalfHeight);
            MathBoard.Matrix.InitialiseScaling(horizontalScale, vertcalScale);
            MathBoard.Matrix.FlipVertically = true;
            MathBoard.Matrix.Rebuild();

            return MathBoard.Matrix.PerformTransformation(points);
        }

        private void AddNewLineToCanvas(double x1, double y1, double x2, double y2, SolidColorBrush color, double strokeThickness = 1)
        {
            var mousePoint = Mouse.GetPosition(this);

            Line line = new Line();
            line.Margin = new Thickness(mousePoint1.X, mousePoint1.Y - 25, 100, 100);
            line.Stroke = color;
            line.StrokeThickness = strokeThickness;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;

            ic.Children.Add(line);
        }

        private bool IsValid(Point point)
        {
            bool isNumber = !double.IsNaN(point.X) && !double.IsNaN(point.Y);
            bool isFinite = !double.IsInfinity(point.X) && !double.IsInfinity(point.Y);

            return isNumber && isFinite;
        }

        private void DrawCoordinatePlane()
        {
            Point dimensions = Settings.Plane.GetDimensions();

            double horizontalMiddle = dimensions.X / 2;
            double verticalMiddle = dimensions.Y / 2;

            //Размер графика
            double width = 400;
            double height = 400;

            double horizontalOffset = (width - 1) / dimensions.X;
            double verticalOffset = (height - 1) / dimensions.Y;

            for (int i = 0; i <= dimensions.X; i++)
            {
                double step = i * verticalOffset + 1;
                double strokeThickness = i == horizontalMiddle ? 4 : Settings.Line.DefaultStrokeThickness;

                AddNewLineToCanvas(0, step, width, step, Settings.Line.Color, strokeThickness);
            }

            for (int i = 0; i <= dimensions.Y; i++)
            {
                double step = i * horizontalOffset + 1;
                double strokeThickness = i == verticalMiddle ? 4 : Settings.Line.DefaultStrokeThickness;

                AddNewLineToCanvas(step, 0, step, height, Settings.Line.Color, strokeThickness);
            }
        }


        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        * 
        *                                              Запуск и закрытие окна
        * 
        * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */
        //TODO: add save box
        private void Window_Closed(object sender, EventArgs e)
        {
            /*  Закрытие и открытие листа на одном месте
            Properties.Settings ps = Properties.Settings.Default;
            ps.Top = this.Top;
            ps.Left = this.Left;
            ps.Save();*/
        }

        //Загрузка окна
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
        *                  Осталось от предыдущих разрабов, не разбирался. Как понял - это боксы ввода формул
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
         *                                              Обработка нажатий на кнопки
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * Клик на ластик
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Eraser);
        }

        /*
         * Клик на карандаш
         */
        private void pen_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Pen);
        }

        /*
         * Клик на выбор цвета
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
         * Клик на очистку графиков
         */
        private void del_Click(object sender, RoutedEventArgs e)
        {
            this.ic.Strokes.Clear();
            ic.Children.Clear();
        }

        /*
         * Клик на File
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
         * Клик на Info
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
         * Клик на Help
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
         * Клик на формулу
         */

        private void formula_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Formula);
            //TODO: add formula box
        }

        /*
         * Изменение слайдера с размером курсора
         */
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 0.1)
                setStroke(0.1);

            setStroke(e.NewValue);
        }

        /*
         * Нажатие на прямоугольник
         */
        private void rectangle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Rectangle);
        }
        /*
         * Нажатие на полярный график
         */
        private void polar_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.PolarGraph);
        }
        /*
         * Нажатие на линию
         */
        private void line_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Line);
        }
        /*
         * Нажатие на эллипс
         */
        private void circle_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Ellipse);
        }
        /*
         * Нажатие на график
         */
        private void dec_graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Graph);
        }
        /*
         * Нажатие на кривую линию
         */
        private void curved_line_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.CurvedLine);
        }


        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         * 
         *                                              Обработка нажатий на канвас
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * Нажатие ЛКМ на канвас
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

                    var lineEraser = getNewLine(mousePoint, mousePoint, Color.FromRgb(255, 255, 255));
                    prev = mousePoint;
                    ic.Children.Add(lineEraser);
                    break;

                case State.Graph:

                    mousePoint1 = mousePoint;

                    MathBoard.FormulaParserWindow parsG = new MathBoard.FormulaParserWindow(mousePoint);
                    parsG.Owner = this;
                    parsG.ShowDialog();
                    string formula = parsG.InputTextBox.Text;

                    List<Point> points = new List<Point>();
                    MathParser mp = new MathParser(Mode.GRAD);
                    MathExpression = formula;
                    for (double i = START_X; i <= END_X; i += STEP)
                    {
                        string temp = MathExpression.Replace("x", i.ToString());
                        if (!mp.Evaluate(temp))
                        {
                        }
                        else
                        {
                            if ((mp.Result < 10 && mp.Result > -10) && (i < 10 && i > -10))
                            {
                                Point point = new Point(i, mp.Result);
                                points.Add(point);
                            }
                        }
                    }
                    DrawCoordinatePlane();
                    DrawFunctionGraph(points);

                    setStateCursor(State.None);

                    break;

                case State.Formula:
                    MathBoard.FormulaParserWindow pars = new MathBoard.FormulaParserWindow(mousePoint);
                    pars.Owner = this;
                    pars.ShowDialog();

                    if (pars.savePng == null)
                        return;
                    Image image = new Image()
                    {
                        Source = pars.savePng,
                        Margin = new Thickness(mousePoint.X, mousePoint.Y - 25, 100, 100)
                    };
                    ic.Children.Add(image);
                    pars.savePng = null;
                    setStateCursor(State.None);
                    break;
            }
            isPaint = true;
        }

        /*
         * Поднятие ЛКМ с канваса
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
         * Ведение зажатой ЛКМ по канвасу
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
                    //TODO: при большом размере курсора недорисовывает маленький эллипс (обрезает)

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
         *                                              Геттеры и сеттеры класса
         * 
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         */

        /*
         * Сеттер выбранного цвета
         */
        private void setAllColor(Color color)
        {
            allColor = color;
        }

        /*
         * Геттер выбранного цвета
         */
        public Color getAllColor()
        {
            return allColor;
        }

        /*
        * Установка состояния курсора (что рисовать). Состояния в enum State
        */
        private void setStateCursor(State state)
        {
            this.stateCursor = state;
        }

        /*
         * Сеттер на установку размера курсора
         */
        private void setStroke(double value)
        {
            stroke = value;
        }


        /*
         * Геттер размера курсора
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
