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
        private TexFormula? Parse_Formula(string input)
        {
            // Create formula object from input text.
            TexFormula? formula = null;
            try
            {
                formula = this.formulaParser.Parse(input);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода");
            }

            return formula;
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

            if (ParseFormula())
            {
                imageCanvas.Source = null;
                return;
            }
            TexFormula formula = Parse_Formula(canvasString);
            if (formula == null)
                return;
            TexRenderer renderer = formula.GetRenderer(TexStyle.Display, 20.0, "Arial");
            savePng = renderer.RenderToBitmap(0.0, 0.0);
            imageCanvas.Source = savePng;
        }
        private bool ParseFormula()
        {
            bool error = false;
            int number = 0;
            foreach (var str in funcStr)
            {
                while (canvasString.Contains(str))
                {
                    int i = 0;
                    int position = canvasString.IndexOf(str);
                    String[] args;
                    switch (str)
                    {
                        case "lim":
                            args = findArgs(str, position, 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 3 + args[1].Length + args[2].Length);
                            canvasString = canvasString.Insert(position, "\\l_im_{" + args[0] + "\\to" + args[1] + "}" + "(" + args[2] + ")");
                            break;

                        case "sqrt":
                            args = findArgs(str, position, 2);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1 + args[1].Length + 1);
                            canvasString = canvasString.Insert(position, "\\sq_rt[" + args[0] + "]{" + args[1] + "}");
                            break;

                        case "bin":
                            args = findArgs(str, position, 2);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1 + args[1].Length + 1);
                            canvasString = canvasString.Insert(position, "\\bi_nom{" + args[0] + "}{" + args[1] + "}");
                            break;

                        case "int":
                            args = findArgs(str, position, 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1 + args[1].Length + 1 + args[2].Length + 1);
                            canvasString = canvasString.Insert(position, "\\i_nt_{" + args[0] + "}^{" + args[1] + "}(" + args[2] + ")");
                            break;

                        case "sum":
                            args = findArgs(str, position, 3);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1 + args[1].Length + 1 + args[2].Length + 1);
                            canvasString = canvasString.Insert(position, "\\s_um_{" + args[0] + "}^{" + args[1] + "} (" + args[2] + ")");
                            break;
                        case "/":
                            if (position + 1 >= canvasString.Length || position == 0)
                            {
                                error = true;
                                break;
                            }
                            if (canvasString[position + 1] == '(')
                            {
                                args = findArgs(str, position, 1);
                                if (args == null)
                                {
                                    error = true;
                                    break;
                                }
                            }
                            else
                                args = new string[] { canvasString[position + 1].ToString() };


                            String backArg;

                            if (canvasString[position - 1] == ')')
                                backArg = findBackArgs(position);
                            else 
                                backArg = canvasString[position - 1].ToString();

                            if (backArg == "" || backArg == null)
                            {
                                error = true;
                                break;
                            }

                            if (canvasString[position - 1] == ')' && canvasString[position + 1] == '(')
                            {
                                canvasString = canvasString.Remove(position - 2 - backArg.Length,
                                    1 + 2 + args[0].Length + 2 + backArg.Length);
                                canvasString = canvasString.Insert(position - 2 - backArg.Length, "\\frac{" + backArg + "}{" + args[0] + "}");
                            }
                            else if (canvasString[position - 1] != ')' && canvasString[position + 1] == '(')
                            {
                                canvasString = canvasString.Remove(position - 1,
                                    1 + 2 + args[0].Length + 1);
                                canvasString = canvasString.Insert(position - 1, "\\frac{" + backArg + "}{" + args[0] + "}");
                            }
                            else if (canvasString[position - 1] == ')' && canvasString[position + 1] != '(')
                            {
                                canvasString = canvasString.Remove(position - 2 - backArg.Length,
                                    1 + 2 + backArg.Length + 1);
                                canvasString = canvasString.Insert(position - 2 - backArg.Length, "\\frac{" + backArg + "}{" + args[0] + "}");
                            }
                            else
                            {
                                canvasString = canvasString.Remove(position - 1, 3);
                                canvasString = canvasString.Insert(position - 1, "\\frac{" + backArg + "}{" + args[0] + "}");
                            }
                            break;

                        case "pow":
                            args = findArgs(str, position, 1);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1);
                            canvasString = canvasString.Insert(position, "^{" + args[0] + "}");
                            break;

                        case "low":
                            args = findArgs(str, position, 1);
                            if (args == null)
                            {
                                error = true;
                                break;
                            }
                            canvasString = canvasString.Remove(position, str.Length + 1 + args[0].Length + 1);
                            canvasString = canvasString.Insert(position, "_{" + args[0] + "}");
                            break;


                    }
                    if (error)
                    {
                        if (str == "/")
                            labelStatus.Content = "Ошибка в делении";
                        else
                            labelStatus.Content = "Ошибка в " + str;
                        return true;
                    }
                }
                number++;
            }
            canvasString = canvasString.Replace("s_um", "sum");
            canvasString = canvasString.Replace("sq_rt", "sqrt");
            canvasString = canvasString.Replace("bi_nom", "binom");
            canvasString = canvasString.Replace("i_nt", "int");
            canvasString = canvasString.Replace("l_im", "lim");
            return false;
        }
        private String[] findArgs(String func, int position, int argsQuantity)
        {
            String[] args = new string[argsQuantity];
            for (int i = 0; i < argsQuantity; i++)
                args[i] = "";

            if (position + func.Length >= canvasString.Length)
                return null;
            char a;
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
            foreach (var arg in args)
                if (arg == "")
                    return null;
            return args;
        }

        private String findBackArgs(int position)
        {
            String finalStr = "";

            int bracketCount = 0;
            for (int i = position - 2; i >= 0; i--)
            {
                if (canvasString[i] == ')')
                    bracketCount++;

                if (canvasString[i] == '(' && bracketCount == 0)
                {
                    char[] arr = finalStr.ToCharArray();
                    Array.Reverse(arr);
                    return new string(arr);
                }
                if (canvasString[i] == '(' && bracketCount != 0)
                    bracketCount--;
                finalStr += canvasString[i];
            }
            return null;
        }
    }
}
