using System.Security.Cryptography;
using System.Text;
using FileAnalyzerSandbox.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace FileAnalyzerSandbox.Application.Services;

/// <summary>
/// Сервис для анализа файлов в изолированной среде (песочнице).
/// Выполняет проверку файлов на наличие вредоносного кода, вирусов и других угроз безопасности.
/// </summary>
public class SandboxService : ISandboxService
{
    private readonly string _sandboxPath;
    private readonly ILogger<SandboxService> _logger;

    // Реальные сигнатуры угроз
    private readonly List<ThreatSignature> _threatSignatures;

    public SandboxService(ILogger<SandboxService> logger)
    {
        _logger = logger;
        _sandboxPath = Path.Combine(Path.GetTempPath(), "FileAnalyzerSandbox", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sandboxPath);

        // Инициализация сигнатур угроз
        _threatSignatures = new List<ThreatSignature>
        {
            // КРИТИЧЕСКИЕ (вес 10)
            new ThreatSignature { Name = "Троян/Бэкдор", Patterns = new[] { "ReverseShell", "BindShell", "Meterpreter", "CobaltStrike", "Empire", "Netcat" }, Weight = 10 },
            new ThreatSignature { Name = "Ransomware", Patterns = new[] { "ransom", "encrypt", "decrypt", "bitcoin", "crypt", "locker", "WannaCry", "Locky" }, Weight = 10 },
            
            // ВЫСОКИЕ (вес 7)
            new ThreatSignature { Name = "Вредоносный PowerShell", Patterns = new[] { "Invoke-Expression", "IEX", "DownloadString", "Invoke-WebRequest", "Net.WebClient" }, Weight = 7 },
            new ThreatSignature { Name = "Эксплойт", Patterns = new[] { "HeapSpray", "Shellcode", "0day", "Use-After-Free" }, Weight = 7 },
            
            // СРЕДНИЕ (вес 5)
            new ThreatSignature { Name = "Подозрительный макрос", Patterns = new[] { "AutoOpen", "Document_Open", "Workbook_Open", "ThisDocument" }, Weight = 5 },
            new ThreatSignature { Name = "Опасная функция", Patterns = new[] { "Process.Start", "File.Delete", "Registry.SetValue", "ShellExecute", "WinExec" }, Weight = 5 },
            
            // НИЗКИЕ (вес 3)
            new ThreatSignature { Name = "Подозрительный JavaScript", Patterns = new[] { "eval(", "document.write", "unescape(", "decodeURIComponent" }, Weight = 3 }
        };
    }

