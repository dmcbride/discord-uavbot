namespace Discord.Extensions.Attributes;

public class ButtonCommandAttribute : BaseInteractionHandlerAttribute
{
    public ButtonCommandAttribute(string commandName, string description) : base(commandName, description)
    {
    }
}