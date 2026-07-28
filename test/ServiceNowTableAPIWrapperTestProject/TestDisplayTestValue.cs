using Blacus.ServiceNowTableAPIWrapper;
using ServiceNowTableAPIWrapperTestProject.Models;

namespace ServiceNowTableAPIWrapperTestProject
{

    [TestClass]
    public sealed class TestDisplayTestValue
    {

        [TestMethod]
        public void DisplayTestValue() =>
            runTest<DisplayTestValue>();

        [TestMethod]
        public void DisplayFalseTestValuePass1() =>
            runTest<DisplayFalseTestValue>();

        private static void runTest<T>() where T : IServiceNowTableModel // Add constraint to T
        {
            var testRunner = new TestRunner<T>();
            Assert.AreEqual(testRunner.OutputResult, testRunner.OutputSample);
        }

    }

}
