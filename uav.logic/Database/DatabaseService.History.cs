global using System.Threading.Tasks;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task AddHistory(IDbUser u, string command, string options, string response)
    {
        using var connect = Connect;

        await SaveUser(u);
        await connect.ExecuteAsync(@"INSERT INTO history (user_id, command, options, response) VALUES (@User_Id, @Command, @Options, @Response)", new {
            User_Id = u.User_Id,
            Command = command,
            Response = response,
            Options = options,
        });
    }
}