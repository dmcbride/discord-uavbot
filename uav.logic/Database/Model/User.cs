using uav.logic.Extensions;

namespace uav.logic.Database.Model;

public interface IDbUser
{
    public ulong User_Id { get; }
    public string User_Name { get; }
    public string User_Nick { get; }

    public string Name() => User_Name.IsNullOrEmpty() ? User_Nick : User_Name;
}