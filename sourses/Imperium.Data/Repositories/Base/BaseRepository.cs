using Dapper;
using Imperium.Data.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Base
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly string _tableName;

        protected BaseRepository(IDbConnectionFactory connectionFactory, string tableName)
        {
            _connectionFactory = connectionFactory;
            _tableName = tableName;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"SELECT * FROM \"{_tableName}\" WHERE \"Id\" = @Id";
            return await connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"SELECT * FROM \"{_tableName}\"";
            return await connection.QueryAsync<T>(sql);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            // Простая реализация для базовых случаев
            // В реальном проекте можно использовать библиотеки типа DapperExtensions или SqlKata
            var allEntities = await GetAllAsync();
            var compiledPredicate = predicate.Compile();
            return allEntities.Where(compiledPredicate);
        }

        public virtual async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await FindAsync(predicate);
            return entities.SingleOrDefault();
        }

        public virtual async Task<Guid> AddAsync(T entity)
        {
            var (columns, values, parameters) = BuildInsertQuery(entity);
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"INSERT INTO \"{_tableName}\" ({columns}) VALUES ({values}) RETURNING \"Id\"";
            return await connection.QuerySingleAsync<Guid>(sql, parameters);
        }

        public virtual async Task<int> AddRangeAsync(IEnumerable<T> entities)
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any()) return 0;

            using var connection = await _connectionFactory.CreateConnectionAsync();

            var rowsAffected = 0;
            foreach (var entity in entitiesList)
            {
                var (columns, values, parameters) = BuildInsertQuery(entity);
                var sql = $"INSERT INTO \"{_tableName}\" ({columns}) VALUES ({values})";
                rowsAffected += await connection.ExecuteAsync(sql, parameters);
            }

            return rowsAffected;
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            var (setClause, parameters) = BuildUpdateQuery(entity);
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"UPDATE \"{_tableName}\" SET {setClause} WHERE \"Id\" = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"DELETE FROM \"{_tableName}\" WHERE \"Id\" = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<Guid> ids)
        {
            var idsList = ids.ToList();
            if (!idsList.Any()) return false;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"DELETE FROM \"{_tableName}\" WHERE \"Id\" = ANY(@Ids)";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = idsList.ToArray() });
            return rowsAffected > 0;
        }

        public virtual async Task<int> CountAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"SELECT COUNT(*) FROM \"{_tableName}\"";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await FindAsync(predicate);
            return entities.Count();
        }

        protected virtual (string columns, string values, object parameters) BuildInsertQuery(T entity)
        {
            var properties = GetWritableProperties().Where(p => p.Name != "Id").ToArray();
            var columns = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
            var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var parameters = new Dictionary<string, object?>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (value is DateTime dateTime && dateTime == default)
                {
                    value = DateTime.UtcNow;
                }
                parameters[prop.Name] = value;
            }

            return (columns, values, parameters);
        }

        protected virtual (string setClause, object parameters) BuildUpdateQuery(T entity)
        {
            var properties = GetWritableProperties().Where(p => p.Name != "Id").ToArray();
            var setClause = string.Join(", ", properties.Select(p => $"\"{p.Name}\" = @{p.Name}"));

            var parameters = new Dictionary<string, object?>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (prop.Name == "UpdatedAt" && value is DateTime && (DateTime)value == default)
                {
                    value = DateTime.UtcNow;
                }
                parameters[prop.Name] = value;
            }

            // Добавляем Id для WHERE условия
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                parameters["Id"] = idProperty.GetValue(entity);
            }

            return (setClause, parameters);
        }

        protected virtual PropertyInfo[] GetWritableProperties()
        {
            return typeof(T).GetProperties()
                .Where(p => p.CanWrite && p.CanRead)
                .Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any())
                .ToArray();
        }
    }
}