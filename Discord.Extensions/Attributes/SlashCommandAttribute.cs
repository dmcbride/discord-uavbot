using System.Collections.ObjectModel;
using System.Reflection;

namespace Discord.Extensions.Attributes;

public class SlashCommandAttribute : BaseInteractionHandlerAttribute
{
  public SlashCommandAttribute(string commandName, string description) : base(commandName, description)
  {
  }
}

[AttributeUsage(AttributeTargets.Method)]
public class SubCommandAttribute : Attribute
{
  public SubCommandAttribute(string commandName, string description)
  {
    CommandName = commandName;
    Description = description;
  }

  public string CommandName { get; }
  public string Description { get; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class SubCommandParameterAttribute : Attribute
{
  public SubCommandParameterAttribute(string description, bool required = false)
  {
    Description = description;
    Required = required;
  }

  public SubCommandParameterAttribute(string description, int min, int max, bool required = false)
  {
    Description = description;
    Required = required;
    Min = min;
    Max = max;
  }

  public string Description { get; }
  public bool Required { get; }
  public int? Min { get; }
  public int? Max { get; }

  private static IReadOnlyCollection<(Type t, ApplicationCommandOptionType a)> _typeMap = new (Type, ApplicationCommandOptionType)[]
  {
    ( typeof(string), ApplicationCommandOptionType.String ),
    ( typeof(long), ApplicationCommandOptionType.Integer ),
    ( typeof(bool), ApplicationCommandOptionType.Boolean ),
    ( typeof(IUser), ApplicationCommandOptionType.User ),
    ( typeof(IChannel), ApplicationCommandOptionType.Channel ),
    ( typeof(IRole), ApplicationCommandOptionType.Role ),
    ( typeof(IMentionable), ApplicationCommandOptionType.Mentionable ),
  };

  private ApplicationCommandOptionType CalculatedType(Type parameterType)
  {
    var expectedType = _typeMap.First(t => t.t.IsAssignableFrom(parameterType));

    return expectedType.a;
  }

  public SlashCommandOptionBuilder ToSlashCommandOptionBuilder(ParameterInfo parameter)
  {
    var option = new SlashCommandOptionBuilder()
        .WithName(parameter.Name)
        .WithType(CalculatedType(parameter.ParameterType))
        .WithDescription(Description)
        .WithRequired(Required);
    
    if (Min.HasValue)
    {
      option.WithMinValue(Min.Value);
    }
    if (Max.HasValue)
    {
      option.WithMaxValue(Max.Value);
    }

    return option;
  }
}