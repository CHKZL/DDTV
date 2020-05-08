using Auxiliary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AuxiliaryTests
{
    [TestClass]
    public class TestArgumentParser
    {
        [TestMethod]
        public void TestNoArgument()
        {
            Dictionary<string, string> result =
                ArgumentParser.parse(new string[] { "DDTV.exe" });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TestOnlyOneOption()
        {
            Dictionary<string, string> result =
                ArgumentParser.parse(new string[] { "DDTV.exe", "-m" });
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("m"));
        }

        [TestMethod]
        public void TestOnlyOneKVArgument()
        {
            Dictionary<string, string> result =
                ArgumentParser.parse(new string[] { "DDTV.exe", "--minimized=true" });
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("minimized"));
            Assert.AreEqual("true", result["minimized"]);
        }

        [TestMethod]
        public void TestCombinedArguments()
        {
            Dictionary<string, string> result =
                   ArgumentParser.parse(new string[] { "DDTV.exe", "-m", "--minimized=true" });
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("m"));
            Assert.IsTrue(result.ContainsKey("minimized"));
            Assert.AreEqual("true", result["minimized"]);
        }

        [TestMethod]
        public void TestBadArguments()
        {
            Assert.ThrowsException<FormatException>(() =>
            {
                ArgumentParser.parse(new string[] { "DDTV.exe", "--minimized=true", "m" });
            });
        }
    }
}
