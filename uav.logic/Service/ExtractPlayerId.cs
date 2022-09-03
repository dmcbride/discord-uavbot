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
    private string[] sharpens = new[] {
        null,
        "0x4",
        "0x8",
    };
    public async Task<string> Extract(string file)
    {
        foreach (var sharpen in sharpens)
        {
            var text = await Tesseract.RunTesseract(file, sharpen);

            var m = playerIdExtractor.Match(text);
            if (m.Success)
            {
                var id = m.Groups["id"].Value;
                // unfortunately, tesseract guesses O when it should be 0, so...
                return id.Replace('O', '0');
            }
        }
        return null;
    }
}