namespace FileAnalyzerSandbox.Application.DTOs;

/// <summary>
/// DTO для передачи результата анализа файла из SandboxService на клиент.
/// Содержит полную информацию об обнаруженных угрозах и отчете.
/// </summary>
/// <remarks>
/// Этот DTO используется для передачи данных между SandboxService и клиентом.
/// Создается в результате выполнения метода AnalyzeFileAsync.
/// </remarks>
public class SandboxResultDto
{
    /// <summary> Отчёт об анализе файла. </summary>
    public string Result { get; set; } = string.Empty;
    /// <summary> Уровень угрозы файла.</summary>
    public string ThreatLevel { get; set; } = string.Empty;
    /// <summary>
    /// Флаг, указывающий на безопасность файла.
    /// </summary>
    /// <value>true — файл безопасен, false — обнаружены угрозы</value>
    public bool IsSafe { get; set; }
    /// <summary>Список уникальных названий обнаруженных угроз.</summary>
    public List<string> Threats { get; set; } = new();
    /// <summary> Детальный список обнаруженных угроз.</summary>
    public List<string> ThreatDetails { get; set; } = new();
    /// <summary>SHA256 хэш проанализированного файла.</summary>
    public string FileHash { get; set; } = string.Empty;
    /// <summary>Дата и время выполнения анализа. </summary>
    public DateTime AnalysisDuration { get; set; }
}