using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.TestFramework.Extensions.AttributeEx;
using MSTest.TestFramework.Extensions.TestMethodEx;
using Moq;
using System.Linq;
using System;

namespace MSTest.TestFramework.ExtensionsTests.AttributeExTests
{
    [TestClass]
    public class RetryAttributeTests
    {
        [TestMethod()]
        [DataRow(UnitTestOutcome.Failed, 1, 1)]
        [DataRow(UnitTestOutcome.Failed, 5, 5)]
        [DataRow(UnitTestOutcome.Failed, 15, 1)]
        [DataRow(UnitTestOutcome.Inconclusive, 1, 1)]
        [DataRow(UnitTestOutcome.Inconclusive, 5, 1)]
        [DataRow(UnitTestOutcome.Inconclusive, 15, 1)]
        [DataRow(UnitTestOutcome.Passed, 1, 1)]
        [DataRow(UnitTestOutcome.Passed, 5, 1)]
        [DataRow(UnitTestOutcome.Passed, 15, 1)]
        [DataRow(UnitTestOutcome.InProgress, 1, 1)]
        [DataRow(UnitTestOutcome.InProgress, 5, 1)]
        [DataRow(UnitTestOutcome.InProgress, 15, 1)]
        [DataRow(UnitTestOutcome.Error, 1, 1)]
        [DataRow(UnitTestOutcome.Error, 5, 1)]
        [DataRow(UnitTestOutcome.Error, 15, 1)]
        [DataRow(UnitTestOutcome.Timeout, 1, 1)]
        [DataRow(UnitTestOutcome.Timeout, 5, 1)]
        [DataRow(UnitTestOutcome.Timeout, 15, 1)]
        [DataRow(UnitTestOutcome.Aborted, 1, 1)]
        [DataRow(UnitTestOutcome.Aborted, 5, 1)]
        [DataRow(UnitTestOutcome.Aborted, 15, 1)]
        [DataRow(UnitTestOutcome.Unknown, 1, 1)]
        [DataRow(UnitTestOutcome.Unknown, 5, 1)]
        [DataRow(UnitTestOutcome.Unknown, 15, 1)]
        [DataRow(UnitTestOutcome.NotRunnable, 1, 1)]
        [DataRow(UnitTestOutcome.NotRunnable, 5, 1)]
        [DataRow(UnitTestOutcome.NotRunnable, 15, 1)]
        public void RetryTestForAllTestOutcomes(
            UnitTestOutcome RequiredTestOutCome,
            int RequestedRetryCount,
            int expectedExecutionAttempts,
            bool retryOnFail = false)
        {
            // Arrange
            TestResult[] expected =
                {
                    new TestResult() { Outcome = RequiredTestOutCome }
                };

            var mockTestMethod = new Mock<ITestMethod>();
            mockTestMethod.Setup(tm => tm.GetAllAttributes(false)).Returns(() =>
                {
                    Attribute[] attr =
                        {
                            new RetryAttribute(RequestedRetryCount, retryOnFail),
                        };
                    return attr;
                }
            );

            var args = It.IsAny<object[]>();
            mockTestMethod.Setup(tm => tm.Invoke(args)).Returns(() =>
                {
                    return expected[0];
                }
            );


            // Act
            var retriableTestMethod = new TestMethodExAttribute();
            var tr = retriableTestMethod.Execute(mockTestMethod.Object);

            // Assert
            mockTestMethod.Verify(tm => tm.Invoke(args), Times.Exactly(expectedExecutionAttempts));
            Assert.AreEqual(tr.Length, expectedExecutionAttempts);
            Assert.IsTrue((tr.All((r) => r.Outcome == RequiredTestOutCome)));
        }

        [TestMethod()]
        [DataRow(UnitTestOutcome.Failed, 1, 1, true)]
        [DataRow(UnitTestOutcome.Failed, 5, 5, true)]
        [DataRow(UnitTestOutcome.Failed, 3, 3, true)]
        [DataRow(UnitTestOutcome.Passed, 1, 1, true)]
        [DataRow(UnitTestOutcome.Passed, 3, 1, true)]
        [DataRow(UnitTestOutcome.Passed, 3, 2, true)]
        [DataRow(UnitTestOutcome.Passed, 3, 3, true)]
        [DataRow(UnitTestOutcome.Passed, 6, 3, true)]
        [DataRow(UnitTestOutcome.Inconclusive, 3, 1, true)]
        [DataRow(UnitTestOutcome.Inconclusive, 3, 3, true)]
        [DataRow(UnitTestOutcome.InProgress, 3, 1, true)]
        [DataRow(UnitTestOutcome.Timeout, 5, 5, true)]
        [DataRow(UnitTestOutcome.Aborted, 2, 2, true)]
        [DataRow(UnitTestOutcome.Unknown, 3, 3, true)]
        [DataRow(UnitTestOutcome.NotRunnable, 1, 1, true)]
        [DataRow(UnitTestOutcome.InProgress, 5, 1, true)]
        [DataRow(UnitTestOutcome.Timeout, 9, 9, true)]
        public void RetryTestForFinalAttemptResultOnlyTestOutcomes(
            UnitTestOutcome requiredTestOutCome,
            int requestedRetryCount,
            int expectedExecutionAttempts,
            bool finalAttemptResultOnly)
        {
            // Arrange
            TestResult[] outcomes = new TestResult[]
            {
        new TestResult() { Outcome = requiredTestOutCome },
            };

            var mockTestMethod = new Mock<ITestMethod>();
            int attempt = 0;

            // Configure the attributes for the mock test method
            mockTestMethod.Setup(tm => tm.GetAllAttributes(false)).Returns(() =>
            {
                Attribute[] attr =
                {
            new RetryAttribute(requestedRetryCount, finalAttemptResultOnly),
        };
                return attr;
            });

            var args = It.IsAny<object[]>();

            // Configure the behavior of the mock method based on finalAttemptResultOnly
            mockTestMethod.Setup(tm => tm.Invoke(args)).Returns(() =>
            {
                attempt++;
                if (finalAttemptResultOnly)
                {
                    // Simulate retries based on the requested retry count
                    if (attempt < expectedExecutionAttempts)
                    {
                        return new TestResult() { Outcome = UnitTestOutcome.Failed };
                    }
                }
                return outcomes[0];
            });

            // Act
            var retriableTestMethod = new TestMethodExAttribute();
            var tr = retriableTestMethod.Execute(mockTestMethod.Object);

            // Assert
            mockTestMethod.Verify(tm => tm.Invoke(args), Times.Exactly(expectedExecutionAttempts));
            Assert.AreEqual(tr.Length, 1);
            Assert.IsTrue(tr.All(r => r.Outcome == requiredTestOutCome));
        }
    }
}
