using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace uav.logic.Service;

public class ExtractPlayerId : TesseractExtractor<string>
{
  private readonly ILog logger = LogManager.GetLogger(typeof(ExtractPlayerId));
  public ExtractPlayerId()
  {

  }

  private readonly Regex playerIdExtractor = new(@"Player\s*ID:\s*(?<id>\S{15,16})", RegexOptions.Singleline);

  private static readonly Dictionary<string, string> tesseractCorrections = new()
  {
    ["O"] = "0",
    ["S"] = "5",
    ["£"] = "E",
    ["€"] = "E",
    ["I"] = "1",
    ["T"] = "1",
  };
  private static readonly Regex tesseractCorrectionRegex = new($"[{string.Join("", tesseractCorrections.Keys)}]");

  protected override string? FindValues(string text)
  {
    var m = playerIdExtractor.Match(text ?? string.Empty);
    logger.Debug(text);
    if (m.Success)
    {
      logger.Debug($"Found player ID: {m.Groups["id"].Value}");
      var id = m.Groups["id"].Value;
      // unfortunately, tesseract guesses some letters wrong, so....
      var fixedId = tesseractCorrectionRegex.Replace(id, m => tesseractCorrections[m.Value]);

      // can be 15-16 characters, chop off anything beyond that.
      fixedId = fixedId.Length > 16 ? fixedId[..16] : fixedId;

      // the player ID is only made up of hex characters, so if it has anything that doesn't match that, toast.
      var invalidCharacters = fixedId.Where(c => !char.IsAsciiHexDigitUpper(c));
      if (invalidCharacters.Any())
      {
        logger.Debug($"Player ID {fixedId} contains invalid characters: {string.Join(", ",invalidCharacters.Select(c => $"'{c}'"))}.");
        return null;
      }

      return fixedId;
    }
    return null;
  }
}