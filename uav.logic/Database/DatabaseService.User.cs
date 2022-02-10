using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task SaveUser(IDbUser u)
    {
        using var connection = Connect;

        await connection.ExecuteAsync(@"INSERT INTO known_users (user_id, user_name, user_nick)
        VALUES (@User_Id, @User_Name, @User_Nick)
        ON DUPLICATE KEY
        UPDATE user_name = VALUES(user_name), user_nick = VALUES(user_nick)", new {
            User_Id = u.User_Id,
            User_Name = u.User_Name,
            User_Nick = u.User_Nick,
        });
    }
}