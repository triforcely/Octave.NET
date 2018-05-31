using System.Globalization;
using System.Linq;

namespace Octave.NET
{
    public static class OctaveDoubleExtensions
    {
        private static string BuildVector(double[] vector)
        {
            return vector.Select(x => x.ToString(CultureInfo.InvariantCulture)).Aggregate((a, b) => $"{a} {b}");
        }

        private static string BuildMatrix(double[][] matrix)
        {
            return matrix.Select(BuildVector).Aggregate((a, b) => $"{a};{b}");
        }

        /// <summary>
        /// Convert array of doubles to octave input.
        /// </summary>
        public static string ToOctave(this double[] vector)
        {
            return $"[{BuildVector(vector)}]";
        }

        public static string ToOctave(this double[][] matrix)
        {
            return $"[{BuildMatrix(matrix)}]";
        }
    }
}
