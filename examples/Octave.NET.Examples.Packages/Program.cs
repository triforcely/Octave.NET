namespace Octave.NET.Examples.Packages
{
    internal class Program
    {
        private static void Main()
        {
            using (var octave = new OctaveContext())
            {
                octave.Execute("pkg load fuzzy-logic-toolkit;");
                double result = octave
                    .Execute("algebraic_product(5,2)")
                    .AsScalar(); // 10
            }
        }
    }
}