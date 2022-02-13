using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task<PlanetInfo> PlanetByNumber(int id)
    {
        using var connection = Connect;
        var results = await connection.QueryAsync<PlanetInfo>(@"
        SELECT * FROM planet_info
        WHERE ID = @id
        ", new {id});

        return results.FirstOrDefault();
    }

    public async Task<PlanetInfo> PlanetByName(string name)
    {
         using var connection = Connect;
        var results = await connection.QueryAsync<PlanetInfo>(@"
        SELECT * FROM planet_info
        WHERE LOWER(name) = LOWER(@name)
        ", new {name});

        return results.SingleOrDefault();
    }

    public async Task<IEnumerable<(int id, string name)>> Planets()
    {
        using var connection = Connect;
        var results = await connection.QueryAsync<(int, string)>(@"
            SELECT id, name FROM planet_info
        ");

        return results;
    }

    public async Task<IEnumerable<string>> Ores()
    {
        using var connection = Connect;
        var results = await connection.QueryAsync<string>(@"
            SELECT DISTINCT ore1 FROM planet_info ORDER BY id
        ");

        return results;
    }

    public async Task<IEnumerable<PlanetInfo>> PlanetsByOre(string ore)
    {
        using var connection = Connect;
        var results = await connection.QueryAsync<PlanetInfo>(@"
            SELECT * FROM planet_info
            WHERE ore1 = @ore OR ore2 = @ore OR ore3 = @ore
            ORDER BY id
        ", new{ore});
        
        return results;
    }
   
}