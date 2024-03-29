﻿using System.Linq.Expressions;
using System.Reflection;

namespace CardboardBox.Database.Generation
{
	public interface IQueryGeneratorService
	{
		string Insert<T>(string tableName, Action<PropertyExpressionBuilder<T>>? exclusions = null);
		string Insert(string tableName, Type type, params string[] exclusions);

		string InsertReturn<T, TReturn>(string tableName, Expression<Func<T, TReturn>> returnCol, Action<PropertyExpressionBuilder<T>>? exclusions = null);
		string InsertReturn(string tableName, Type type, string returnCol, params string[] exclusions);

		string Update<T>(string tableName, Action<PropertyExpressionBuilder<T>>? exclusions = null) where T : DbObject;
		string Update<T, TId>(string tableName, Expression<Func<T, TId>> id, Action<PropertyExpressionBuilder<T>>? exclusions = null);
		string Update(string tableName, Type type, PropertyMap id, params string[] exclusions);

		string Upsert<T>(string tableName,
			Action<PropertyExpressionBuilder<T>> conflicts,
			Action<PropertyExpressionBuilder<T>>? insertExclusions = null,
			Action<PropertyExpressionBuilder<T>>? updateExclusions = null,
			string? returning = null);
		string Upsert<T, TReturn>(string tableName,
			Action<PropertyExpressionBuilder<T>> conflicts,
			Action<PropertyExpressionBuilder<T>>? insertExclusions = null,
			Action<PropertyExpressionBuilder<T>>? updateExclusions = null,
			Expression<Func<T, TReturn>>? returnCol = null);
		string Upsert(string tableName, Type type, string[] conflicts, string[] insertExclusions, string[] updateExclusions, string? returning = null);

		string SelectAll(string tableName);
		string SelectAllNonDeleted(string tableName);
		string SelectId<T>(string tableName);
		string Select<T>(string tableName, Action<PropertyExpressionBuilder<T>> columns);
		string Select(string tableName, params PropertyMap[] columns);
	}

	public class QueryGeneratorService : IQueryGeneratorService
	{
		private readonly INameConvention _convention;

		public QueryGeneratorService(INameConvention convention)
		{
			_convention = convention;
		}

		#region Helper Methods
		public Dictionary<string, string> PropertyMask => new()
		{
			[AuditableColumnName(t => t.UpdatedAt)] = "CURRENT_TIMESTAMP",
			[AuditableColumnName(t => t.CreatedAt)] = "CURRENT_TIMESTAMP"
		};

		public string AuditableColumnName<T>(Expression<Func<DbAuditable, T>> property)
		{
			return ColumnName(property);
		}

		public string ColumnName<T, TProp>(Expression<Func<T, TProp>> property)
		{
			var prop = property.GetPropertyInfo();
			if (prop == null) throw new ArgumentNullException(nameof(property));
			return _convention.ToCase(prop.Name) ?? string.Empty;
		}

		public PropertyName GetPropertyName(PropertyInfo prop) => new(prop.Name, _convention.ToCase(prop.Name) ?? string.Empty);

		public PropertyName[] GetPropertyNames<T>(Action<PropertyExpressionBuilder<T>> action)
		{
			var bob = new PropertyExpressionBuilder<T>();
			action?.Invoke(bob);
			return bob.Properties.Select(GetPropertyName).ToArray();
		}

		public string[] GetPropertySnakes<T>(Action<PropertyExpressionBuilder<T>>? action)
		{
			var bob = new PropertyExpressionBuilder<T>();
			action?.Invoke(bob);
			return bob.Properties.Select(t => GetPropertyName(t).Snake).ToArray();
		}

		public PropertyName[] GetPropertyNames(Type type)
		{
			return type.GetProperties()
				.Select(GetPropertyName)
				.ToArray();
		}

		public PropertyMap[] GetPropertyMap(IEnumerable<PropertyName> names, string[] exclusions, bool applyTransforms = true, bool pascalCase = false)
		{
			return names
				.Where(t => !exclusions.Any(a => a == t.Snake))
				.Select(t =>
				{
					if (applyTransforms && PropertyMask.ContainsKey(t.Snake))
						return new PropertyMap(t.Snake, PropertyMask[t.Snake]);

					var orig = pascalCase ? t.Original.ToPascalCase() : t.Original;

					return new PropertyMap(t.Snake, ":" + orig);
				})
				.ToArray();
		}

		public bool AssignableFrom<T>(Type type)
		{
			return typeof(T).IsAssignableFrom(type);
		}
		#endregion

		#region Inserts
		public string[] DeteremineInsertExclusions(Type type)
		{
			if (AssignableFrom<DbObject>(type))
				return new[] 
				{
					AuditableColumnName(t => t.Id),
					AuditableColumnName(t => t.DeletedAt)
				};

			return Array.Empty<string>();
		}

		public string Insert<T>(string tableName, Action<PropertyExpressionBuilder<T>>? exclusions = null)
		{
			var exlus = GetPropertySnakes(exclusions);
			return Insert(tableName, typeof(T), exlus);
		}

		public string Insert(string tableName, Type type, params string[] exclusions)
		{
			if (exclusions == null || exclusions.Length == 0)
				exclusions = DeteremineInsertExclusions(type);

			var props = GetPropertyMap(GetPropertyNames(type), exclusions);

			var columns = props.Select(t => t.Snake).ToArray();
			var values = props.Select(t => t.Value).ToArray();

			return $@"INSERT INTO {tableName} (
	{string.Join(",\r\n\t", columns)}
) VALUES (
	{string.Join(",\r\n\t", values)}
)";
		}

