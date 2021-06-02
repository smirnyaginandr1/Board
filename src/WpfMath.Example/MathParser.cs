using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MathBoard
{
    public enum Mode { RAD, DEG, GRAD };
    class MathParser
    {
        private ArrayList FunctionList = new ArrayList(new string[] { "abs", "acos", "asin", "atan", "ceil", "cos", "cosh", "exp", "floor", "ln", "log", "sign", "sin", "sinh", "sqrt", "tan", "tanh" });
        private double Value;
        private double Factor;
        private Mode mode;

        public MathParser()
        {
            this.Mode = Mode.RAD;
        }
        public MathParser(Mode mode)
        {
            this.Mode = mode;
        }

        public double Result
        {
            get { return this.Value; }
        }
        public Mode Mode
        {
            get { return this.mode; }
            set
            {
                this.mode = value;
                switch (value)
                {
                    case Mode.RAD:
                        this.Factor = 1.0;
                        break;
                    case Mode.DEG:
                        this.Factor = 2.0 * Math.PI / 360.0;
                        break;
                    case Mode.GRAD:
                        this.Factor = 2.0 * Math.PI / 400.0;
                        break;
                }
            }
        }

        public bool Evaluate(string Expression)
        {
            try
            {
                Expression = Expression.Replace(" ", "");
                Regex regEx = new Regex(@"(?<=[\d\)])(?=[a-df-z\(])|(?<=pi)(?=[^\+\-\*\/\\^!)])|(?<=\))(?=\d)|(?<=[^\/\*\+\-])(?=exp)", RegexOptions.IgnoreCase);
                Expression = regEx.Replace(Expression, "*");
                regEx = new Regex("pi", RegexOptions.IgnoreCase);
                Expression = regEx.Replace(Expression, Math.PI.ToString());
                regEx = new Regex(@"([a-z]*)\(([^\(\)]+)\)(\^|!?)", RegexOptions.IgnoreCase);
                Match m = regEx.Match(Expression);
                while (m.Success)
                {
                    if (m.Groups[3].Value.Length > 0) Expression = Expression.Replace(m.Value, "{" + m.Groups[1].Value + this.Solve(m.Groups[2].Value) + "}" + m.Groups[3].Value);
                    else Expression = Expression.Replace(m.Value, m.Groups[1].Value + this.Solve(m.Groups[2].Value));
                    m = regEx.Match(Expression);
                }
                this.Value = Convert.ToDouble(this.Solve(Expression));
                return true;
            }
            catch
            {
                // Shit!
                return false;
            }
        }

        private string Solve(string Expression)
        {
            Regex regEx = new Regex(@"([a-z]{2,})([\+-]?\d+,*\d*[eE][\+-]*\d+|[\+-]?\d+,*\d*)", RegexOptions.IgnoreCase);
            Match m = regEx.Match(Expression);
            while (m.Success && this.FunctionList.IndexOf(m.Groups[1].Value.ToLower()) > -1)
            {
                switch (m.Groups[1].Value.ToLower())
                {
                    case "abs":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Abs(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "acos":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Acos(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "asin":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Asin(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "atan":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Atan(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "cos":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Cos(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "ceil":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Ceiling(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "cosh":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Cosh(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "exp":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Exp(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "floor":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Floor(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "ln":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Log(Convert.ToDouble(m.Groups[2].Value), Math.Exp(1.0)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "log":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Log10(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "sign":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Sign(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "sin":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Sin(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "sinh":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Sinh(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "sqrt":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Sqrt(Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "tan":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Tan(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                    case "tanh":
                        Expression = Expression.Replace(m.Groups[0].Value, Math.Tanh(this.Factor * Convert.ToDouble(m.Groups[2].Value)).ToString());
                        m = regEx.Match(Expression);
                        continue;
                }
            }
            regEx = new Regex(@"\{(.+)\}!");
            m = regEx.Match(Expression);
            while (m.Success)
            {
                double n = Convert.ToDouble(m.Groups[1].Value);
                if ((n < 0) && (n != Math.Round(n))) throw new Exception();
                Expression = regEx.Replace(Expression, this.Fact(Convert.ToDouble(m.Groups[1].Value)).ToString(), 1);
                m = regEx.Match(Expression);
            }
            regEx = new Regex(@"(\d+,*\d*[eE][\+-]?\d+|\d+,*\d*)!");
            m = regEx.Match(Expression);
            while (m.Success)
            {
                double n = Convert.ToDouble(m.Groups[1].Value);
                if ((n < 0) && (n != Math.Round(n))) throw new Exception();
                Expression = regEx.Replace(Expression, this.Fact(Convert.ToDouble(m.Groups[1].Value)).ToString(), 1);
                m = regEx.Match(Expression);
            }
            regEx = new Regex(@"\{(.+)\}\^(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                Expression = Expression.Replace(m.Value, Math.Pow(Convert.ToDouble(m.Groups[1].Value), Convert.ToDouble(m.Groups[2].Value)).ToString());
                m = regEx.Match(Expression);
            }
            regEx = new Regex(@"(\d+,*\d*e[\+-]?\d+|\d+,*\d*)\^(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                Expression = regEx.Replace(Expression, Math.Pow(Convert.ToDouble(m.Groups[1].Value), Convert.ToDouble(m.Groups[2].Value)).ToString(), 1);
                m = regEx.Match(Expression);
            }
            regEx = new Regex(@"([\+-]?\d+,*\d*[eE][\+-]?\d+|[\-\+]?\d+,*\d*)([\/\*])(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                double result;
                switch (m.Groups[2].Value)
                {
                    case "*":
                        result = Convert.ToDouble(m.Groups[1].Value) * Convert.ToDouble(m.Groups[3].Value);
                        if ((result < 0) || (m.Index == 0)) Expression = regEx.Replace(Expression, result.ToString(), 1);
                        else Expression = Expression.Replace(m.Value, "+" + result);
                        m = regEx.Match(Expression);
                        continue;
                    case "/":
                        result = Convert.ToDouble(m.Groups[1].Value) / Convert.ToDouble(m.Groups[3].Value);
                        if ((result < 0) || (m.Index == 0)) Expression = regEx.Replace(Expression, result.ToString(), 1);
                        else Expression = regEx.Replace(Expression, "+" + result, 1);
                        m = regEx.Match(Expression);
                        continue;
                }
            }
            regEx = new Regex(@"([\+-]?\d+,*\d*[eE][\+-]?\d+|[\+-]?\d+,*\d*)([\+-])(-?\d+,*\d*[eE][\+-]?\d+|-?\d+,*\d*)");
            m = regEx.Match(Expression, 0);
            while (m.Success)
            {
                double result;
                switch (m.Groups[2].Value)
                {
                    case "+":
                        result = Convert.ToDouble(m.Groups[1].Value) + Convert.ToDouble(m.Groups[3].Value);
                        if ((result < 0) || (m.Index == 0)) Expression = regEx.Replace(Expression, result.ToString(), 1);
                        else Expression = regEx.Replace(Expression, "+" + result, 1);
                        m = regEx.Match(Expression);
                        continue;
                    case "-":
                        result = Convert.ToDouble(m.Groups[1].Value) - Convert.ToDouble(m.Groups[3].Value);
                        if ((result < 0) || (m.Index == 0)) Expression = regEx.Replace(Expression, result.ToString(), 1);
                        else Expression = regEx.Replace(Expression, "+" + result, 1);
                        m = regEx.Match(Expression);
                        continue;
                }
            }
            if (Expression.StartsWith("--")) Expression = Expression.Substring(2);
            return Expression;
        }

        private double Fact(double n)
        {
            return n == 0.0 ? 1.0 : n * Fact(n - 1.0);
        }
    }
}
