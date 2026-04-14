using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class IdleDetectorTests
{
    [Theory]
    [InlineData(1,  30_000)]   // 1 min  → 30 s (floor)
    [InlineData(2,  30_000)]   // 2 min  → 30 s (floor)
    [InlineData(4,  60_000)]   // 4 min  → 60 s
    [InlineData(5,  75_000)]   // 5 min  → 75 s
    [InlineData(10, 150_000)]  // 10 min → 150 s
    [InlineData(20, 300_000)]  // 20 min → 300 s
    public void ComputePollIntervalMs_ReturnsCorrectInterval(int thresholdMin, int expectedMs)
    {
        Assert.Equal(expectedMs, IdleDetector.ComputePollIntervalMs(thresholdMin));
    }

    [Fact]
    public void GetIdleTime_ReturnsNonNegative()
    {
        Assert.True(IdleDetector.GetIdleTime() >= TimeSpan.Zero);
    }
}
