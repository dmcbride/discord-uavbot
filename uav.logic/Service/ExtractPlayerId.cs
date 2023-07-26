using log4net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace uav.logic.Service;

public class ExtractPlayerId : TesseractExtractor<string>
{
    private ILog logger = LogManager.GetLogger(typeof(ExtractPlayerId));
    public ExtractPlayerId()
    {
        
    }

    private Regex playerIdExtractor = new Regex(@"Player\s*ID:\s*(?<id>\S+)", RegexOptions.Singleline);
    private string?[] sharpens = new[] {
        null,
        "0x4",
        "0x8",
    };

    private static Dictionary<string, string> tesseractCorrections = new Dictionary<string, string> {
        ["O"] = "0",
        ["S"] = "5",
        ["£"] = "E"
    };
    private static Regex tesseractCorrectionRegex = new Regex($"[{string.Join("", tesseractCorrections.Keys)}]");

  protected override string? FindValues(string text)
  {
    var m = playerIdExtractor.Match(text ?? string.Empty);
    logger.Debug(text);
    if (m.Success)
    {
        var id = m.Groups["id"].Value;
        // unfortunately, tesseract guesses some letters wrong, so....
        var fixedId = tesseractCorrectionRegex.Replace(id, m => tesseractCorrections[m.Value]);
        // return only the first 16 characters
        return fixedId.Substring(0, 16);
    }
    return null;
  }
}