using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Code.ConvertingForToLinq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.ConvertingForToLinq
{
    [TestClass]
    public class ConvertForToLinqTest
    {
        [TestMethod]
        public void Construct()
        {
            new Span<int>(-2, 30);
            Console.WriteLine(int.MinValue);
            Console.WriteLine(int.MaxValue);
        }

        [TestMethod]
        public void GeneralityOfReplace()
        {
            var input = new[] { "Hello", "World", "Bye", "Again" };
            var replaced = input.Replace(x => x.StartsWith("A"), x => new[] { x.ToLower()});
            var result = string.Join(", ", replaced);

            Assert.AreEqual("Hello, World, Bye, again", result);
        }
    }
}
