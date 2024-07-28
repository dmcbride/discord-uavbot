namespace Discord.Extensions.Attributes;

public abstract class BaseInteractionHandlerAttribute : Attribute
{
    public BaseInteractionHandlerAttribute(string commandName, string description)
    {
        Name = commandName;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; }
}