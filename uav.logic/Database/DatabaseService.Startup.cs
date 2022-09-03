using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

internal interface IDapperMappedType {}

partial class DatabaseService
{
    static DatabaseService()
    {
        var map = new CustomPropertyTypeMap(typeof(Hint), (type, columnName) =>
        {
            columnName = columnName.ToLower();
            var result = type.GetProperties().Single(p =>
                            (p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name.ToLower()) == columnName
                        );
            return result;
        });

        var mappedTypes = typeof(DatabaseService).Assembly.GetTypes()
            .Where(t => t.GetInterface(nameof(IDapperMappedType)) != null);
        foreach (var type in mappedTypes)
        {
            SqlMapper.SetTypeMap(type, map);
        }
    }
}