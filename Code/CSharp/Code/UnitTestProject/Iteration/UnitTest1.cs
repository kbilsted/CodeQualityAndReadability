using System;

using Code.Iteration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.Iteration
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var numbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sut = new IterationExamples();
            var expected = sut.Gotos(numbers);
            Console.WriteLine(expected);

            Assert.AreEqual(expected, sut.Gotos(numbers));
            Assert.AreEqual(expected, sut.UnboundWhile(numbers));
            Assert.AreEqual(expected, sut.BoundedWhile(numbers));
            Assert.AreEqual(expected, sut.For(numbers));
            Assert.AreEqual(expected, sut.ForWithExtractMethod(numbers));
            Assert.AreEqual(expected, sut.Foreach(numbers));
            Assert.AreEqual(expected, sut.Enumerator(numbers));
            Assert.AreEqual(expected, sut.Linq(numbers));
        }

        [TestMethod]
        public void TestMethod2()
        {
            var numbers = new[] { 1, 1, 2, 1, 1, 5, 1, 5 };
            var sut = new ComplexIteration();

            var expected = sut.IterationSkippingSome(numbers);
            Console.WriteLine(expected);

            Assert.AreEqual(expected, sut.IterationSkippingSome(numbers));
            Assert.AreEqual(expected, sut.IterationSkippingSomeExtracted(numbers));
            Assert.AreEqual(expected, sut.IterationSkippingSomeExtractedAndSkipLogic(numbers));

            
        }
    }
}
