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
            await connection.ExecuteAsync(@"INSERT INTO ark_value (reporter, gv, base_credits) VALUES (@Reporter, @Gv, @BaseCredits)", v);

            // find out how many we have now.
            var parameters = new {v.BaseCredits};
            var atThisCredit = await connection.QueryAsync<int>(
                @"SELECT COUNT(*) FROM ark_value WHERE base_credits = @BaseCredits",
                parameters
            );

            var inTier = await CountInRange(tierMin, tierMax);

            return (atThisCredit.Single(), inTier);
        }

        public async Task<int> CountInRange(double gvMin, double gvMax)
        {
            using var connection = Connect;
            var parameters = new {gvMin, gvMax};
            var values = await connection.QueryAsync<int>(
                @"SELECT COUNT(*) FROM ark_value WHERE gv >= @gvMin AND gv < @gvMax AND oopsed = 0",
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
                     WHERE gv < @v AND oopsed = 0
                     ORDER BY gv DESC
                     LIMIT 5
                  ) UNION (
                    SELECT * FROM ark_value
                     WHERE gv > @v AND oopsed = 0
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
                $"(gv >= {t.gvMin} AND gv < {t.gvMax} AND (base_credits < {t.creditMin} OR base_credits > {t.creditMax}))"
            );

            var sql = $"SELECT * FROM ark_value WHERE {string.Join(" OR ", queries)}";
            Console.WriteLine(sql);

            var values = await connection.QueryAsync<ArkValue>(sql);

            return values;
        }
    }
}