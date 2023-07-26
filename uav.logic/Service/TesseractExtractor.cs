namespace uav.logic.Service;

public abstract class TesseractExtractor<T> where T : class
{
  public TesseractExtractor()
  {
  }

  private string?[] sharpens = new[] {
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

      var values = FindValues(text ?? string.Empty);
      if (values != null)
      {
        return values;
      }
    }
    return null;
  }
}