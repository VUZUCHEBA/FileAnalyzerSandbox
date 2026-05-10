namespace FileAnalyzerSandbox.Application.DTOs;

/// <summary>DTO для передачи данных анализа файла на клиент</summary>
public class FileAnalysisDto
{
    /// <summary>Идентификатор анализа</summary>
    public Guid Id { get; set; }
    /// <summary>Системное имя файла</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>Оригинальное имя файла</summary>
    public string OriginalFileName { get; set; } = string.Empty;
    /// <summary>Размер файла</summary>
    public long FileSize { get; set; }
    /// <summary>Тип файла (расширение)</summary>
    public string FileType { get; set; } = string.Empty;
    /// <summary>SHA256 хэш файла</summary>
    public string FileHash { get; set; } = string.Empty;
    /// <summary>Статус анализа</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Результат анализа</summary>
    public string? AnalysisResult { get; set; }
    /// <summary>Уровень угрозы</summary>
    public string? ThreatLevel { get; set; }
    /// <summary>Дата загрузки</summary>
    public DateTime UploadedAt { get; set; }
    /// <summary>Дата анализа</summary>
    public DateTime? AnalyzedAt { get; set; }
    /// <summary>Логи анализа</summary>
    public List<AnalysisLogDto> Logs { get; set; } = new();
}

/// <summary>DTO для передачи лога анализа</summary>
public class AnalysisLogDto
{
    /// <summary>Уровень логирования</summary>
    public string LogLevel { get; set; } = string.Empty;
    /// <summary>Сообщение</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Детали</summary>
    public string? Details { get; set; }
    /// <summary>Время создания</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>DTO для загрузки файла</summary>
public class UploadFileDto
{
    /// <summary>Имя файла</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>Содержимое файла</summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();
    /// <summary>Идентификатор пользователя</summary>
    public Guid UserId { get; set; }
}