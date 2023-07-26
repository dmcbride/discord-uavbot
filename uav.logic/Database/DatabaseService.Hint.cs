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
                updated = UTC_TIMESTAMP(),
                approved_by = NULL,
                approved_at = NULL
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
            @"SELECT hint_name, title
            FROM hints
            WHERE user_id = @User_Id
            AND approved_by IS NOT NULL",
            parameters
        );
    }

    public async Task<IEnumerable<Hint>> GetAllHints()
    {
        using var connect = Connect;
        return await connect.QueryAsync<Hint>(
            @"SELECT * FROM hints WHERE approved_by IS NOT NULL"
        );
    }

    public async Task<Hint?> GetHint(IDbUser? u, string hintName, bool includeUnapproved = false)
    {
        using var connect = Connect;
        var parameters = new {u?.User_Id, hintName, includeUnapproved};
        return (await connect.QueryAsync<Hint>(
            @"SELECT * FROM hints
            WHERE (@User_Id IS NULL OR user_id = @User_Id)
            AND hint_name = @hintName
            AND (@includeUnapproved OR approved_by IS NOT NULL)
            ORDER BY CASE WHEN user_id = @User_Id THEN 0 ELSE 1 END LIMIT 1",
            parameters
        )).SingleOrDefault();
    }

    public async Task<IEnumerable<KnownUser>> GetHintUsers()
    {
        using var connect = Connect;
        return await connect.QueryAsync<KnownUser>(
            $@"SELECT distinct u.* FROM {Table.Users} u
            JOIN hints h ON u.user_id = h.user_id
            WHERE approved_by IS NOT NULL"
        );
    }

    public async Task ApproveHint(IDbUser u, ulong hintUser, string hintName)
    {
        using var connect = Connect;
        await connect.ExecuteAsync(
            @"UPDATE hints SET approved_by = @approved_by WHERE user_id = @hintUser AND hint_name = @hintName",
            new {
                hintUser,
                hintName,
                approved_by = u.User_Id,
            }
        );
    }

    public async Task RejectHint(ulong hintUser, string hintName)
    {
        // rejecting is just deleting.        
        using var connect = Connect;
        await connect.ExecuteAsync(
            @"DELETE FROM hints WHERE user_id = @UserId AND hint_name = @HintName",
            new {
                UserId = hintUser,
                HintName = hintName,
            });
   }

}