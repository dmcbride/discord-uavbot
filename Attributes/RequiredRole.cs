using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace uav.Attributes
{
    public class RequiredRoleAttribute : Attribute
    {
        private string _role;
        public RequiredRoleAttribute(string role)
        {
            _role = role;
        }

        public bool IsAcceptable(SocketUser socketUser)
        {
            if (!(socketUser is SocketGuildUser user))
            {
                return false;
            }

            return user.Roles.Any(r => r.Name == _role);
        }
    }
}