using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfMath.Converters;
using WpfMath.Controls;
using WpfMath;
using System.IO;

namespace MathBoard
{
    /// <summary>
    /// Логика взаимодействия для FormulaParserWindow.xaml
    /// </summary>
    public partial class FormulaParserWindow : Window
    {
        private readonly TexFormulaParser _formulaParser = new TexFormulaParser();
        public FormulaControl newFormula = new FormulaControl();
        public BitmapSource savePng;
        public FormulaParserWindow(Point mousePoint)
        {
            InitializeComponent();
            newFormula.Height = 100;
            ic.EditingMode = InkCanvasEditingMode.None;
        }
        /*
                 * Парс строки
         */
        public TexFormula? ParseFormula(string input, FormulaControl fc)
        {
            // Create formula object from input text.
            TexFormula? formula = null;
            try
            {
                if (input.Contains("^"))
                {

                }
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
         * Создание билдера для вставки в XAML
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

        private void inputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
             Formula.SelectionStart = InputTextBox.SelectionStart;
             Formula.SelectionLength = InputTextBox.SelectionLength;
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var formula = ParseFormula(InputTextBox.Text, Formula);
            if (formula == null) return;
            var renderer = formula.GetRenderer(TexStyle.Display, this.newFormula.Scale, "Arial");
            savePng = renderer.RenderToBitmap(0, 0, 300);
            this.Close();
        }

    }
}
