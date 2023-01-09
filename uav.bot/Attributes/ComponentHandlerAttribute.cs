using System;

namespace uav.bot.Attributes;

public class ComponentHandlerAttribute : CommandHandlerAttribute
{
    public ComponentHandlerAttribute(string commandName) : base(commandName)
    {
    }
}
