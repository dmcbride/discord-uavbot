using System.Collections.Generic;
using Discord.WebSocket;
using uav.logic.Extensions;

namespace uav.logic.Database.Model;

public interface IDbUser
{
    public ulong User_Id { get; }
    public string User_Name { get; }
    public string User_Nick { get; }
    public IEnumerable<SocketRole> Roles { get; }

    public string Name() => User_Nick.IsNullOrEmpty() ? User_Name : User_Nick;
}

public class KnownUser
{
    public ulong User_Id;
    public string User_Name;
    public string User_Nick;
    public bool Is_Mod;
    public string Player_Id;

    public string Name() => User_Nick.IsNullOrEmpty() ? User_Name : User_Nick;
}