using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octave.NET.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Octave.NET.Tests
{
    [TestClass]
    public class OctaveContextTests
    {
        private const int Timeout = 15000;

        public OctaveContextTests()
        {
            OctaveContext.OctaveSettings.PreventColdStarts = true;
        }

        [TestMethod]
        public void WhenCorrectScript_ShouldReturnString()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute("123");

                //assert
                Assert.IsTrue(result.StartsWith("ans"));
                Assert.IsTrue(result.EndsWith("123"));
            }
        }

        [TestMethod]
        public void WhenPassedMaxDoubleValue_ShouldReturnMaxValue()
        {
            const double input = double.MaxValue;

            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute(input.ToString(CultureInfo.InvariantCulture), Timeout).AsScalar();

                //assert
                Assert.AreEqual(input, result);
            }
        }

        [TestMethod]
        public void WhenPassedMinDoubleValue_ShouldReturnMinValue()
        {
            const double input = double.MinValue;

            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute(input.ToString(CultureInfo.InvariantCulture), Timeout).AsScalar();

                //assert
                Assert.AreEqual(input, result);
            }
        }

        [TestMethod]
        public void WhenPassedDoubleInRange_ShouldReturnCorrectValue()
        {
            const double input = 15;

            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute(input.ToString(CultureInfo.InvariantCulture), Timeout).AsScalar();

                //assert
                Assert.AreEqual(input, result);
            }
        }

        [TestMethod]
        public void WhenPassedVectorString_ShouldReturnCorrectVector()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute("[0 1 2 3 4 ]", Timeout).AsVector();

                //assert
                CollectionAssert.AreEqual(new double[] { 0, 1, 2, 3, 4 }, result);
            }
        }

        [TestMethod]
        public void WhenPassedMatrixString_ShouldReturnCorrectMatrix()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var result = octave.Execute("[0 1 2 3 4 ; 4 3 2 1 0]", Timeout).AsMatrix();

                //assert
                CollectionAssert.AreEqual(new double[] { 0, 1, 2, 3, 4 }, result[0]);
                CollectionAssert.AreEqual(new double[] { 4, 3, 2, 1, 0 }, result[1]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OctaveScriptError))]
        public void WhenPassedInvalidScript_ShouldThrowException()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                octave.Execute("'123", Timeout);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OctaveCommandTimeoutException))]
        public void WhenScriptExecutionTakesTooLong_ShouldThrowException()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                octave.Execute("pause(100)", 25);
            }
        }

        [TestMethod]
        public void WhenInputFunctionUsedInExecuteMultiple_CapturesInput()
        {
            //arrange 
            using (var octave = new OctaveContext())
            {
                //act
                var res = octave.ExecuteMultiple(new string[]
                {
                    "input(\"prompt\")",
                    "15"
                }).AsScalar();

                //assert
                Assert.AreEqual(15, res);
            }
        }

        [TestMethod]
        public void WhenHeavilyMultithreaded_ThrowsNoExceptions()
        {
            var tasks = new List<Task>();

            //arrange
            for (var i = 0; i < Environment.ProcessorCount * 2; i++)
            {
                var task = Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        using (var octave = new OctaveContext())
                        {
                            //act
                            var result = octave.Execute("2+2").AsScalar();

                            //assert
                            Assert.AreEqual(4, result);
                        }
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}