namespace Parser.Core.Pipelines;

public static class RetryPolicy
{
    public static async ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> action, int retries, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await action(ct);
                return;
            }
            catch when (attempt < retries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25 * (attempt + 1)), ct);
            }
        }
    }
}
