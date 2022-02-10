using System;
using System.Collections.Generic;
using Discord;

namespace uav.logic.Constants;

public static class Support
{
    private static string[] supportStatements = new [] {
        $"Support UAV {IpmEmoji.heart}",
        $"Like UAV? Show your support!",
        $"UAV needs your support",
        $"If UAV helps, show your support.",
    };

    private static Random rng = new Random();
    private static T randomItem<T>(IList<T> items) => items[rng.Next(items.Count)];

    public static string SupportStatement => $"[{randomItem(supportStatements)}](https://ko-fi.com/tanktalus)";
    public static EmbedFieldBuilder SupportStatementFieldEmbed => new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("\u200B")
                .WithValue(Support.SupportStatement);
}