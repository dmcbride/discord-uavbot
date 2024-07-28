using System.Linq;
using Discord.WebSocket;
using uav.logic.Database.Model;
using uav.logic.Extensions;

public static partial class UserExtensions

{
    private partial class DbUser : IDbUser
    {
        SocketGuildUser? _u;
        SocketWebhookUser? _wu;
        SocketUser? _su;
        public DbUser(SocketGuildUser u)
        {
            _u = u;
        }
        public DbUser(SocketWebhookUser u)
        {
            _wu = u;
        }
        public DbUser(SocketUser u)
        {
            _su = u;
        }

        public ulong User_Id => _u?.Id ?? _wu?.Id ?? _su!.Id;

        public string User_Name => _u?.Username ?? _wu?.Username ?? _su!.Username;

        public string? User_Nick => _u?.Nickname ?? null;

        public IEnumerable<SocketRole> Roles => _u?.Roles ?? Enumerable.Empty<SocketRole>();

        public string UserNameDisplay => User_Name.FixName();
    }

    public static IDbUser ToDbUser(this SocketGuildUser u) => new DbUser(u);
    public static IDbUser ToDbUser(this SocketWebhookUser u) => new DbUser(u);

    // let's face it, everything we're doing actually has SocketGuildUser under the covers
    // so make it easier.
    public static IDbUser ToDbUser(this SocketUser u) => u switch {
        SocketGuildUser sgu => new DbUser(sgu),
        SocketWebhookUser swu => new DbUser(swu),
        SocketUser sgu => new DbUser(sgu),
        _ => throw new System.Exception($"Unknown user type {u.GetType().Name}"),
    };

    public static string DisplayName(this SocketGuildUser u) => (u.Nickname ?? u.Username).FixName();
}