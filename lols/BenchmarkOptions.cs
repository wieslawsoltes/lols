using System;

namespace lols;

internal static class BenchmarkOptions
{
    private const string FastTextEnvVar = "LOLS_USE_FAST_TEXTBLOCK";

    public static readonly bool UseFastTextBlock =
        !string.Equals(Environment.GetEnvironmentVariable(FastTextEnvVar), "0", StringComparison.OrdinalIgnoreCase);
}
