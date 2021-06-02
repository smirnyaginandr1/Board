using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace MathBoard
{
    class Matrix
    {
        static Point translation;
        static Point scaling;
        static Point reflection;

        static double[,] tranformationMatrix;

        static Matrix()
        {
            translation = new Point(0.0, 0.0);
            scaling = new Point(1.0, 1.0);
            reflection = new Point(1.0, 1.0);

            tranformationMatrix = new double[,]
            {
                { 1.0, 0.0, 0.0 },
                { 0.0, 1.0, 0.0 },
                { 0.0, 0.0, 1.0 }
            };
        }

        public static List<Point> PerformTransformation(List<Point> points)
        {
            var transformedPoints = new List<Point>();

            foreach (var point in points)
            {
                double x = tranformationMatrix[0, 0] * point.X + tranformationMatrix[0, 1] * point.Y + tranformationMatrix[0, 2];
                double y = tranformationMatrix[1, 0] * point.X + tranformationMatrix[1, 1] * point.Y + tranformationMatrix[1, 2];
                transformedPoints.Add(new Point(x, y));
            }

            return transformedPoints;
        }

        public static void InitialiseTranslation(double x, double y)
        {
            translation.X = x;
            translation.Y = y;
        }

        public static void InitialiseScaling(double x, double y)
        {
            scaling.X = x;
            scaling.Y = y;
        }

        public static void Rebuild()
        {
            tranformationMatrix[0, 2] = translation.X;
            tranformationMatrix[1, 2] = translation.Y;
            tranformationMatrix[0, 0] = scaling.X * reflection.X;
            tranformationMatrix[1, 1] = scaling.Y * reflection.Y;
        }

        public static bool FlipVertically
        {
            set
            {
                reflection.Y = value ? -1 : 1;
            }
        }
    }
}
