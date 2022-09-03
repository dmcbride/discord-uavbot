using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using uav.logic.Constants;

namespace uav.bot.SlashCommand.Admin;

public abstract class BaseAdminSlashCommand : BaseSlashCommand
{
    protected virtual IEnumerable<ulong> AllowedRoles => Roles.Admins;
    
    public override Task Invoke(SocketSlashCommand command)
    {
        var allowed = IsInARole(AllowedRoles);

        if (!allowed)
        {
            return RespondAsync($"This command is restricted.", ephemeral: true);
        }

        return InvokeAdminCommand(command);
    }

    protected abstract Task InvokeAdminCommand(SocketSlashCommand command);
}