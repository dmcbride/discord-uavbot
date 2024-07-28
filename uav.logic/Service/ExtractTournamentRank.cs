using System.Text.RegularExpressions;

namespace uav.logic.Service;

public class ExtractTournamentRank : TesseractExtractor<int?>
{
  private static Regex RankExtractor = new Regex(@"You\s+finished\s+at\s+rank\s+(?<rank>\d+)", RegexOptions.Singleline);
  protected override int? FindValues(string text)
  {
    if (text == null)
    {
      return null;
    }
    var m = RankExtractor.Match(text);
    if (m.Success)
    {
      return int.Parse(m.Groups["rank"].Value);
    }
    return null;
  }
}
