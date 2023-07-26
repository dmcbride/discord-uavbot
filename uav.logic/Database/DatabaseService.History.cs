global using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task AddHistory(IDbUser u, string command, string? options, string? response)
    {
        using var connect = Connect;

        await SaveUser(u);
        await connect.ExecuteAsync($@"INSERT INTO {Table.History} (user_id, command, options, response) VALUES (@User_Id, @Command, @Options, @Response)", new {
            User_Id = u.User_Id,
            Command = command,
            Response = response,
            Options = options,
        });
    }

    public async Task AddParticipationHistory(IDbUser u)
    {
        using var connect = Connect;

        var now = DateTime.UtcNow;
        var yyyymm = now.Year * 100 + now.Month;

        await connect.ExecuteAsync($@"
INSERT INTO {Table.ParticipationHistory} (user_id, yyyymm)
VALUES (@User_Id, @yyyymm)
ON DUPLICATE KEY UPDATE
    count = count + 1
", new {
    User_Id = u.User_Id,
    yyyymm = yyyymm,
}
);
    }

    public async Task<IEnumerable<TopParticipationHistory>> GetTopParticipationHistoryWinners(int minActivity, int maxWinners, int year, int month, bool includeMods)
    {
        using var connect = Connect;

        var yyyymm = year * 100 + month;

        return await connect.QueryAsync<TopParticipationHistory>($@"
SELECT u.*, count
FROM {Table.ParticipationHistory} p
JOIN {Table.Users} u ON p.user_id = u.user_id
WHERE count >= @minActivity
  AND (@includeMods OR not is_mod)
  AND yyyymm = @yyyymm
ORDER BY RAND()
LIMIT @maxWinners",
            new { minActivity, maxWinners, yyyymm, includeMods });
    }

    public class TopParticipationHistory : KnownUser
    {
        public int Count;
    }
}