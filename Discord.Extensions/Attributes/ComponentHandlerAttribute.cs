namespace Discord.Extensions.Attributes;

public class SelectMenuHandlerAttribute : BaseInteractionHandlerAttribute
{
    public SelectMenuHandlerAttribute(string commandName, string description) : base(commandName, description)
    {
    }
}
