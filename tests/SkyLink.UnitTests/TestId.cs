namespace SkyLink.UnitTests;

internal static class TestId
{
    private static int _next;

    public static int Next() => Interlocked.Increment(ref _next);
}
