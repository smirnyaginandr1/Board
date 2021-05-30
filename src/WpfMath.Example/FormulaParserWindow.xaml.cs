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
        private String canvasString = "";
       
        private String templateSqrt = "\\sqrt[arg1]{arg2}	";
        private String templateBinom = "\\binom{arg1}{arg2}";
        private String templateDrob = "\\frac{arg1}{arg2}";
        private String templatePow = "^{arg1}";
        private String templateLow = "_{arg1}";
        private String templateIntegral = "\\int_{arg1}^{arg2} arg3";
        private String templateSum = "\\sum_{arg2}^{arg1} arg3";
        private List<String> funcStr = new List<String> { "lim", "sqrt", "bin", "/", "^", "low", "int", "sum" };
        private List<List<int>> table;
        private TexFormulaParser formulaParser = new TexFormulaParser();

        public Image imageCanvas;
        public bool saveFlag;
        public BitmapSource savePng;
        public FormulaParserWindow()
        {
            InitializeComponent();
            ic.EditingMode = InkCanvasEditingMode.None;
            saveFlag = true;
            imageCanvas = new Image() { };
            ic.Children.Add(imageCanvas);
            
        }


        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            saveFlag = false;
            this.Close();
        }

        private void Parse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvasString = InputTextBox.Text;
            canvasString = canvasString.Replace(" ", "");
            if (canvasString == "")
            {
                imageCanvas.Source = null;
                return;
            }

            createTable();
            if (ParseFormula())
            {
                imageCanvas.Source = null;
                return;
            }
            TexFormula formula = formulaParser.Parse(canvasString);
            TexRenderer renderer = formula.GetRenderer(TexStyle.Display, 20.0, "Arial");
            savePng = renderer.RenderToBitmap(0.0, 0.0);
            imageCanvas.Source = savePng;
        }

        private void createTable()
        {
            table = new List<List<int>>();
            for (int i = 0; i < funcStr.Count; i++){
                String copyStr = String.Copy(canvasString);
                String str = funcStr[i];
                table.Add(new List<int>());
                int count = 0;
                while (true)
                {
                    if (!copyStr.Contains(str))
                        break;
                    table[i].Add(copyStr.IndexOf(str) + (count * str.Length));
                    count++;
                    copyStr = copyStr.Remove(copyStr.IndexOf(str), str.Length);
                }
            }
        }
        private void reloadTable()
        {
            for (int i = 0; i < funcStr.Count; i++)
            {
                String copyStr = String.Copy(canvasString);
                String str = funcStr[i];
                int strCount = 0;
                int count = 0;
                while (true)
                {
                    if (!copyStr.Contains(str))
                        break;
                    table[i][strCount] = copyStr.IndexOf(str) + (count * str.Length);
                    strCount++;
                    count++;
                    copyStr = copyStr.Remove(copyStr.IndexOf(str), str.Length);
                }
            }
        }
        private bool ParseFormula()
        {
            bool error = false;
            int number = 0;
            foreach (var str in funcStr)
            {
                for (int i = 0; i < table[number].Count; i++)
                {
                    switch (str)
                    {
                        case "lim":
                            error = parseLim(table[number][i]);
                            break;

                        case "sqrt":
                            error = parseSqrt(table[number][i]);
                            break;

                        case "bin":
                            error = parseBin(table[number][i]);
                            break;

                        case "/":
                            error = parseDrob(table[number][i]);
                            break;

                        case "^":
                            error = parsePow(table[number][i]);
                            break;
                            
                        case "low":
                            error = parseLowIndex(table[number][i]);
                            break;
                            
                        case "int":
                            error = parseIntegr(table[number][i]);
                            break;

                        case "sum":
                            error = parseSum(table[number][i]);
                            break;
                    }
                    if (error)
                    {
                        labelStatus.Content = "Ошибка в " + str;
                        return true;
                    }
                    reloadTable();
                }
                number++;
            }
            return false;
        }
        private bool parseLim(int position)
        { 
            String[] args = new string[3] { "", "", "" };
            //Проверка на скобку после lim
            if (canvasString[position + 3] != '(')
                return true;
            int countArg = 0;
            for (int i = position + 4; i < canvasString.Length; i++)
            {
                if (canvasString[i] == ',' || canvasString[i] == ')')
                {
                    countArg++;
                    if (countArg == args.Length)
                        break;
                    continue;
                }
                args[countArg] += canvasString[i];
            }
            // \\lim_{arg1 \\to arg2} arg3
            canvasString = canvasString.Remove(position, 4 + args[0].Length + 3 + args[1].Length + args[2].Length);
            canvasString = canvasString.Insert(position, "\\lim_{" + args[0] + " \\to " + args[1] + "} " + args[2]);
            
            return false;
        }

        private bool parseSqrt(int indexTableRow)
        {
            String arg1 = "", arg2 = "";
            return false;
        }

        private bool parseBin(int indexTableRow)
        {
            String arg1 = "", arg2 = "";
            return false;
        }
        private bool parseDrob(int indexTableRow)
        {
            String arg1 = "", arg2 = "";
            return false;
        }
        private bool parsePow(int indexTableRow)
        {
            String arg1 = "", arg2 = "";
            return false;
        }
        private bool parseLowIndex(int indexTableRow)
        {
            String arg1 = "", arg2 = "";
            return false;
        }
        private bool parseIntegr(int indexTableRow)
        {
            String arg1 = "", arg2 = "", arg3 = "";
            return false;
        }
        private bool parseSum(int indexTableRow)
        {
            String arg1 = "", arg2 = "", arg3 = "";
            return false;
        }
    }
}
