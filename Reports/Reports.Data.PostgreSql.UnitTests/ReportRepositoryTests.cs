using Microsoft.EntityFrameworkCore;
using Reports.Data.PostgreSql.Entities;
using Reports.Data.PostgreSql.Repositories;
using Reports.Exceptions;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Data.PostgreSql.UnitTests;

public class ReportRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"rpt_{Guid.NewGuid()}");
 
    [Fact]
    public async Task CreateReport_WhenNameUnique_SavesAndReturnsId()
    {
        await using var db = NewDb();
        var repo = new ReportRepository(db);
        var report = new Report
        {
            Name = "Январь 2025", 
            Status = ReportStatus.InProgress,
            PeriodFrom = new DateOnly(2025, 1, 1), 
            PeriodTo = new DateOnly(2025, 1, 31)
        };
 
        var id = await repo.CreateReport(report);
 
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await db.Reports.AnyAsync(r => r.Id == id));
    }
 
    [Fact]
    public async Task CreateReport_WhenNameDuplicate_ThrowsClientErrorException()
    {
        await using var db = NewDb();
        db.Reports.Add(new ReportEntity
        {
            Name = "Январь 2025", 
            Status = ReportStatus.Generated,
            PeriodFrom = new DateOnly(2025, 1, 1), 
            PeriodTo = new DateOnly(2025, 1, 31),
            FilePath = "f.csv"
        });
        await db.SaveChangesAsync();
 
        var repo = new ReportRepository(db);
        await Assert.ThrowsAsync<ClientErrorException>(() => repo.CreateReport(new Report
        {
            Name = "Январь 2025", 
            Status = ReportStatus.InProgress,
            PeriodFrom = new DateOnly(2025, 1, 1), 
            PeriodTo = new DateOnly(2025, 1, 31)
        }));
    }
 
    [Fact]
    public async Task GetReportById_WhenExists_ReturnsReport()
    {
        await using var db = NewDb();
        var entity = new ReportEntity
        {
            Name = "Тест", 
            Status = ReportStatus.Generated,
            PeriodFrom = new DateOnly(2025, 1, 1), 
            PeriodTo = new DateOnly(2025, 1, 31), 
            FilePath = "f.csv"
        };
        db.Reports.Add(entity);
        await db.SaveChangesAsync();
 
        var repo = new ReportRepository(db);
        var report = await repo.GetReportById(entity.Id);
 
        Assert.Equal("Тест", report.Name);
        Assert.Equal(ReportStatus.Generated, report.Status);
    }
 
    [Fact]
    public async Task GetReportById_WhenNotFound_ThrowsClientErrorException()
    {
        await using var db = NewDb();
        var repo = new ReportRepository(db);
 
        await Assert.ThrowsAsync<ClientErrorException>(() => repo.GetReportById(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetReportByName_WhenExists_ReturnsReport()
    {
        await using var db = NewDb();
        db.Reports.Add(new ReportEntity
        {
            Name = "Февраль", 
            Status = ReportStatus.Generated,
            PeriodFrom = new DateOnly(2025, 2, 1), 
            PeriodTo = new DateOnly(2025, 2, 28), 
            FilePath = "f.csv"
        });
        await db.SaveChangesAsync();
 
        var repo = new ReportRepository(db);
        var report = await repo.GetReportByName("Февраль");
 
        Assert.Equal("Февраль", report.Name);
    }
 
    [Fact]
    public async Task GetReportByName_WhenNotFound_ThrowsClientErrorException()
    {
        await using var db = NewDb();
        var repo = new ReportRepository(db);
 
        await Assert.ThrowsAsync<ClientErrorException>(() => repo.GetReportByName("Несуществующий"));
    }
 
    [Fact]
    public async Task GetGeneratedReports_ReturnsOnlyGeneratedStatus()
    {
        await using var db = NewDb();
        db.Reports.AddRange(
            new ReportEntity
            {
                Name = "A", 
                Status = ReportStatus.Generated, 
                PeriodFrom = DateOnly.MinValue, 
                PeriodTo = DateOnly.MinValue, 
                FilePath = "a.csv"
            },
            new ReportEntity
            {
                Name = "B", 
                Status = ReportStatus.InProgress, 
                PeriodFrom = DateOnly.MinValue, 
                PeriodTo = DateOnly.MinValue, 
                FilePath = ""
            },
            new ReportEntity
            {
                Name = "C", 
                Status = ReportStatus.Error, 
                PeriodFrom = DateOnly.MinValue, 
                PeriodTo = DateOnly.MinValue, 
                FilePath = ""
            }
        );
        await db.SaveChangesAsync();
 
        var repo = new ReportRepository(db);
        var result = (await repo.GetGeneratedReports()).ToList();
 
        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }
 
    [Fact]
    public async Task UpdateReport_WhenExists_UpdatesStatusAndFilePath()
    {
        await using var db = NewDb();
        var entity = new ReportEntity
        {
            Name = "X", Status = ReportStatus.InProgress,
            PeriodFrom = new DateOnly(2025, 1, 1), 
            PeriodTo = new DateOnly(2025, 1, 31), 
            FilePath = ""
        };
        db.Reports.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
 
        var repo = new ReportRepository(db);
        await repo.UpdateReport(entity.Id, new Report
        {
            Name = "X", Status = ReportStatus.Generated, FilePath = "new/path.csv"
        });
 
        var updated = await db.Reports.FindAsync(entity.Id);
        Assert.Equal(ReportStatus.Generated, updated!.Status);
        Assert.Equal("new/path.csv", updated.FilePath);
    }
 
    [Fact]
    public async Task UpdateReport_WhenNotFound_ThrowsClientErrorException()
    {
        await using var db = NewDb();
        var repo = new ReportRepository(db);
 
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            repo.UpdateReport(Guid.NewGuid(), new Report { Status = ReportStatus.Error }));
    }
}