namespace FileAnalyzerSandbox.Domain.Entities;

/// <summary>Сущность пользователя системы</summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Имя пользователя (логин)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// Email адрес пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Хэш пароля пользователя
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// Дата и время регистрации пользователя
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Коллекция анализов, выполненных пользователем
    /// </summary>
    public virtual ICollection<FileAnalysis> FileAnalyses { get; set; } = new List<FileAnalysis>();
}