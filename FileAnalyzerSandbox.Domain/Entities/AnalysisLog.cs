namespace FileAnalyzerSandbox.Domain.Entities;

/// <summary>Лог событий в процессе анализа файла</summary>
public class AnalysisLog
{
    /// <summary>Уникальный идентификатор записи лога</summary>
    public Guid Id { get; set; }
    /// <summary>Идентификатор анализа, к которому относится лог</summary>
    public Guid FileAnalysisId { get; set; }
    /// <summary>Уровень логирования (Information, Warning, Error)</summary>
    public string LogLevel { get; set; } = string.Empty;
    /// <summary>Текст сообщения лога</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Дополнительные детали</summary>
    public string? Details { get; set; }
    /// <summary>Время создания записи лога</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>Навигационное свойство: анализ, к которому относится лог</summary>
    public virtual FileAnalysis? FileAnalysis { get; set; }
}