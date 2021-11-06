using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using uav.logic.Database.Model;
using uav.logic.Models;

namespace uav.logic.Database
{
    public class DatabaseService
    {
        private string connectionString = Environment.GetEnvironmentVariable("uav_dbConnection");
        private MySqlConnection Connect => new MySqlConnection(connectionString);

        public async Task<(int atThisCredit, int inTier)> AddArkValue(ArkValue v, double tierMin, double tierMax)
        {
            using var connection = Connect;
            await connection.ExecuteAsync(@"INSERT INTO ark_value (reporter, gv, base_credits) VALUES (@Reporter, @Gv, @Base_Credits)", v);

            // find out how many we have now.
            var parameters = new {v.Base_Credits};
            var atThisCredit = await connection.QueryAsync<int>(
                @"SELECT COUNT(*) FROM ark_value WHERE base_credits = @Base_Credits AND NOT oopsed",
                parameters
            );

            // how many in this tier
            var inTier = await CountInRange(tierMin, tierMax);

            return (atThisCredit.Single(), inTier);
        }

        public async Task<int> CountInRange(double gvMin, double gvMax)
        {
            using var connection = Connect;
            var parameters = new {gvMin, gvMax};
            var values = await connection.QueryAsync<int>(
                @"SELECT COUNT(*) FROM ark_value WHERE gv >= @gvMin AND gv < @gvMax AND NOT oopsed",
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

        public async Task<UserCounts> CountByUser(string user)
        {
            using var connection = Connect;
            var parameters = new {user};
            var values = await connection.QueryAsync<UserCounts>(
                @"SELECT COUNT(*) total, COUNT(distinct base_credits) distinctBaseCredits, COUNT(DISTINCT FLOOR(LOG10(gv))) distinctTiers FROM ark_value WHERE reporter = @user AND NOT oopsed",
                parameters
            );
            return values.Single();
        }

        public async Task<IEnumerable<ArkValue>> FindValue(double v)
        {
            using var connection = Connect;
            var values = await connection.QueryAsync<ArkValue>(
                @"SELECT * FROM (
                    SELECT * FROM ark_value
                     WHERE gv < @v AND NOT oopsed
                     ORDER BY gv DESC
                     LIMIT 5
                  ) UNION (
                    SELECT * FROM ark_value
                     WHERE gv > @v AND NOT oopsed
                     ORDER BY gv ASC
                     LIMIT 5
                     )", new { v }
            );

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

            var sql = $"SELECT * FROM ark_value {where}";
            //Console.WriteLine(sql);

            var values = await connection.QueryAsync<ArkValue>(sql);
            await connection.ExecuteAsync($"UPDATE ark_value SET oopsed = 1 {where}");

            return values;
        }

        public async IAsyncEnumerable<(int tier, double minGv, int minCredit, IEnumerable<(double gv, int baseCredits)> data)> DataByTier()
        {
            using var connection = Connect;
            var credits = new Credits();

            foreach (var t in credits.AllTiers())
            {
                var values = await connection.QueryAsync<ArkValue>(
                    $"SELECT * FROM ark_value WHERE gv >= {t.gvMin} AND gv < {t.gvMax} AND NOT oopsed"
                );

                yield return (t.tier, t.gvMin, t.creditMin, values.Select(a => (a.Gv, a.Base_Credits)));
            }
        }

        public async Task<IEnumerable<LatestStoreVersion>> GetLatestStoreVersions()
        {
            using var connection = Connect;
            var values = await connection.QueryAsync<LatestStoreVersion>(
                $"SELECT * FROM latest_store_versions"
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
                @$"SELECT min(gv) AS gv, base_credits FROM ark_value WHERE NOT oopsed AND gv >= {minGv} AND gv < {maxGv} GROUP BY base_credits
                UNION
                SELECT max(gv) AS gv, base_credits FROM ark_value WHERE NOT oopsed AND gv >= {minGv} AND gv < {maxGv} GROUP BY base_credits"
            );

            return values;
        }
    }
}