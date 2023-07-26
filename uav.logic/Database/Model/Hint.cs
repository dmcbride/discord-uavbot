using System;
using System.ComponentModel;

namespace uav.logic.Database.Model;

public class Hint : IDapperMappedType
{
    [Description("user_id")]
    public ulong UserId { get; set; }
    [Description("hint_name")]
    public string? HintName { get; set; }
    [Description("title")]
    public string? Title { get; set; }
    [Description("hint_text")]
    public string? HintText { get; set; }

    [Description("created")]
    public DateTimeOffset Created { get; set; }
    [Description("updated")]
    public DateTimeOffset Updated { get; set; }

    [Description("approved_by")]
    public ulong? ApprovedBy { get; set; }
    [Description("approved_at")]
    public DateTimeOffset? ApprovedAt { get; set; }

    public Hint()
    {
        Created = DateTimeOffset.Now;
        Updated = DateTimeOffset.Now;
    }

    public Hint(IDbUser dbUser, string? hintName = null) : this()
    {
        UserId = dbUser.User_Id;
        HintName = hintName;
    }

    public Hint(IDbUser dbUser, string hintName, string title, string hintText)
    : this(dbUser, hintName)
    {
        Title = title;
        HintText = hintText;
    }
}
