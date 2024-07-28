using System.Text.RegularExpressions;
using log4net;
using log4net.Core;

namespace uav.logic.Service;

public partial class ExtractFarmingInfo : TesseractExtractor<(string? credits, string? galaxies)?>
{
  private readonly ILog logger = LogManager.GetLogger(typeof(ExtractFarmingInfo));
  private readonly Regex galaxiesExtractor = GalaxiesExtractorRegex();
  private readonly Regex creditsExtractor = CreditsExtractorRegex();
  protected override (string? credits, string? galaxies)? FindValues(string text)
  {
    text ??= string.Empty;

    logger.Debug($"{nameof(FindValues)}: {text}");
    var (galaxies, credits) = (galaxiesExtractor.Match(text), creditsExtractor.Match(text));
    if (galaxies.Success && credits.Success)
    {
      return (credits.Groups["credits"].Value, galaxies.Groups["galaxies"].Value);
    }
    return null;
  }

    [GeneratedRegex("Galaxies\\s+Sold\\s+(?<galaxies>[\\d,]+)")]
    private static partial Regex GalaxiesExtractorRegex();
    [GeneratedRegex("Credits\\s+Earned\\s+(?<credits>[\\d,]+)")]
    private static partial Regex CreditsExtractorRegex();
}