using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

public partial class DatabaseService
{
    public async Task AddHint(Hint h)
    {
        using var connect = Connect;
        await connect.ExecuteAsync(
            @"INSERT INTO hints (user_id, hint_name, title, hint_text)
            VALUES (@UserId, @HintName, @Title, @HintText)
            ON DUPLICATE KEY UPDATE
                hint_text = @HintText,
                title = @Title,
                updated = NOW()
                ",
             h);
    }

    public async Task RemoveHint(Hint h)
    {
        using var connect = Connect;
        await connect.ExecuteAsync(
            @"DELETE FROM hints WHERE user_id = @UserId AND hint_name = @HintName",
             h);
    }

    public async Task<IEnumerable<Hint>> GetHints(IDbUser u)
    {
        using var connect = Connect;
        var parameters = new {u.User_Id};
        return await connect.QueryAsync<Hint>(
            @"SELECT hint_name, title FROM hints WHERE user_id = @User_Id",
            parameters
        );
    }

    public async Task<IEnumerable<Hint>> GetAllHints()
    {
        using var connect = Connect;
        return await connect.QueryAsync<Hint>(
            @"SELECT * FROM hints"
        );
    }

    public async Task<Hint?> GetHint(IDbUser? u, string hintName)
    {
        using var connect = Connect;
        var parameters = new {u?.User_Id, hintName};
        return (await connect.QueryAsync<Hint>(
            @"SELECT * FROM hints WHERE (@User_Id IS NULL OR user_id = @User_Id) AND hint_name = @hintName ORDER BY CASE WHEN user_id = @User_Id THEN 0 ELSE 1 END LIMIT 1",
            parameters
        )).SingleOrDefault();
    }

    public async Task<IEnumerable<KnownUser>> GetHintUsers()
    {
        using var connect = Connect;
        return await connect.QueryAsync<KnownUser>(
            @"SELECT distinct u.* FROM known_users u
            JOIN hints h ON u.user_id = h.user_id"
        );
    }

}