namespace Octave.NET.Examples.Packages
{
    internal class Program
    {
        private static void Main()
        {
            using (var octave = new OctaveContext())
            {
                octave.Execute("pkg load fuzzy-logic-toolkit;"); // I don't care about output from this command so semicolon is fine

                var algebraicPoduct = octave.Execute("algebraic_product(5,2)").AsScalar(); // 10
            }
        }
    }
}