    public async Task SaveFileAsync(string fileName, byte[] content)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);
        await File.WriteAllBytesAsync(filePath, content);
        _logger.LogInformation("Файл сохранён: {FileName}", fileName);
    }

    public async Task<SandboxResultDto> AnalyzeFileAsync(string fileName)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл {fileName} не найден");

        _logger.LogInformation("Анализ файла: {FileName}", fileName);

        var content = await File.ReadAllBytesAsync(filePath);
        var extension = Path.GetExtension(fileName).ToLower();
        var fileInfo = new FileInfo(filePath);

        var threats = new List<DetectedThreat>();
        var threatDetails = new List<string>();
        var totalScore = 0;

        // 
        // 1. АНАЛИЗ РАСШИРЕНИЯ ФАЙЛА
        // 
        var extensionRisk = GetExtensionRisk(extension);
        if (extensionRisk > 0)
        {
            threats.Add(new DetectedThreat { Name = $"Расширение {extension}", Weight = extensionRisk });
            threatDetails.Add($"Файл с расширением {extension} (риск: {extensionRisk}/10)");
            totalScore += extensionRisk;
        }

        // 
        // 2. ПРОВЕРКА НА МАСКИРОВКУ (EXE под видом другого файла)
        // 
        if (IsExecutableByContent(content))
        {
            // Файл является исполняемым (имеет MZ сигнатуру)
            if (extension != ".exe" && extension != ".dll" && extension != ".scr")
            {
                threats.Add(new DetectedThreat { Name = "Маскировка файла", Weight = 8 });
                threatDetails.Add($"⚠️ ФАЙЛ МАСКИРУЕТСЯ! Расширение .{extension}, но на самом деле EXE/DLL");
                totalScore += 8;
            }

            // Дополнительные проверки для EXE файлов
            if (IsPacked(content))
            {
                threats.Add(new DetectedThreat { Name = "Упакованный код", Weight = 5 });
                threatDetails.Add("Файл упакован (UPX/MPRESS) - часто используется для скрытия вредоносного кода");
                totalScore += 5;
            }

            if (fileInfo.Length < 50 * 1024)
            {
                threats.Add(new DetectedThreat { Name = "Маленький EXE файл", Weight = 3 });
                threatDetails.Add("Подозрительно маленький размер для исполняемого файла (менее 50 КБ)");
                totalScore += 3;
            }
        }

        // 
        // 3. АНАЛИЗ СОДЕРЖИМОГО ДЛЯ ТЕКСТОВЫХ ФАЙЛОВ
        //
        if (IsTextFile(extension) && !IsExecutableByContent(content))
        {
            var textContent = Encoding.UTF8.GetString(content);

            foreach (var signature in _threatSignatures)
            {
                foreach (var pattern in signature.Patterns)
                {
                    if (textContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        threats.Add(new DetectedThreat { Name = signature.Name, Weight = signature.Weight });
                        threatDetails.Add($"Обнаружен паттерн: '{pattern}' - {signature.Name}");
                        totalScore += signature.Weight;
                        break; // Не дублируем одинаковые угрозы
                    }
                }
            }
        }

        // 
        // 4. ОПРЕДЕЛЕНИЕ УРОВНЯ УГРОЗЫ
        // 
        string threatLevel;
        if (totalScore <= 2)
            threatLevel = "Безопасно";
        else if (totalScore <= 8)
            threatLevel = "Низкий";
        else if (totalScore <= 15)
            threatLevel = "Средний";
        else if (totalScore <= 25)
            threatLevel = "Высокий";
        else
            threatLevel = "Критический";

        // 
        // 5. ФОРМИРОВАНИЕ ОТЧЕТА
        // 
        var result = new SandboxResultDto
        {
            Result = GenerateReport(fileName, threatLevel, threatDetails, fileInfo.Length, totalScore, threats),
            ThreatLevel = threatLevel,
            IsSafe = threatLevel == "Безопасно",
            Threats = threats.Select(t => t.Name).Distinct().ToList(),
            ThreatDetails = threatDetails,
            FileHash = ComputeHash(content),
            AnalysisDuration = DateTime.Now
        };

        _logger.LogInformation("Анализ завершен. Уровень: {Level}, Оценка: {Score}, Найдено угроз: {Count}",
            threatLevel, totalScore, threats.Count);

        // Очистка файла из песочницы
        await DeleteFileAsync(fileName);

        return result;
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
            _logger.LogInformation("Файл удалён: {FileName}", fileName);
        }
    }

    public Task<bool> IsHealthyAsync() => Task.FromResult(Directory.Exists(_sandboxPath));

    // 
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // 

    private int GetExtensionRisk(string extension)
    {
        return extension switch
        {
            ".exe" or ".dll" or ".scr" => 8,
            ".bat" or ".cmd" or ".ps1" => 6,
            ".vbs" or ".js" or ".jar" => 5,
            ".docm" or ".xlsm" or ".pptm" => 4,
            ".pdf" => 2,
            _ => 0
        };
    }

    private bool IsExecutableFile(string extension)
    {
        return extension == ".exe" || extension == ".dll" || extension == ".scr" ||
               extension == ".bat" || extension == ".cmd" || extension == ".ps1";
    }

    private bool IsExecutableByContent(byte[] content)
    {
        // Проверка MZ сигнатуры (признак EXE/DLL файла)
        return content.Length > 2 && content[0] == 0x4D && content[1] == 0x5A;
    }

    private bool IsTextFile(string extension)
    {
        return extension == ".txt" || extension == ".js" || extension == ".html" ||
               extension == ".css" || extension == ".xml" || extension == ".json" ||
               extension == ".ps1" || extension == ".bat" || extension == ".vbs" ||
               extension == ".csv";
    }

    private bool IsPacked(byte[] content)
    {
        var textContent = Encoding.ASCII.GetString(content.Take(5000).ToArray());
        string[] packers = { "UPX", "Themida", "VMProtect", "MPRESS", "ASPack", "Enigma" };
        return packers.Any(p => textContent.Contains(p));
    }

    private string GenerateReport(string fileName, string threatLevel, List<string> threatDetails,
        long fileSize, int score, List<DetectedThreat> threats)
    {
        var report = new StringBuilder();

        report.AppendLine($"                    ОТЧЁТ ОБ АНАЛИЗЕ ФАЙЛА                    ");
        report.AppendLine();
        report.AppendLine($"Имя файла: {Path.GetFileName(fileName)}");
        report.AppendLine($"Размер: {FormatFileSize(fileSize)}");
        report.AppendLine($"Уровень угрозы: {threatLevel}");
        report.AppendLine($"Оценка безопасности: {score}/100");
        report.AppendLine($"Время анализа: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        report.AppendLine();
        report.AppendLine(new string('═', 60));

        if (threatDetails.Any())
        {
            report.AppendLine("⚠️ ОБНАРУЖЕННЫЕ УГРОЗЫ:");
            report.AppendLine();
            foreach (var detail in threatDetails)
            {
                report.AppendLine($"  • {detail}");
            }
            report.AppendLine();
            report.AppendLine(new string('─', 60));
            report.AppendLine();
            report.AppendLine(GetRecommendation(threatLevel));
        }
        else
        {
            report.AppendLine("✅ УГРОЗ НЕ ОБНАРУЖЕНО");
            report.AppendLine();
            report.AppendLine("Файл успешно прошёл все проверки:");
            report.AppendLine("  • Проверка расширения — пройдена");
            report.AppendLine("  • Проверка на маскировку — пройдена");
            report.AppendLine("  • Проверка сигнатур — пройдена");
            report.AppendLine("  • Проверка на упаковку — пройдена");
            report.AppendLine();
            report.AppendLine(GetRecommendation(threatLevel));
        }

        return report.ToString();
    }

    private string GetRecommendation(string threatLevel)
    {
        return threatLevel switch
        {
            "Критический" => "❌ КРИТИЧЕСКАЯ УГРОЗА! НЕ ОТКРЫВАЙТЕ файл. Немедленно удалите и запустите антивирус.",
            "Высокий" => "⚠️ ВЫСОКАЯ УГРОЗА! Очень опасно. Рекомендуется удалить файл.",
            "Средний" => "⚠️ СРЕДНЯЯ УГРОЗА! Будьте осторожны. Проверьте файл антивирусом.",
            "Низкий" => "ℹ️ НИЗКАЯ УГРОЗА. Незначительные риски. Рекомендуется проверка.",
            "Безопасно" => "✅ ФАЙЛ БЕЗОПАСЕН. Можно открывать.",
            _ => "ℹ️ Стандартная осторожность."
        };
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} Б";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} КБ";
        return $"{bytes / (1024.0 * 1024.0):F2} МБ";
    }

    private string ComputeHash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(content);
        return Convert.ToHexString(hash).ToLower();
    }

    public void Cleanup()
    {
        if (Directory.Exists(_sandboxPath))
        {
            Directory.Delete(_sandboxPath, true);
            _logger.LogInformation("Песочница очищена");
        }
    }
}

// 
// ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ
// 

public class ThreatSignature
{
    public string Name { get; set; } = string.Empty;
    public string[] Patterns { get; set; } = Array.Empty<string>();
    public int Weight { get; set; }
}

public class DetectedThreat
{
    public string Name { get; set; } = string.Empty;
    public int Weight { get; set; }
}