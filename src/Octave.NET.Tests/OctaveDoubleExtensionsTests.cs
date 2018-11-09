using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Octave.NET.Tests
{
    [TestClass]
    public class OctaveDoubleExtensionsTests
    {
        [TestMethod]
        public void VectorToOctave_ReturnsCorrectResult()
        {
            //arrange
            var input = new double[] { 1, 2, 3 };

            //act
            var res = input.ToOctave();

            //assert
            Assert.AreEqual(res, "[1 2 3]");
        }

        [TestMethod]
        public void EmptyVectorToOctave_ReturnsCorrectResult()
        {
            //arrange
            var input = new double[0];

            //act
            var res = input.ToOctave();

            //assert
            Assert.AreEqual(res, "[]");
        }

        [TestMethod]
        public void MatrixToOctave_ReturnsCorrectResult()
        {
            //arrange
            var input = new[]
            {
                new double[] {1, 2, 3},
                new double[] {3, 2, 1}
            };

            //act
            var res = input.ToOctave();

            //assert
            Assert.AreEqual(res, "[1 2 3;3 2 1]");
        }

        [TestMethod]
        public void EmptyMatrixToOctave_ReturnsCorrectResult()
        {
            //arrange
            var input = new double[0][];

            //act
            var res = input.ToOctave();

            //assert
            Assert.AreEqual(res, "[]");
        }
    }
}
