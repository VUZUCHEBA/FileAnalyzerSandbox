using FileAnalyzerSandbox.Domain.Entities;
using FileAnalyzerSandbox.Domain.Interfaces;
using FileAnalyzerSandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileAnalyzerSandbox.Infrastructure.Repositories;

public class FileAnalysisRepository : GenericRepository<FileAnalysis>, IFileAnalysisRepository
{
    public FileAnalysisRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FileAnalysis>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FileAnalysis>> GetByStatusAsync(AnalysisStatus status)
    {
        return await _dbSet
            .Where(f => f.Status == status)
            .ToListAsync();
    }

    public async Task<FileAnalysis?> GetWithLogsAsync(Guid id)
    {
        return await _dbSet
            .Include(f => f.AnalysisLogs)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
}