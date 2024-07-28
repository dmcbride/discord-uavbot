using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Constants;
using uav.logic.Database.Model;

namespace uav.logic.Database;

public partial class DatabaseService
{
  public async Task<(IEnumerable<GuildMember> guildMembers, IDictionary<ulong, string> userNames)> GetGuildMembers()
  {
    using var connection = Connect;
    var members = (await connection.QueryAsync<GuildMember>("SELECT * FROM guild_member")).ToArray();
    var userNames = (await connection.QueryAsync<KnownUser>("SELECT user_id, user_name FROM user WHERE user_id IN @UserIds", new {UserIds = members.Select(m => m.UserId)}))
      .ToDictionary(u => u.User_Id, u => u.Name());
    return (members, userNames);
  }

  public async Task JoinGuilds(IDbUser user, bool isPermanent)
  {
    using var connection = Connect;
    await connection.ExecuteAsync(@"
      INSERT INTO guild_members (user_id, is_temporary)
      VALUES (@User_Id, @IsTemporary)
      ON DUPLICATE KEY UPDATE is_temporary = @IsTemporary
      ", new {user.User_Id, IsTemporary = !isPermanent});
  }

  public async Task LeaveGuilds(IDbUser user)
  {
    using var connection = Connect;
    await connection.ExecuteAsync(@"
      DELETE FROM guild_members
      WHERE user_id = @User_Id
      ", new {user.User_Id});
  }

  public async Task<(bool isInGuilds, ulong typeRole)> IsInGuilds(IDbUser user)
  {
    using var connection = Connect;
    var guildMember = await connection.QueryFirstOrDefaultAsync<GuildMember>(@"
      SELECT is_temporary FROM guild_members
      WHERE user_id = @User_Id
      ", new {user.User_Id});
    var role = guildMember?.IsTemporary switch
    {
      false => Roles.PermanentGuildAccessRole,
      true => Roles.GuildAccessRole,
      _ => 0ul
    };
    return (guildMember != null, role);
  }

  public async Task CleanupTemporaryGuildMembers()
  {
    using var connection = Connect;
    await connection.ExecuteAsync(@"
      DELETE FROM guild_members
      WHERE is_temporary = 1
      ");
  }
}