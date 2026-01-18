using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using log4net;

namespace uav.logic.Service;

public class Tesseract
{
    private byte[]? data;
    private readonly string file;
    private static readonly ILog logger = LogManager.GetLogger(typeof(Tesseract));

    public Tesseract(string file)
    {
        this.file = file;
    }

    private string? _tesseractExtraParameters = null;
    public string TesseractExtraParameters
    {
        set
        {
            _tesseractExtraParameters = value;
        }
    }

    public async Task<string?> RunTesseract(string? sharpen = null)
    {
        if (data == null)
        {
            if (file.StartsWith("http"))
                data = await Download(file);
            else
                data = await Load(file);
        }

        return await RunTesseract(data, sharpen);
    }

    private static async Task<byte[]> Download(string url)
    {
        using var client = new HttpClient();
        var data = await client.GetByteArrayAsync(url);

        return data;
    }
    
    private static async Task<byte[]> Load(string file)
    {
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        return await ReadAllBytesAsync(fs);
    }

    public async Task<string?> RunTesseract(byte[] data, string? sharpen = null)
    {
        sharpen = sharpen == null ? string.Empty : $" -sharpen {sharpen}";
        var convertCmd = "/usr/bin/convert";
        var convertArgs = $"- -set colorspace Gray -separate -average{sharpen} -";
        logger.Debug($"Running convert command: {convertCmd} {convertArgs}");
        await RunBinarySubprocess(convertCmd, convertArgs, data, async (r) =>
        {
            // was -alpha off{sharpen} -negate
            data = await ReadAllBytesAsync(r.BaseStream);
        });

        string? results = null;
        var tesseractCmd = "/usr/bin/tesseract";
        var tesseractArgs = $"{_tesseractExtraParameters} - -";
        logger.Debug($"Running tesseract command: {tesseractCmd} {tesseractArgs}");
        await RunBinarySubprocess(tesseractCmd, tesseractArgs, data, async (r) =>
        {
            results = await r.ReadToEndAsync();
        });

        return results;
    }

    private static async Task RunBinarySubprocess(string filename, string arguments, byte[] data, Func<StreamReader, Task> handleOutput, Func<StreamReader, Task>? handleError = null)
    {
        handleError ??= (reader) => reader.ReadToEndAsync();

        var subProc = new Process();
        subProc.StartInfo.FileName = filename;
        subProc.StartInfo.Arguments = arguments;
        subProc.StartInfo.RedirectStandardError = true;
        subProc.StartInfo.RedirectStandardInput = true;
        subProc.StartInfo.RedirectStandardOutput = true;
        subProc.StartInfo.UseShellExecute = false;
        subProc.Start();

        var writing = subProc.StandardInput.BaseStream.WriteAsync(data);
        var reading = handleOutput(subProc.StandardOutput);
        var error = handleError(subProc.StandardError);

        await writing;
        subProc.StandardInput.Close();
        await subProc.WaitForExitAsync();
        await Task.WhenAll(reading, error);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream s)
    {
        var chunks = new List<(byte[] data, int length)>();
        var buffer = new byte[32767];
        int chunkSize;
        while ( (chunkSize = await s.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            chunks.Add((buffer, chunkSize));
            buffer = new byte[32767];
        }

        var data = new byte[chunks.Sum(c => c.length)];
        var offset = 0;
        foreach(var chunk in chunks)
        {
            // use the chunk size because it could be less than the buffer size, we don't want all of it.
            Array.Copy(chunk.data, 0, data, offset, chunk.length);
            offset += chunk.length;
        }

        return data;
    }

}