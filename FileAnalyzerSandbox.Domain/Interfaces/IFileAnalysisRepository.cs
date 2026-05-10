using FileAnalyzerSandbox.Domain.Entities;

namespace FileAnalyzerSandbox.Domain.Interfaces;

/// <summary>Репозиторий для работы с анализами файлов</summary>
public interface IFileAnalysisRepository : IRepository<FileAnalysis>
{
    /// <summary>Получает все анализы пользователя</summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Коллекция анализов пользователя</returns>
    Task<IEnumerable<FileAnalysis>> GetByUserAsync(Guid userId);
    /// <summary>Получает анализы по статусу</summary>
    /// <param name="status">Статус анализа</param>
    /// <returns>Коллекция анализов с указанным статусом</returns>
    Task<IEnumerable<FileAnalysis>> GetByStatusAsync(AnalysisStatus status);
    /// <summary> Получает анализ вместе с логами</summary>
    /// <param name="id">Идентификатор анализа</param>
    /// <returns>Анализ с загруженными логами или null</returns>
    Task<FileAnalysis?> GetWithLogsAsync(Guid id);
}