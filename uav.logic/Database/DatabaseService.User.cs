using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Constants;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task SaveUser(IDbUser? u)
    {
        if (u is null)
        {
            return;
        }
        
        using var connection = Connect;

        var is_mod = u.Roles.Select(r => r.Id).Any(Roles.AllMods.Contains);

        await connection.ExecuteAsync($@"INSERT INTO {Table.Users} (user_id, user_name, user_nick, is_mod)
        VALUES (@User_Id, @User_Name, @User_Nick, @is_mod)
        ON DUPLICATE KEY
        UPDATE user_name = VALUES(user_name), user_nick = VALUES(user_nick), is_mod = VALUES(is_mod)", new {
          u.User_Id,
          u.User_Name,
          u.User_Nick,
          is_mod,
        });
    }

    public async Task<IDictionary<ulong, DateTimeOffset>> GetLastSeen(IEnumerable<ulong> userId, DateTimeOffset? minDate = null)
    {
        using var connection = Connect;
        var result = await connection.QueryAsync<(ulong userId, DateTimeOffset lastSeen)>($@"SELECT user_id, last_seen FROM {Table.Users} WHERE user_id IN @userId AND (@minDate IS NULL OR @minDate < last_seen)", new {
            userId,
            minDate,
        });
        return result.ToDictionary(r => r.userId, r => r.lastSeen);
    }

    public Task RegisterUser(IDbUser u, string playerId)
    {
        return RegisterUser(u.User_Id, playerId);
    }

    public async Task RegisterUser(ulong User_Id, string playerId)
    {
        using var connection = Connect;
        await connection.ExecuteAsync($@"UPDATE {Table.Users} SET player_id = @playerId WHERE user_id = @User_Id", new {
            User_Id,
            playerId,
        });
    }

    public async Task<IEnumerable<KnownUser>> GetUserPlayerIds(IEnumerable<ulong> discordIds)
    {
        using var connection = Connect;
        return await connection.QueryAsync<KnownUser>($@"SELECT user_id, player_id FROM {Table.Users} WHERE user_id IN @discordIds", new {
            discordIds
        });
    }

    public async Task<KnownUser?> GetUser(ulong User_Id)
    {
        using var connection = Connect;
        return await connection.QuerySingleOrDefaultAsync<KnownUser>($@"SELECT * FROM {Table.Users} WHERE user_id = @User_Id", new {
            User_Id
        });
    }

    public Task<UserConfig> GetUserConfig(IDbUser u) => GetUserConfig(u.User_Id);

    public async Task<UserConfig> GetUserConfig(ulong User_Id)
    {
        using var connection = Connect;
        return await connection.QuerySingleOrDefaultAsync<UserConfig>($@"SELECT * FROM {Table.UserConfig} WHERE user_id = @User_Id", new {
            User_Id
        }) ?? new UserConfig { UserId = User_Id };
    }

    public async Task UpdateUserConfig(UserConfig uc)
    {
        using var connection = Connect;
        await connection.ExecuteAsync($@"
   INSERT INTO {Table.UserConfig} (
        user_id,
        lounge_level,
        has_exodus,
        credits_1,
        credits_2,
        credits_3,
        credits_4,
        credits_5,
        credits_6,
        credits_7,
        credits_8,
        credits_9,
        credits_10,
        credits_11,
        credits_12,
        credits_13
    ) VALUES (
        @UserId,
        @LoungeLevel,
        @HasExodus,
        @Credits1,
        @Credits2,
        @Credits3,
        @Credits4,
        @Credits5,
        @Credits6,
        @Credits7,
        @Credits8,
        @Credits9,
        @Credits10,
        @Credits11,
        @Credits12,
        @Credits13
    ) ON DUPLICATE KEY UPDATE
        lounge_level = VALUES(lounge_level),
        has_exodus = VALUES(has_exodus),
        credits_1 = VALUES(credits_1),
        credits_2 = VALUES(credits_2),
        credits_3 = VALUES(credits_3),
        credits_4 = VALUES(credits_4),
        credits_5 = VALUES(credits_5),
        credits_6 = VALUES(credits_6),
        credits_7 = VALUES(credits_7),
        credits_8 = VALUES(credits_8),
        credits_9 = VALUES(credits_9),
        credits_10 = VALUES(credits_10),
        credits_11 = VALUES(credits_11),
        credits_12 = VALUES(credits_12),
        credits_13 = VALUES(credits_13)
", uc);
    }

    public async Task<KnownUser?> GetUserFromPlayerId(string playerId)
    {
        using var connection = Connect;
        return await connection.QuerySingleOrDefaultAsync<KnownUser>($@"SELECT * FROM {Table.Users} WHERE player_id = @playerId", new {
            playerId
        });
    }
}