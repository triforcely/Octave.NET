namespace Octave.NET.Examples.Basics
{
    internal class Program
    {
        private static void Main()
        {
            using (var octave = new OctaveContext())
            {
                // Please note that there is no ";" at the end. This is important if we want to capture the output.
                var scalarResult = octave
                        .Execute("2 + 2")
                        .AsScalar();

                var vectorResult = octave
                    .Execute("[1 2 3 4 5]")
                    .AsVector();

                var matrixResult = octave
                    .Execute("[1 2 3 4 5; 5 4 3 2 1]")
                    .AsMatrix();

                var vec = new double[] { 1, 2, 3, 4, 5 };
                var anotherVector = octave.Execute(vec.ToOctave()); // [1 2 3 4 5]

                var mat = new[]
                {
                    new double[] { 1, 2, 3, 4, 5 },
                    new double[] { 1, 2, 3, 4, 5 }
                };
                var anotherMatrix = octave.Execute(mat.ToOctave());// [1 2 3 4 5;1 2 3 4 5]
            }
        }
    }
}