using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace MathBoard
{
    class Settings
    {
        public static class Line
        {
            public static readonly SolidColorBrush Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204));
            public static readonly double DefaultStrokeThickness = 1;
        }

        public static class Canvas
        {
            public static readonly SolidColorBrush Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
        }

        public static class Point
        {
            public static readonly SolidColorBrush Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204));
            public static readonly double Radius = 14.0;
            public static readonly double DefaultOpacity = 0.0;
            public static readonly double HoveredOpacity = 0.8;
        }

        public static class Plane
        {
            public static readonly int HorizontalLowerBound = -10;
            public static readonly int HorizontalUpperBound = 10;
            public static readonly int VerticalLowerBound = -10;
            public static readonly int VerticalUpperBound = 10;

            public static System.Windows.Point GetDimensions()
            {
                double width = VerticalUpperBound - VerticalLowerBound;
                double height = HorizontalUpperBound - HorizontalLowerBound;

                return new System.Windows.Point(width, height);
            }
        }
    }
}
