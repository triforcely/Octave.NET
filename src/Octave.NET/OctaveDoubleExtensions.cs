using System.Globalization;
using System.Text;

namespace Octave.NET
{
    public static class OctaveDoubleExtensions
    {
        private static StringBuilder AppendVector(this StringBuilder stringBuilder, double[] vector)
        {
            if (vector.Length > 0)
            {
                foreach (double d in vector)
                {
                    stringBuilder.Append(d.ToString(CultureInfo.InvariantCulture)).Append(' ');
                }

                stringBuilder.Length--;
            }

            return stringBuilder;
        }

        /// <summary>
        /// Convert array of doubles to octave input (vector).
        /// </summary>
        public static string ToOctave(this double[] vector)
        {
            return new StringBuilder().Append('[').AppendVector(vector).Append(']').ToString();
        }

        /// <summary>
        /// Convert two-dimensional array of doubles to octave input (matrix).
        /// </summary>
        public static string ToOctave(this double[][] matrix)
        {
            StringBuilder stringBuilder = new StringBuilder().Append('[');

            if (matrix.Length > 0)
            {
                foreach (double[] vector in matrix)
                {
                    stringBuilder.AppendVector(vector).Append(';');
                }

                stringBuilder.Length--;
            }

            return stringBuilder.Append(']').ToString();
        }
    }
}
