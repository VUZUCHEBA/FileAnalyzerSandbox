namespace FileAnalyzerSandbox.Web.Models;

/// <summary>DTO для передачи данных анализа файла на клиент</summary>
public class FileAnalysisDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AnalysisResult { get; set; }
    public string? ThreatLevel { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public List<AnalysisLogDto> Logs { get; set; } = new();
}

public class AnalysisLogDto
{
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}