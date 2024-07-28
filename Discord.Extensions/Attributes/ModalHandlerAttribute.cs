namespace Discord.Extensions.Attributes;

public abstract class ModalHandlerAttribute : BaseInteractionHandlerAttribute
{
    public ModalHandlerAttribute(string commandName, string description) : base(commandName, description)
    {
    }
}