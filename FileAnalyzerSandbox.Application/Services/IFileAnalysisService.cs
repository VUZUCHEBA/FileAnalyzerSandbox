using FileAnalyzerSandbox.Application.DTOs;

namespace FileAnalyzerSandbox.Application.Services;

public interface IFileAnalysisService
{
    Task<FileAnalysisDto> UploadFileAsync(UploadFileDto uploadDto);
    Task<FileAnalysisDto> GetAnalysisResultAsync(Guid analysisId);
    Task<IEnumerable<FileAnalysisDto>> GetUserAnalysesAsync(Guid userId);
    Task StartAnalysisAsync(Guid analysisId);
    Task<string> GetAnalysisStatusAsync(Guid analysisId);
    /// <summary>
    /// Deletes all analyses from history
    /// </summary>
    Task DeleteAllAnalysesAsync();
}