using System.Linq.Expressions;

namespace FileAnalyzerSandbox.Domain.Interfaces;

/// <summary> Базовый интерфейс репозитория для работы с сущностями</summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Получает сущность по идентификатору</summary>
    /// <param name="id">Идентификатор сущности</param>
    /// <returns>Сущность или null, если не найдена</returns>
    Task<T?> GetByIdAsync(Guid id);
    /// <summary>Получает все сущности</summary>
    /// <returns>Коллекция всех сущностей</returns>
    Task<IEnumerable<T>> GetAllAsync();
    /// <summary>Находит сущности по условию</summary>
    /// <param name="predicate">Условие фильтрации</param>
    /// <returns>Коллекция сущностей, удовлетворяющих условию</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    /// <summary>Добавляет новую сущность</summary>
    /// <param name="entity">Сущность для добавления</param>
    Task AddAsync(T entity);
    /// <summary>Обновляет существующую сущность</summary>
    /// <param name="entity">Сущность с обновленными данными</param>
    Task UpdateAsync(T entity);
    /// <summary> Удаляет сущность </summary>
    /// <param name="entity">Сущность для удаления</param>
    Task DeleteAsync(T entity);
    /// <summary>Проверяет существование сущности по условию</summary>
    /// <param name="predicate">Условие проверки</param>
    /// <returns>true, если сущность существует</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}