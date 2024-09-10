﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.TestFramework.Extensions.AttributeEx;
using System.Collections.Generic;
using System.Linq;

namespace MSTest.TestFramework.Extensions.TestMethodEx
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestMethodExAttribute : TestMethodAttribute
    {
        private TestResult[] executeWithRepeatAndRetry(
            ITestMethod testMethod,
            int repeatCount,
            int retryCount,
            bool finalAttemptResultOnly = false)
        {
            var res = new List<TestResult>();

            for (int count = 0; count < repeatCount; count++)
            {
                var testResults = executeWithRetryOnFailure(testMethod, retryCount);

                if (finalAttemptResultOnly)
                {
                    res.Add(testResults.Last());
                }
                else
                {
                    res.AddRange(testResults);
                }

                if (testResults.All((tr) => tr.Outcome == UnitTestOutcome.Passed))
                {
                    continue;
                }

                break;
            }

            return res.ToArray();
        }


        private TestResult[] executeWithRetryOnFailure(
            ITestMethod testMethod,
            int retryCount)
        {
            var res = new List<TestResult>();

            for (int count = 0; count < retryCount; count++)
            {
                var testResults = base.Execute(testMethod);
                res.AddRange(testResults);

                if (testResults.Any((tr) => tr.Outcome == UnitTestOutcome.Failed))
                {
                    continue;
                }

                break;
            }

            return res.ToArray();
        }

        public override TestResult[] Execute(
            ITestMethod testMethod)
        {
            // NOTE
            // This implementation will need to be refactored as we add more
            // execution variations.

            int retryCount = 1;
            int repeatCount = 1;
            bool finalAttemptResultOnly = false;

            Attribute[] attr = testMethod.GetAllAttributes(false);

            var retryAttr = attr.OfType<RetryAttribute>().FirstOrDefault();
            if (retryAttr != null)
            {
                retryCount = retryAttr.Value;
                finalAttemptResultOnly = retryAttr.FinalAttemptResultOnly;
            }
            var repeatAttr = attr.OfType<RepeatAttribute>().FirstOrDefault();
            if (repeatAttr != null)
            {
                repeatCount = repeatAttr.Value;
            }

            var res = executeWithRepeatAndRetry(testMethod, repeatCount, retryCount, finalAttemptResultOnly);
            return res;
        }
    }
}
