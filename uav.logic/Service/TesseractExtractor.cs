using System.Reflection;
using log4net;

namespace uav.logic.Service;

public abstract class TesseractExtractor<T>
{
  private ILog? logger = MethodBase.GetCurrentMethod()?.DeclaringType == null ? null : LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
  public TesseractExtractor()
  {
  }

  private readonly string?[] sharpens = new[] {
    null,
    "0x4",
    "0x8",
  };

  protected abstract T? FindValues(string text);

  public async Task<T?> Extract(string file)
  {
    var tesseract = new Tesseract(file);
    foreach (var sharpen in sharpens)
    {
      var text = await tesseract.RunTesseract(sharpen);

      if (text == null)
      {
        continue;
      }

      var values = FindValues(text);
      if (values != null)
      {
        return values;
      }
    }
    return default;
  }
}