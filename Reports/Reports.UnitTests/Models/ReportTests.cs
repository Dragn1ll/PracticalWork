using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.UnitTests.Models;

public class ReportModelTests
{
    [Fact]
    public void MarkAsGenerated_SetsFilePathAndStatus()
    {
        var report = new Report { Name = "Тест", Status = ReportStatus.InProgress };
 
        report.MarkAsGenerated("2025/1/report.csv");
 
        Assert.Equal("2025/1/report.csv", report.FilePath);
        Assert.Equal(ReportStatus.Generated, report.Status);
        Assert.True(report.GeneratedAt > DateTime.UtcNow.AddSeconds(-5));
    }
 
    [Fact]
    public void MarkAsGenerated_Twice_OverwritesPreviousFilePath()
    {
        var report = new Report { Status = ReportStatus.InProgress };
        report.MarkAsGenerated("old.csv");
        report.MarkAsGenerated("new.csv");
 
        Assert.Equal("new.csv", report.FilePath);
        Assert.Equal(ReportStatus.Generated, report.Status);
    }
 
    [Fact]
    public void Report_DefaultStatus_IsInProgress_OrDefault()
    {
        var report = new Report();
        Assert.NotEqual(ReportStatus.Generated, report.Status);
    }
}
