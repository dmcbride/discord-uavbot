using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using uav.logic.Database.Model;
using uav.logic.Models;

namespace uav.logic.Database;

public partial class DatabaseService
{
    private string? connectionString = Environment.GetEnvironmentVariable("uav_dbConnection");
    private MySqlConnection Connect => new MySqlConnection(connectionString);

    private struct Table
    {
        public const string Users = "known_users";
        public const string UserConfig = "user_config";
        public const string ArkValue = "ark_value";
        public const string LatestStoreVersions = "latest_store_versions";
        public const string Hints = "hints";
        public const string History = "history";
        public const string Polls = "polls";
        public const string PollVotes = "poll_votes";
        public const string ParticipationHistory = "participation_history";
    }

    public async Task<(bool success, int atThisCredit, int inTier)> AddArkValue(ArkValue v, double tierMin, double tierMax, IDbUser user)
    {
        await SaveUser(user);

        using var connection = Connect;
        var success = true;
        try {
            await connection.ExecuteAsync($@"INSERT INTO {Table.ArkValue} (gv, base_credits, user_id) VALUES (@Gv, @Base_Credits, @user_id)", v);
        } catch (MySqlException e) when (e.Number == 1062) {
            // duplicate key
            success = false;
        }

        // find out how many we have now.
        var parameters = new {v.Base_Credits};
        var atThisCredit = await connection.QueryAsync<int>(
            $@"SELECT COUNT(*) FROM {Table.ArkValue} WHERE base_credits = @Base_Credits AND NOT oopsed",
            parameters
        );

        // how many in this tier
        var inTier = await CountInRange(tierMin, tierMax);

        await connection.CloseAsync();

        return (success, atThisCredit.Single(), inTier);
    }

    public async Task<int> CountInRange(double gvMin, double gvMax)
    {
        using var connection = Connect;
        var parameters = new {gvMin, gvMax};
        var values = await connection.QueryAsync<int>(
            $@"SELECT COUNT(*) FROM {Table.ArkValue} WHERE gv >= @gvMin AND gv < @gvMax AND NOT oopsed",
            parameters
        );
        return values.Single();
    }

    public struct UserCounts
    {
        public int total;
        public int distinctBaseCredits;
        public int distinctTiers;
    }

    public async Task<UserCounts> CountByUser(IDbUser user)
    {
        using var connection = Connect;
        var parameters = new {user.User_Id};
        var values = await connection.QueryAsync<UserCounts>(
            $@"SELECT COUNT(*) total, COUNT(distinct base_credits) distinctBaseCredits, COUNT(DISTINCT FLOOR(LOG10(gv))) distinctTiers FROM {Table.ArkValue} WHERE user_id = @user_id AND NOT oopsed",
            parameters
        );
        return values.Single();
    }

    public async Task<IEnumerable<ArkValue>> FindValueByCredit(int credit)
    {
        using var connection = Connect;
        var sql = $@"
SELECT * FROM (
    SELECT *
      FROM {Table.ArkValue}
     WHERE base_credits <= @c AND NOT oopsed
     ORDER BY gv DESC
     LIMIT 5
) x UNION SELECT * FROM (
    SELECT *
      FROM {Table.ArkValue}
     WHERE base_credits >= @c AND NOT oopsed
     ORDER BY gv ASC
     LIMIT 5
) y";
        var values = await connection.QueryAsync<ArkValue>(sql, new {c=credit});

        return values.OrderBy(v => v.Gv);
    }

    public async Task<IEnumerable<ArkValue>> FindOutOfRange()
    {
        using var connection = Connect;
        var credits = new Credits();

        var queries = credits.AllTiers().Select((t, i) =>
            $"(base_credits = {t.creditMin} OR gv >= {t.gvMin} AND gv < {t.gvMax} AND (base_credits < {t.creditMin} OR base_credits > {t.creditMax}))"
        );
        var where = $"WHERE NOT oopsed AND ({string.Join(" OR ", queries)})";

        var sql = $"SELECT * FROM {Table.ArkValue} {where}";

        var values = await connection.QueryAsync<ArkValue>(sql);
        await connection.ExecuteAsync($"UPDATE {Table.ArkValue} SET oopsed = 1 {where}");

        return values;
    }

    public async IAsyncEnumerable<(int tier, double minGv, int minCredit, IEnumerable<(double gv, int baseCredits)> data)> DataByTier()
    {
        using var connection = Connect;
        var credits = new Credits();

        foreach (var t in credits.AllTiers())
        {
            var values = await connection.QueryAsync<ArkValue>(
                $"SELECT * FROM {Table.ArkValue} WHERE gv >= {t.gvMin} AND gv < {t.gvMax} AND NOT oopsed"
            );

            yield return (t.tier, t.gvMin, t.creditMin, values.Select(a => (a.Gv, a.Base_Credits)));
        }
    }

    public async Task<IEnumerable<LatestStoreVersion>> GetLatestStoreVersions()
    {
        using var connection = Connect;
        var values = await connection.QueryAsync<LatestStoreVersion>(
            $"SELECT * FROM {Table.LatestStoreVersions}"
        );

        return values;
    }

    public class CreditMinMaxValues
    {
        public double gv;
        public int base_credits;
    }

    public async Task<IEnumerable<CreditMinMaxValues>> GetMinMaxValuesForCredits(GV gv)
    {
        using var connection = Connect;
        var (minGv, maxGv) = gv.TierRange();

        var values = await connection.QueryAsync<CreditMinMaxValues>(
            @$"SELECT min(gv) AS gv, base_credits FROM {Table.ArkValue} WHERE NOT oopsed AND gv >= {minGv} AND gv < {maxGv} GROUP BY base_credits
            UNION
            SELECT max(gv) AS gv, base_credits FROM {Table.ArkValue} WHERE NOT oopsed AND gv >= {minGv} AND gv < {maxGv} GROUP BY base_credits"
        );

        return values;
    }
}
