using System.Text.RegularExpressions;
using uav.logic.Models;

namespace uav.logic.Service;

public class ExtractGvBaseCredits : TesseractExtractor<ExtractGvBaseCredits.ExtractedValues>
{
  public record ExtractedValues(GV gv, int credits);

  private Regex gvExtractor = new(@"Galaxy\s+Value\s*\$(\d+\.?\d*(?:E\+\d+|[a-zA-Z]{1,2}|0))");
  private Regex creditsExtractor = new(@"Base Reward\s*(\d+)(?:\s*(\d+))");
  protected override ExtractedValues? FindValues(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return null;
    }
    var gvMatch = gvExtractor.Match(text);
    var creditMatch = creditsExtractor.Match(text);

    // sometimes tesseract thinks an ending O is a 0, so we'll fix that here.
    var gvBase = gvMatch.Groups[1].Value;
    if (gvBase.EndsWith("0") && !gvBase.Contains("E"))
    {
      gvBase = gvBase[..^1] + "O";
    }

    if (gvMatch.Success && creditMatch.Success &&
        GV.TryFromString(gvBase, out var gv, out _) &&
        int.TryParse(creditMatch.Groups[1].Value, out var credits))
    {
      var creditService = new Credits();
      var expectedMinimumCredits = creditService.TierCredits(gv);
      var expectedMaximumCredits = creditService.TierCredits(gv, 1);

      if (credits >= expectedMinimumCredits && credits < expectedMaximumCredits)
      {
        return new ExtractedValues(gv, credits);
      }

      if (creditMatch.Groups.Count > 2 && creditMatch.Groups[2].Success && creditMatch.Groups[2].Length > 0 &&
          int.TryParse(string.Join("", creditMatch.Groups[1].Value, creditMatch.Groups[2].Value), out var credits2) &&
          credits2 >= expectedMinimumCredits && credits2 < expectedMaximumCredits)
      {
        return new ExtractedValues(gv, credits2);
      }
    }
    return null;
  }
}