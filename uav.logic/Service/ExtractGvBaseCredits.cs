using System.Text.RegularExpressions;
using uav.logic.Models;

namespace uav.logic.Service;

public class ExtractGvBaseCredits : TesseractExtractor<ExtractGvBaseCredits.ExtractedValues>
{
  public record ExtractedValues(GV gv, int credits);

  private Regex gvExtractor = new Regex(@"Galaxy Value\s*\$(\d+\.?\d*[a-zA-Z]{1,2}|\d+\.?\d*E\+\d+)");
  private Regex creditsExtractor = new Regex(@"Base Reward\s*(\d+)(?:\s*(\d+))");
  protected override ExtractedValues? FindValues(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return null;
    }
    var gvMatch = gvExtractor.Match(text);
    var creditMatch = creditsExtractor.Match(text);

    if (gvMatch.Success && creditMatch.Success &&
        GV.TryFromString(gvMatch.Groups[1].Value, out var gv, out _) &&
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