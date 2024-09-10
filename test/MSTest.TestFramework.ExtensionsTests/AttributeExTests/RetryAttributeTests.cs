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
        [DataRow(UnitTestOutcome.Failed, 1, 1, true)] // retryOnFail = true, Expected to retry once on failure
        [DataRow(UnitTestOutcome.Failed, 5, 5, true)] // retryOnFail = true, Expected to retry 5 times on failure
        [DataRow(UnitTestOutcome.Passed, 1, 1, true)] // retryOnFail = true, Test should pass immediately
        [DataRow(UnitTestOutcome.Passed, 6, 3, true)] // retryOnFail = true, Test should pass on second attempt
        [DataRow(UnitTestOutcome.Failed, 1, 1, false)] // retryOnFail = false, Test should not retry if fails
        [DataRow(UnitTestOutcome.Passed, 1, 1, false)] // retryOnFail = false, Test should pass immediately
        public void RetryTestForRetryOnFailTestOutcomes(
            UnitTestOutcome requiredTestOutCome,
            int requestedRetryCount,
            int expectedExecutionAttempts,
            bool retryOnFail)
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
                    new RetryAttribute(requestedRetryCount, retryOnFail),
                };
                return attr;
            });

            var args = It.IsAny<object[]>();

            // Configure the behavior of the mock method based on retryOnFail
            mockTestMethod.Setup(tm => tm.Invoke(args)).Returns(() =>
            {
                attempt++;
                if (retryOnFail)
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
            Assert.AreEqual(tr.Length, requiredTestOutCome == UnitTestOutcome.Passed ? 1 : expectedExecutionAttempts);
            Assert.IsTrue(tr.All(r => r.Outcome == requiredTestOutCome));
        }
    }
}
