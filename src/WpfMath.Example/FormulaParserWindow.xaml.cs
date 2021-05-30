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
       

        private List<String> funcStr = new List<String> { "lim", "sqrt", "bin", "/", "pow", "low", "int", "sum" };
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
                    String[] args;
                    switch (str)
                    {
                        case "lim":
                            args = findArgs(str, table[number][i], 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(table[number][i], str.Length + 1 + args[0].Length + 3 + args[1].Length + args[2].Length);
                            canvasString = canvasString.Insert(table[number][i], "\\lim_{" + args[0] + " \\to " + args[1] + "} " + "(" + args[2] + ")");
                            break;

                        case "sqrt":
                            args = findArgs(str, table[number][i], 2);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(table[number][i], str.Length + 1 + args[0].Length + 1 + args[1].Length + 1);
                            canvasString = canvasString.Insert(table[number][i], "\\sqrt[" + args[0] + "]{" + args[1] + "}");
                            break;

                        case "bin":
                            args = findArgs(str, table[number][i], 2);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(table[number][i], str.Length + 1 + args[0].Length + 1 + args[1].Length + 1);
                            canvasString = canvasString.Insert(table[number][i], "\\binom{" + args[0] + "}{" + args[1] + "}");
                            break;

                        case "int":
                            args = findArgs(str, table[number][i], 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(table[number][i], str.Length + 1 + args[0].Length + 1 + args[1].Length + 1 + args[2].Length + 1);
                            canvasString = canvasString.Insert(table[number][i], "\\int_{" + args[0] + "}^{" + args[1] + "} (" + args[2] + ")");
                            break;

                        case "sum":
                            args = findArgs(str, table[number][i], 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(table[number][i], str.Length + 1 + args[0].Length + 1 + args[1].Length + 1 + args[2].Length + 1);
                            canvasString = canvasString.Insert(table[number][i], "\\sum_{" + args[0] + "}^{" + args[1] + "} (" + args[2] + ")");
                            break;
                        case "/":
                            error = parseDrob(table[number][i]);
                            break;

                        case "pow":
                            error = parsePow(table[number][i]);
                            break;

                        case "low":
                            error = parseLowIndex(table[number][i]);
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
        private String[] findArgs(String func, int position, int argsQuantity)
        {
            String[] args = new string[argsQuantity];
            for (int i = 0; i < argsQuantity; i++)
                args[i] = "";

            if (position + func.Length >= canvasString.Length)
                return null;

            if (canvasString[position + func.Length] != '(')
                return null;

            int countArg = 0, bracketCount = 0;
            for (int i = position + func.Length + 1; i < canvasString.Length; i++)
            {
                if (canvasString[i] == '(')
                    bracketCount++;

                if ((canvasString[i] == ',' || canvasString[i] == ')') && bracketCount == 0)
                {
                    countArg++;
                    if (countArg == args.Length)
                        break;
                    continue;
                }
                if (canvasString[i] == ')' && bracketCount != 0)
                    bracketCount--;
                args[countArg] += canvasString[i];
            }
            foreach(var arg in args)
                if(arg == "")
                    return null;
            return args;
        }

        private bool parseDrob(int position)
        {//"\\frac{arg1}{arg2}"
            return false;
        }
        private bool parsePow(int position)
        {//"^{arg1}"
            return false;
        }
        private bool parseLowIndex(int position)
        {//"_{arg1}"
            return false;
        }
    }
}
