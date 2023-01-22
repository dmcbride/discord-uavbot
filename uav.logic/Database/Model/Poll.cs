using System;
using System.Collections.Generic;
using Discord;
using uav.logic.Constants;

namespace uav.logic.Database.Model;

public class Poll : IDapperMappedType
{
    public ulong PollId { get; set; }

    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MsgId { get; set; }
    public string? PollUserKey { get; set; }
    public string? Description { get; set; }
    public int MaxOptions { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndDate { get; set; }
    public OptionsType Options { get; set; }
    public bool Completed { get; set; }
    
    public struct OptionsType : DatabaseService.IDapperJsonType
    {
        public record Option(int Id, string Text);
        public List<Option> Options { get; set; }
    }

    public EmbedBuilder ToEmbedded(int voters = 0)
    {
        var eb = new EmbedBuilder()
            .WithFooter($"Poll ID: {PollUserKey}")
            .WithTimestamp(CreatedDate)
            .WithColor(Color.Blue)
            .AddField("Poll Question", Description)
            .AddField("Number of Users Participating", voters, true)
            .AddField("Poll End Date", EndDate.ToString("yyyy-MM-dd HH:mm:ss zzz"))
            ;

        return eb;
    }

    public MessageComponent ToSelectMenu(string commandName)
    {
        var choices = new List<SelectMenuOptionBuilder>();
        foreach (var option in Options.Options)
        {
            var text = $"{IpmEmoji.Team(option.Id)} {option.Text}";
            choices.Add(new SelectMenuOptionBuilder(text, option.Id.ToString()));
        }

        var sm = new SelectMenuBuilder()
            .WithCustomId($"{commandName}:vote:{PollId}")
            .WithPlaceholder(MaxOptions == 1 ? "Select an option" : $"Select up to {MaxOptions} options")
            .WithMinValues(1)
            .WithMaxValues(MaxOptions)
            .WithOptions(choices)
            ;

        return new ComponentBuilder()
            .WithSelectMenu(sm)
            .Build();
    }
}