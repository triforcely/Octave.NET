using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Octave.NET.Tests
{
    [TestClass]
    public class OctaveStringExtensionsTests
    {
        [TestMethod]
        public void StringAsScalar_ReturnsCorrectResult()
        {
            //arrange
            var input = "ans = 34";

            //act
            var res = input.AsScalar();

            //assert
            Assert.AreEqual(res, 34);
        }

        [TestMethod]
        public void StringInfinityAsScalar_ReturnsCorrectResult()
        {
            //arrange
            var input = "ans = Inf";

            //act
            var res = input.AsScalar();

            //assert
            Assert.AreEqual(res, double.MaxValue);
        }

        [TestMethod]
        public void StringMinusInfinityAsScalar_ReturnsCorrectResult()
        {
            //arrange
            var input = "ans = -Inf";

            //act
            var res = input.AsScalar();

            //assert
            Assert.AreEqual(res, double.MinValue);
        }

        [TestMethod]
        public void StringAsVector_ReturnsCorrectResult()
        {
            //arrange
            var input = "ans = 1 2 3";

            //act
            var res = input.AsVector();

            //assert
            Assert.AreEqual(res.Length, 3);
            Assert.AreEqual(res[0], 1);
            Assert.AreEqual(res[1], 2);
            Assert.AreEqual(res[2], 3);
        }

        [TestMethod]
        public void StringWithInfinitiesAsVector_ReturnsCorrectResult()
        {
            //arrange
            var input = "ans = 1 -Inf Inf";

            //act
            var res = input.AsVector();

            //assert
            Assert.AreEqual(res.Length, 3);
            Assert.AreEqual(res[0], 1);
            Assert.AreEqual(res[1], double.MinValue);
            Assert.AreEqual(res[2], double.MaxValue);
        }

        [TestMethod]
        public void StringAsMatrix_ReturnsCorrectResult()
        {
            //arrange
            var input = $"ans = 4 5 6{Environment.NewLine}7 8 9";

            //act
            var res = input.AsMatrix();

            //assert
            Assert.AreEqual(res.Length, 2);
            Assert.AreEqual(res[0].Length, 3);
            Assert.AreEqual(res[0][0], 4);
            Assert.AreEqual(res[0][1], 5);
            Assert.AreEqual(res[0][2], 6);
            Assert.AreEqual(res[1].Length, 3);
            Assert.AreEqual(res[1][0], 7);
            Assert.AreEqual(res[1][1], 8);
            Assert.AreEqual(res[1][2], 9);
        }
    }
}
