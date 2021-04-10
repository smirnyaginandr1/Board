using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WpfMath.Converters;
using WpfMath.Controls;

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
                MessageBox.Show("An error occurred while parsing the given input:" + Environment.NewLine +
                    Environment.NewLine + ex.Message, "WPF-Math Example", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return formula;
        }

        private void saveButton_Click(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            { 
                // Choose file
                SaveFileDialog saveFileDialog = new SaveFileDialog()
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

        private void Window_Closed(object sender, EventArgs e)
        {
            //
        }

        private void inputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Formula.SelectionStart = InputTextBox.SelectionStart;
            Formula.SelectionLength = InputTextBox.SelectionLength;
        }

        private void FormulaTextBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem) ((ComboBox) sender).SelectedItem;
            InputTextBox.Text = (string)item.DataContext;
        }

        private void imgTL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            newFormula = new FormulaControl();
            ParseFormula(InputTextBox.Text, newFormula);
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ic.EditingMode == InkCanvasEditingMode.Ink)
                ic.EditingMode = InkCanvasEditingMode.EraseByPoint;
            else ic.EditingMode = InkCanvasEditingMode.Ink;
        }
    }
}
