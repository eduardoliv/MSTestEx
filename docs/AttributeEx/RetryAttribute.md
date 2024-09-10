# RetryAttribute
- The __RetryAttribute__ is used on a test method to specify that it should be rerun if it fails, up to a maximum number of times.
- Only tests with an outcome of "Failed", can trigger a retry. For the complete set of possible test outcomes see [here](https://github.com/Microsoft/testfx/blob/master/src/TestFramework/MSTest.Core/UnitTestOutcome.cs).
- A test method can be retried up to a maximum of 10 times (the input argument representing the retry count will be automatically sanitized to be within the range 1 to 10).
- Each execution attempt is recorded as a child test.
- If a test method is rerun due to failure and passes on a subsequent run, the overall result will be reported based on the `finalAttemptResultOnly` flag:
  - If `finalAttemptResultOnly` is set to **`true`**, only the result of the last retry attempt is considered, meaning the final result could be **Passed** if the last retry passed.
  - If `finalAttemptResultOnly` is set to **`false`**, the overall result will be reported as **Failed**.

- In the case of data driven tests only those invocations of the test method that failed are subject to retry. For retries of data driven tests, the execution attempt number is not shown as part of the test method's display name.

## Arguments
- __retryCount__: An integer representing the number of retries (minimum of 1, maximum of 10).
- __finalAttemptResultOnly__ (optional, default `false`): A boolean value indicating whether only the result from the last retry attempt should be considered as the overall test result.


## Usage
- add a NuGet reference to the MSTestEx package.
- create / open an MSTestV2 based test project.
- add the following:
```
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.TestFramework.Extensions.AttributeEx;
using MSTest.TestFramework.Extensions.TestMethodEx;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethodEx]
        [Retry(5, true)]
        public void TestMethod1()
        {
            // Test logic that causes the test to fail.
        }
    }
}
```

## Applies to
MSTestEx v1.0.3

## Notes
 - Note that any surrounding TestInitialize and TestCleanup methods will be executed only once.