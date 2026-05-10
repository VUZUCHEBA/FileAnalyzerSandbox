namespace FileAnalyzerSandbox.Domain.Entities;

/// <summary>Сущность, представляющая результат анализа файла в базе данных.</summary>
public class FileAnalysis
{
    /// <summary>Уникальный идентификатор анализа</summary>
    public Guid Id { get; set; }
    /// <summary>Уникальное имя файла в системе (сгенерированное GUID)</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>Оригинальное имя файла, указанное пользователем</summary>
    public string OriginalFileName { get; set; } = string.Empty;
    /// <summary>Размер файла</summary>
    public long FileSize { get; set; }
    /// <summary>Тип файла (расширение)</summary>
    public string FileType { get; set; } = string.Empty;
    /// <summary>SHA256 хэш файла для идентификации и проверки уникальности</summary>
    public string FileHash { get; set; } = string.Empty;
    /// <summary>Текущий статус анализа файла </summary>
    public AnalysisStatus Status { get; set; }
    /// <summary>Полный текстовый отчёт об анализе</summary>
    public string? AnalysisResult { get; set; }
    /// <summary>Уровень угрозы: "Безопасно", "Низкий", "Средний", "Высокий", "Критический"</summary>
    public string? ThreatLevel { get; set; }
    /// <summary>Дата и время загрузки файла</summary>
    public DateTime UploadedAt { get; set; }
    /// <summary>Дата и время завершения анализа (null, если еще не завершен)</summary>
    public DateTime? AnalyzedAt { get; set; }
    /// <summary>Идентификатор пользователя, загрузившего файл</summary>
    public Guid UserId { get; set; }
    /// <summary>Навигационное свойство: пользователь, загрузивший файл</summary>
    public virtual User? User { get; set; }
    /// <summary>Коллекция логов анализа файла</summary>
    public virtual ICollection<AnalysisLog> AnalysisLogs { get; set; } = new List<AnalysisLog>();
}
/// <summary>Статус анализа файла</summary>
public enum AnalysisStatus
{
    /// <summary>Файл загружен, ожидает анализа</summary>
    Pending = 0,
    /// <summary>Файл находится в процессе анализа</summary>
    Processing = 1,
    /// <summary>Анализ успешно завершен</summary>
    Completed = 2,
    /// <summary>Анализ завершился с ошибкой</summary>
    Failed = 3
}