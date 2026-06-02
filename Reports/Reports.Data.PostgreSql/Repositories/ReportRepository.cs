using Microsoft.EntityFrameworkCore;
using Reports.Abstractions.Storage.Repositories;
using Reports.Data.PostgreSql.Entities;
using Reports.Exceptions;
using Reports.Models;
using Reports.SharedKernel.Enums;

namespace Reports.Data.PostgreSql.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateReport(Report report)
    {
        if (await _context.Reports.FirstOrDefaultAsync(r => r.Name == report.Name) != null)
        {
            throw new ClientErrorException("Уже существует отчёт с таким названием.");
        }
        
        var entity = new ReportEntity()
        {
            Name = report.Name,
            PeriodFrom = report.PeriodFrom,
            PeriodTo = report.PeriodTo,
            Status = report.Status,
            FilePath = report.FilePath ?? string.Empty,
            GeneratedAt = report.GeneratedAt
        };
        
        _context.Reports.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task<Report> GetReportById(Guid reportId)
    {
        var entity = await _context.Reports.FindAsync(reportId);
        if (entity == null)
        {
            throw new ClientErrorException("Не существует отчёта с таким идентификатором");
        }
        
        return new Report
        {
            Name = entity.Name,
            PeriodFrom = entity.PeriodFrom,
            PeriodTo = entity.PeriodTo,
            Status = entity.Status,
            FilePath = entity.FilePath,
            GeneratedAt = entity.GeneratedAt
        };
    }

    public async Task<Report> GetReportByName(string reportName)
    {
        var entity = await _context.Reports.FirstOrDefaultAsync(r => r.Name == reportName);
        if (entity == null)
        {
            throw new ClientErrorException("Не существует отчёта с таким названием");
        }
        
        return new Report
        {
            Name = entity.Name,
            PeriodFrom = entity.PeriodFrom,
            PeriodTo = entity.PeriodTo,
            Status = entity.Status,
            FilePath = entity.FilePath,
            GeneratedAt = entity.GeneratedAt
        };
    }

    public async Task<IEnumerable<Report>> GetGeneratedReports()
    {
        return await _context.Reports.Where(r => r.Status == ReportStatus.Generated)
            .Select(r => new Report
            {
                Name = r.Name,
                PeriodFrom = r.PeriodFrom,
                PeriodTo = r.PeriodTo,
                Status = r.Status,
                FilePath = r.FilePath,
                GeneratedAt = r.GeneratedAt
            })
            .ToListAsync();
    }

    public async Task UpdateReport(Guid reportId, Report report)
    {
        var entity = await _context.Reports.FindAsync(reportId);
        if (entity == null)
        {
            throw new ClientErrorException("Не существует отчёта с таким идентификатором");
        }
        
        entity.FilePath = report.FilePath;
        entity.Status = report.Status;
        
        _context.Reports.Update(entity);
        await _context.SaveChangesAsync();
    }
}