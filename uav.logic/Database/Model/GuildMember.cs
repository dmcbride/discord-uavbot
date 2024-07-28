using System;

namespace uav.logic.Database.Model;

public class GuildMember : IDapperMappedType
{
  public ulong UserId { get; set; }
  public bool IsTemporary { get; set; }
  DateTimeOffset AddedAt { get; set; }
}