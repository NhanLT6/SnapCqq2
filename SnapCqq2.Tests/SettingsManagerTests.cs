using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class SettingsManagerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public SettingsManagerTests()
    {
        Directory.CreateDirectory(_tempDir);
        SettingsManager.FilePath = Path.Combine(_tempDir, "settings.json");
    }

    public void Dispose()
    {
        SettingsManager.FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SnapCqq2", "settings.json");
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileAbsent()
    {
        var s = SettingsManager.Load();
        Assert.Equal(4, s.IdleThresholdMinutes);
        Assert.True(s.ActiveDays[0]);   // Monday on
        Assert.False(s.ActiveDays[5]);  // Saturday off
    }

    [Fact]
    public void Save_ThenLoad_RoundTrips()
    {
        var original = new AppSettings
        {
            IdleThresholdMinutes = 7,
            WorkStart = new TimeOnly(8, 30),
            UseMouseClick = true,
            DefaultMode = "Manual"
        };

        SettingsManager.Save(original);
        var loaded = SettingsManager.Load();

        Assert.Equal(7, loaded.IdleThresholdMinutes);
        Assert.Equal(new TimeOnly(8, 30), loaded.WorkStart);
        Assert.True(loaded.UseMouseClick);
        Assert.Equal("Manual", loaded.DefaultMode);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileCorrupt()
    {
        File.WriteAllText(SettingsManager.FilePath, "not json {{{{");
        var s = SettingsManager.Load();
        Assert.Equal(4, s.IdleThresholdMinutes);
    }
}
