using FluentAssertions;
using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.UnitTests.Models;

public class ReportTests
{
    [Fact]
    public void MarkAsGenerated_SetsFilePathStatusAndGeneratedAt()
    {
        var report = new Report
        {
            Name = "Январь",
            Status = ReportStatus.InProgress
        };
 
        report.MarkAsGenerated("2025/1/report.csv");
 
        Assert.Equal("2025/1/report.csv", report.FilePath);
        Assert.Equal(ReportStatus.Generated, report.Status);
        Assert.True(report.GeneratedAt > DateTime.UtcNow.AddSeconds(-5));
    }
 
    [Fact]
    public void MarkAsGenerated_WhenCalledTwice_OverwritesFilePath()
    {
        var report = new Report { Status = ReportStatus.InProgress };
        
        report.MarkAsGenerated("old/path.csv");
        report.MarkAsGenerated("new/path.csv");
        
        Assert.Equal("new/path.csv", report.FilePath);
    }
 
    [Fact]
    public void Report_DefaultStatus_IsNotGenerated()
    {
        var report = new Report();
        
        Assert.NotEqual(ReportStatus.Generated, report.Status);
    }
}