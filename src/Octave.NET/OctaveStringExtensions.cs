using System;
using System.Globalization;
using System.Linq;

namespace Octave.NET
{
    public static class OctaveStringExtensions
    {
        public static double AsScalar(this string input)
        {
            input = CleanInput(input);

            if (input.EndsWith("-Inf"))
                return double.MinValue;

            if (input.EndsWith("Inf"))
                return double.MaxValue;

            return ParseDouble(input);
        }

        public static double[] AsVector(this string input)
        {
            input = CleanInput(input);

            const char blank = (char) 32;
            var data = input.Split(blank);

            var size = data.Count(d => d.Length > 0);

            var result = new double[size];

            var iterator = 0;
            foreach (var number in data)
            {
                if (number.Length == 0)
                    continue;

                var parsed = ParseDouble(number);

                result[iterator] = parsed;
                iterator++;
            }

            return result;
        }

        public static double[][] AsMatrix(this string input)
        {
            input = CleanInput(input);

            var rows = input.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var matrix = new double[rows.Length][];

            for (var i = 0; i < matrix.Length; i++)
                matrix[i] = rows[i].Trim().AsVector();

            return matrix;
        }

        private static double ParseDouble(string number)
        {
            if (number.Contains("-Inf"))
                return double.MinValue;

            if (number.Contains("Inf"))
                return double.MaxValue;

            return double.Parse(number, CultureInfo.InvariantCulture);
        }

        private static string CleanInput(string input)
        {
            var index = input.IndexOf('=');
            if (index != -1)
                input = input.Substring(index + 1, input.Length - index - 1).Trim();

            return input;
        }
    }
}