		public string InsertReturn<T, TReturn>(string tableName, Expression<Func<T, TReturn>> returnCol, Action<PropertyExpressionBuilder<T>>? exclusions = null)
		{
			var rc = GetPropertyName(Extensions.GetPropertyInfo(returnCol)).Snake;
			var exlus = GetPropertySnakes(exclusions);
			return InsertReturn(tableName, typeof(T), rc, exlus);
		}

		public string InsertReturn(string tableName, Type type, string returnCol, params string[] exclusions)
		{
			return Insert(tableName, type, exclusions) + " RETURNING " + returnCol;
		}
		#endregion

		#region Updates
		public string[] DeteremineUpdateExclusions(Type type)
		{
			if (AssignableFrom<DbObject>(type))
				return new[]
				{
					AuditableColumnName(t => t.Id),
					AuditableColumnName(t => t.CreatedAt)
				};

			return Array.Empty<string>();
		}

		public string Update<T>(string tableName, Action<PropertyExpressionBuilder<T>>? exclusions = null) where T : DbObject
		{
			return Update(tableName, t => t.Id, exclusions);
		}

		public string Update<T, TId>(string tableName, Expression<Func<T, TId>> id, Action<PropertyExpressionBuilder<T>>? exclusions = null)
		{
			var idProp = Extensions.GetPropertyInfo(id);
			var idCol = GetPropertyMap(new[] { GetPropertyName(idProp) }, Array.Empty<string>()).First();

			var exlus = GetPropertySnakes(exclusions);
			return Update(tableName, typeof(T), idCol, exlus);
		}

		public string Update(string tableName, Type type, PropertyMap id, params string[] exclusions)
		{
			if (exclusions == null || exclusions.Length == 0)
				exclusions = DeteremineUpdateExclusions(type);

			var props = GetPropertyMap(GetPropertyNames(type), exclusions);

			var columns = props.Select(t => $"{t.Snake} = {t.Value}");

			return $@"UPDATE {tableName}
SET
	{string.Join(",\r\n", columns)}
WHERE
	{id.Snake} = {id.Value}";
		}
		#endregion

		#region Upserts
		public string Upsert<T, TReturn>(string tableName,
			Action<PropertyExpressionBuilder<T>> conflicts,
			Action<PropertyExpressionBuilder<T>>? insertExclusions = null,
			Action<PropertyExpressionBuilder<T>>? updateExclusions = null,
			Expression<Func<T, TReturn>>? returnCol = null)
		{
			var rc = GetPropertyName(Extensions.GetPropertyInfo(returnCol)).Snake;
			return Upsert(tableName, conflicts, insertExclusions, updateExclusions, rc);
		}

		public string Upsert<T>(string tableName,
			Action<PropertyExpressionBuilder<T>> conflicts,
			Action<PropertyExpressionBuilder<T>>? insertExclusions = null,
			Action<PropertyExpressionBuilder<T>>? updateExclusions = null,
			string? returning = null)
		{
			var ins = GetPropertySnakes(insertExclusions);
			var ups = GetPropertySnakes(updateExclusions);
			var confs = GetPropertySnakes(conflicts);
			return Upsert(tableName, typeof(T), confs, ins, ups, returning);
		}

		public string Upsert(string tableName, Type type, string[] conflicts, string[] insertExclusions, string[] updateExclusions, string? returning = null)
		{
			var insert = Insert(tableName, type, insertExclusions);

			var props = GetPropertyMap(GetPropertyNames(type), updateExclusions);

			var columns = props.Select(t => $"{t.Snake} = {t.Value}");

			var ret = !string.IsNullOrEmpty(returning) ? "\r\nRETURNING " + returning : "";

			return $@"{insert} ON CONFLICT ({string.Join(", ", conflicts)}) DO UPDATE SET
	{string.Join(",\r\n\t", columns)}{ret}";
		}
		#endregion

		#region Selects
		public string SelectAllNonDeleted(string tableName)
		{
			var deletedAt = AuditableColumnName(t => t.DeletedAt);
			return $"SELECT * FROM {tableName} WHERE {deletedAt} IS NULL";

		}

		public string SelectAll(string tableName)
		{
			return $"SELECT * FROM {tableName}";
		}

		public string SelectId<T>(string tableName)
		{
			var type = typeof(T);
			if (AssignableFrom<DbObject>(type))
				return Select(tableName, new PropertyMap[]
				{
					new (AuditableColumnName(t => t.Id), ":Id"),
					new (AuditableColumnName(t => t.DeletedAt), "IS NULL", false)
				});

			throw new ArgumentException($"{type.Name} is not supported.");
		}

		public string Select<T>(string tableName, Action<PropertyExpressionBuilder<T>> columns)
		{
			var propNames = GetPropertyNames(columns);
			var map = GetPropertyMap(propNames, Array.Empty<string>(), false, true);
			return Select(tableName, map);
		}

		public string Select(string tableName, params PropertyMap[] columns)
		{
			var cols = columns.Select(t => t.applyEquals ? $"{t.Snake} = {t.Value}" : $"{t.Snake} {t.Value}");
			return $@"SELECT
	*
FROM {tableName}
WHERE
	{string.Join(" AND\r\n\t", cols)}";
		}
		#endregion
	}

	public record class PropertyName(string Original, string Snake);
	public record class PropertyMap(string Snake, string Value, bool applyEquals = true);
}
