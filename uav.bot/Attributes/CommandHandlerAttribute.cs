using System;

namespace uav.bot.Attributes;

public abstract class CommandHandlerAttribute : Attribute
{
    public CommandHandlerAttribute(string commandName)
    {
        CommandName = commandName;
    }

    public string CommandName { get; }
}