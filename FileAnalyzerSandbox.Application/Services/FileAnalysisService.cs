using System.Security.Cryptography;
using System.Text;
using FileAnalyzerSandbox.Application.DTOs;
using FileAnalyzerSandbox.Domain.Entities;
using FileAnalyzerSandbox.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileAnalyzerSandbox.Application.Services;

/// <summary>
/// Сервис управления анализом файлов.
/// Координирует процесс загрузки, анализа и сохранения результатов.
/// </summary>
/// <remarks>
/// Основные функции:
/// <list type="number">
/// <item><description>Загрузка файлов и сохранение в БД</description></item>
/// <item><description>Запуск анализа через SandboxService</description></item>
/// <item><description>Получение результатов и истории анализов</description></item>
/// </list>
/// </remarks>

public class FileAnalysisService : IFileAnalysisService
{
    private readonly IFileAnalysisRepository _repository;
    private readonly IRepository<User> _userRepository;
    private readonly ISandboxService _sandboxService;
    private readonly ILogger<FileAnalysisService> _logger;

    /// <summary> Инициализирует новый экземпляр сервиса анализа файлов.</summary>
    /// <param name="repository">Репозиторий для работы с анализами</param>
    /// <param name="userRepository">Репозиторий для работы с пользователями</param>
    /// <param name="sandboxService">Сервис для анализа файлов в песочнице</param>
    /// <param name="logger">Логгер для записи событий</param>
    public FileAnalysisService(
        IFileAnalysisRepository repository,
        IRepository<User> userRepository,
        ISandboxService sandboxService,
        ILogger<FileAnalysisService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _sandboxService = sandboxService;
        _logger = logger;
    }

    /// <summary>
    /// Загружает файл и сохраняет информацию о нем в базу данных.
    /// </summary>
    /// <param name="uploadDto">DTO с данными загружаемого файла</param>
    /// <returns>DTO с информацией о загруженном файле</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если пользователь не найден</exception>
    public async Task<FileAnalysisDto> UploadFileAsync(UploadFileDto uploadDto)
    {
        var user = await _userRepository.GetByIdAsync(uploadDto.UserId);
        if (user == null)
            throw new ArgumentException("User not found");

        var fileHash = ComputeHash(uploadDto.Content);

        var analysis = new FileAnalysis
        {
            Id = Guid.NewGuid(),
            FileName = Guid.NewGuid().ToString(),
            OriginalFileName = uploadDto.FileName,
            FileSize = uploadDto.Content.Length,
            FileType = Path.GetExtension(uploadDto.FileName),
            FileHash = fileHash,
            Status = AnalysisStatus.Pending,
            UploadedAt = DateTime.UtcNow,
            UserId = uploadDto.UserId
        };

        // Save file to sandbox
        await _sandboxService.SaveFileAsync(analysis.FileName, uploadDto.Content);

        await _repository.AddAsync(analysis);
        _logger.LogInformation("File uploaded: {FileName} by user {UserId}", uploadDto.FileName, uploadDto.UserId);

        return MapToDto(analysis);
    }


    /// <summary>
    /// Запускает анализ файла в песочнице.
    /// </summary>
    /// <param name="analysisId">Идентификатор анализа в базе данных</param>
    /// <exception cref="ArgumentException">Выбрасывается, если анализ не найден</exception>
    public async Task StartAnalysisAsync(Guid analysisId)
    {
        var analysis = await _repository.GetByIdAsync(analysisId);
        if (analysis == null)
            throw new ArgumentException("Analysis not found");

        analysis.Status = AnalysisStatus.Processing;
        await _repository.UpdateAsync(analysis);

        try
        {
            var result = await _sandboxService.AnalyzeFileAsync(analysis.FileName);

            analysis.Status = AnalysisStatus.Completed;
            analysis.AnalysisResult = result.GetType().GetProperty("Result")?.GetValue(result)?.ToString();
            analysis.ThreatLevel = result.GetType().GetProperty("ThreatLevel")?.GetValue(result)?.ToString();
            analysis.AnalyzedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(analysis);
            _logger.LogInformation("Analysis completed for {AnalysisId}", analysisId);
        }
        catch (Exception ex)
        {
            analysis.Status = AnalysisStatus.Failed;
            await _repository.UpdateAsync(analysis);
            _logger.LogError(ex, "Analysis failed for {AnalysisId}", analysisId);
            throw;
        }
    }

    /// <summary>
    /// Deletes all analyses from the database
    /// </summary>
    public async Task DeleteAllAnalysesAsync()
    {
        var allAnalyses = await _repository.GetAllAsync();
        foreach (var analysis in allAnalyses)
        {
            await _repository.DeleteAsync(analysis);
        }
        _logger.LogInformation("All analyses cleared from history");
    }

    /// <summary>
    /// Получает результат анализа по идентификатору.
    /// </summary>
    /// <param name="analysisId">Идентификатор анализа</param>
    /// <returns>DTO с полными данными анализа</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если анализ не найден</exception>
    public async Task<FileAnalysisDto> GetAnalysisResultAsync(Guid analysisId)
    {
        var analysis = await _repository.GetWithLogsAsync(analysisId);
        if (analysis == null)
            throw new ArgumentException("Analysis not found");

        return MapToDto(analysis);
    }

    public async Task<IEnumerable<FileAnalysisDto>> GetUserAnalysesAsync(Guid userId)
    {
        var analyses = await _repository.GetByUserAsync(userId);
        return analyses.Select(MapToDto);
    }

    /// <summary>
    /// Получает список всех анализов пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Коллекция DTO с информацией об анализах</returns>
    public async Task<string> GetAnalysisStatusAsync(Guid analysisId)
    {
        var analysis = await _repository.GetByIdAsync(analysisId);
        return analysis?.Status.ToString() ?? "Not found";
    }

    private string ComputeHash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(content);
        return Convert.ToHexString(hash).ToLower();
    }

    private FileAnalysisDto MapToDto(FileAnalysis analysis)
    {
        return new FileAnalysisDto
        {
            Id = analysis.Id,
            FileName = analysis.FileName,
            OriginalFileName = analysis.OriginalFileName,
            FileSize = analysis.FileSize,
            FileType = analysis.FileType,
            FileHash = analysis.FileHash,
            Status = analysis.Status.ToString(),
            AnalysisResult = analysis.AnalysisResult,
            ThreatLevel = analysis.ThreatLevel,
            UploadedAt = analysis.UploadedAt,
            AnalyzedAt = analysis.AnalyzedAt,
            Logs = analysis.AnalysisLogs?.Select(l => new AnalysisLogDto
            {
                LogLevel = l.LogLevel,
                Message = l.Message,
                Details = l.Details,
                Timestamp = l.Timestamp
            }).ToList() ?? new()
        };
    }
}