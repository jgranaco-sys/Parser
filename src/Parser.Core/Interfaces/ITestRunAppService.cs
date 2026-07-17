using Parser.Core.Models;

namespace Parser.Core.Interfaces;

public interface ITestRunAppService
{
    Task<TestResult> RunTestAsync(string profileId, string payload, string inputFormat);
}
