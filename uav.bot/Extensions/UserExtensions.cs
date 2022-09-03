using System.Linq;
using Discord;
using Discord.WebSocket;
using uav.logic.Database.Model;

public static class UserExtensions

{
    private class DbUser : IDbUser
    {
        SocketGuildUser _u;
        SocketWebhookUser _wu;
        public DbUser(SocketGuildUser u)
        {
            _u = u;
        }
        public DbUser(SocketWebhookUser u)
        {
            _wu = u;
        }

        public ulong User_Id => _u?.Id ?? _wu.Id;

        public string User_Name => _u?.Username ?? _wu.Username;

        public string User_Nick => _u?.Nickname ?? null;

        public IEnumerable<SocketRole> Roles => _u.Roles ?? Enumerable.Empty<SocketRole>();
    }

    public static IDbUser ToDbUser(this SocketGuildUser u) => new DbUser(u);
    public static IDbUser ToDbUser(this SocketWebhookUser u) => new DbUser(u);

    // let's face it, everything we're doing actually has SocketGuildUser under the covers
    // so make it easier.
    public static IDbUser ToDbUser(this SocketUser u) => u switch {
        SocketGuildUser sgu => new DbUser(sgu),
        SocketWebhookUser swu => new DbUser(swu),
        _ => throw new System.Exception(),
    };

    public static string DisplayName(this SocketGuildUser u) => u.Nickname ?? u.Username;
}