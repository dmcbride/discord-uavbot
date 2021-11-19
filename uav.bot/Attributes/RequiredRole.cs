using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace uav.Attributes
{
    public class RequiredRoleAttribute : Attribute
    {
        private ISet<string> _roles;
        public RequiredRoleAttribute(params string[] roles)
        {
            _roles = roles.ToHashSet();
        }

        public bool IsAcceptable(SocketUser socketUser)
        {
            if (!(socketUser is SocketGuildUser user))
            {
                return false;
            }

            return user.Roles.Any(r => _roles.Contains(r.Name));
        }
    }
}