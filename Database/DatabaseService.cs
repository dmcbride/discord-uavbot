using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using uav.Database.Model;

namespace uav.Database
{
    public class DatabaseService
    {
        private string connectionString = Environment.GetEnvironmentVariable("uav_dbConnection");
        private MySqlConnection Connect => new MySqlConnection(connectionString);

        public async Task AddArkValue(ArkValue v)
        {
            using var connection = Connect;
            await connection.ExecuteAsync(@"INSERT INTO ark_value (reporter, gv, base_credits) VALUES (@Reporter, @Gv, @BaseCredits)", v);
        }

        public async Task<IEnumerable<ArkValue>> FindValue(double v)
        {
            using var connection = Connect;
            var values = await connection.QueryAsync<ArkValue>(
                @"SELECT * FROM (
                    SELECT * FROM ark_value
                     WHERE gv < @v
                     ORDER BY gv DESC
                     LIMIT 5
                  ) UNION (
                    SELECT * FROM ark_value
                     WHERE gv > @v
                     ORDER BY gv ASC
                     LIMIT 5
                     )", new { v }
            );

            

            return values.OrderBy(v => v.Gv);
        }
    }
}