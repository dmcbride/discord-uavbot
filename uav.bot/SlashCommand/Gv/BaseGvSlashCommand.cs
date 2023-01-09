using System;
using System.Text.RegularExpressions;
using uav.logic.Constants;
using uav.logic.Models;

namespace uav.bot.SlashCommand.Gv;

public abstract class BaseGvSlashCommand : BaseSlashCommand
{
    protected const double cashArkChance = .7d;
    protected const double dmArkChance = 1 - cashArkChance;
    protected const double arksPerHour = 10d;

    protected string ZeroOrMore(int count, string singular, string plural = null)
    {
        var things = singular;
        if (count != 1)
        {
            things = plural ?? singular + (pluralNeedsE.IsMatch(singular) ? "es" : "s");
        }
        return $"{count} {things}";
    }

    protected string ZeroOrMoreCash(int count, string boost) => ZeroOrMore(count, $"{IpmEmoji.boostcashwindfall} {boost}");

    private static readonly Regex pluralNeedsE = new Regex("(s|sh|ch|x|z)$");
}