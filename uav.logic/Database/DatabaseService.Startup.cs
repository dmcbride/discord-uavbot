using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

internal interface IDapperMappedType {}

partial class DatabaseService
{
    private static Regex _dapperUnderscorePositions = new Regex(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])");

    private static CustomPropertyTypeMap columnMapper<T>() where T : IDapperMappedType => columnMapper(typeof(T));
    private static CustomPropertyTypeMap columnMapper(Type type)
    {
        return new CustomPropertyTypeMap(type, (type, columnName) =>
        {
            var underscoredName = _dapperUnderscorePositions.Replace(columnName.ToLower(), "_");
            var lcName = columnName.ToLower();
            return type.GetProperties().Single(prop => {
                var attrName = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? prop.Name.ToLower();
                return attrName == underscoredName || attrName == lcName;
            });
        });
    }

    static DatabaseService()
    {
        SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new DateTimeOffsetHandler());

        var mappedTypes = typeof(DatabaseService).Assembly.GetTypes()
            .Where(t => t.GetInterface(nameof(IDapperMappedType)) != null);
        foreach (var type in mappedTypes)
        {
            var map = new CustomPropertyTypeMap(type, (type, columnName) =>
            {
                columnName = columnName.ToLower();
                var columnNameNoUnderscore = columnName.Replace("_", "");
                var result = type.GetProperties().Single(p => {
                    var attrName = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name.ToLower();
                    return attrName == columnName || attrName == columnNameNoUnderscore;
                });
                return result;
            });
            SqlMapper.SetTypeMap(type, map);
        }

        var jsonTypes = typeof(DatabaseService).Assembly.GetTypes()
            .Where(t => t.GetInterface(nameof(IDapperJsonType)) != null);
        foreach (var type in jsonTypes)
        {
            SqlMapper.AddTypeHandler(type, new JsonTypeHandler());
        }
    }

    public interface IDapperJsonType
    {}

    public class JsonTypeHandler : SqlMapper.ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            return JsonSerializer.Deserialize(value as string, destinationType);
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = JsonSerializer.Serialize(value);
        }
    }

    public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            return DateTimeOffset.Parse(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.DbType = DbType.DateTimeOffset;
            parameter.Value = value == DateTimeOffset.MinValue ? null : value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}