using FileAnalyzerSandbox.Application.DTOs;

namespace FileAnalyzerSandbox.Application.Services;

public interface ISandboxService
{
    Task SaveFileAsync(string fileName, byte[] content);
    Task<SandboxResultDto> AnalyzeFileAsync(string fileName);
    Task DeleteFileAsync(string fileName);
    Task<bool> IsHealthyAsync();
}