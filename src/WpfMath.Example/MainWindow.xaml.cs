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

//“ут содержатс€ фигуры дл€ рисовани€
using System.Windows.Shapes;

namespace WpfMath.Example
{
    public partial class MainWindow : Window
    {
        private readonly TexFormulaParser _formulaParser = new TexFormulaParser();
        public FormulaControl newFormula = new FormulaControl();
        public MainWindow()
        {
            InitializeComponent();
            newFormula.Height = 100;

        }
        private State stateCursor = State.None;
        private enum State{ 
          Ellipse,
          Rectangle,
          Line,
          Pen,
          Eraser,
          Graph,
          None,
          Formula
        };


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

        private string AddSVGHeader(string svgText)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>")
                .AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" >")
                .AppendLine(svgText)
                .AppendLine("</svg>");

            return builder.ToString();
        }

        //TODO: add save box
        
        private void Window_Closed(object sender, EventArgs e)
        {
            /*  «акрытие и открытие листа на одном месте
            Properties.Settings ps = Properties.Settings.Default;
            ps.Top = this.Top;
            ps.Left = this.Left;
            ps.Save();*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*Properties.Settings ps = Properties.Settings.Default;
            this.Top = ps.Top;
            this.Left = ps.Left;*/
        }

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Eraser);
            ic.EditingMode = InkCanvasEditingMode.EraseByPoint;

        }

        private void pen_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Pen);
            ic.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (stateCursor == State.Eraser)
                return;

            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ic.DefaultDrawingAttributes.Color = Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
            }
        }

        private void del_Click(object sender, RoutedEventArgs e)
        {
            this.ic.Strokes.Clear();
            InputTextBox.Clear();
        }
        private void ContexMenuFile_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }
        private void ContexMenuInfo_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }
        private void ContexMenuHelp_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        private void formula_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Formula);
            //TODO: add formula box
        }
        Point point;
        private void graph_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Graph);
            //TODO: add graph
        }

        private void lines_Click(object sender, RoutedEventArgs e)
        {
            setStateCursor(State.Line);
            //TODO: add line
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 1)
            {
                setSizeCursor(0.1);
                return;
            }
            setSizeCursor(e.NewValue);
        }

        private void setStateCursor(State state)
        {
            this.stateCursor = state;
        }

        private void setSizeCursor(double value)
        {
            ic.DefaultDrawingAttributes.Width = value;
            ic.DefaultDrawingAttributes.Height = value;
        }
    }
}
