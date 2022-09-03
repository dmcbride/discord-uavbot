using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Constants;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task SaveUser(IDbUser u)
    {
        using var connection = Connect;

        var is_mod = u.Roles.Select(r => r.Id).Any(Roles.AllMods.Contains);

        await connection.ExecuteAsync(@"INSERT INTO known_users (user_id, user_name, user_nick, is_mod)
        VALUES (@User_Id, @User_Name, @User_Nick, @is_mod)
        ON DUPLICATE KEY
        UPDATE user_name = VALUES(user_name), user_nick = VALUES(user_nick), is_mod = VALUES(is_mod)", new {
            User_Id = u.User_Id,
            User_Name = u.User_Name,
            User_Nick = u.User_Nick,
            is_mod,
        });
    }

    public Task RegisterUser(IDbUser u, string playerId)
    {
        return RegisterUser(u.User_Id, playerId);
    }

    public async Task RegisterUser(ulong User_Id, string playerId)
    {
        using var connection = Connect;
        await connection.ExecuteAsync(@"UPDATE known_users SET player_id = @playerId WHERE user_id = @User_Id", new {
            User_Id,
            playerId,
        });
    }

    public async Task<IEnumerable<KnownUser>> GetUserPlayerIds(IEnumerable<ulong> discordIds)
    {
        using var connection = Connect;
        return await connection.QueryAsync<KnownUser>(@"SELECT user_id, player_id FROM known_users WHERE user_id IN @discordIds", new {
            discordIds
        });
    }

    public async Task<KnownUser> GetUser(ulong User_Id)
    {
        using var connection = Connect;
        return await connection.QuerySingleOrDefaultAsync<KnownUser>(@"SELECT * FROM known_users WHERE user_id = @User_Id", new {
            User_Id
        });
    }

}