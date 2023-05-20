using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace uav.logic.Service;

public class ExtractPlayerId
{
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

    public async Task<string?> Extract(string file)
    {
        foreach (var sharpen in sharpens)
        {
            var text = await Tesseract.RunTesseract(file, sharpen);

            var m = playerIdExtractor.Match(text ?? string.Empty);
            if (m.Success)
            {
                var id = m.Groups["id"].Value;
                // unfortunately, tesseract guesses some letters wrong, so....
                var fixedId = tesseractCorrectionRegex.Replace(id, m => tesseractCorrections[m.Value]);
                // return only the first 16 characters
                return fixedId.Substring(0, 16);
            }
        }
        return null;
    }
}