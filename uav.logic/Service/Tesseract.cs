using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace uav.logic.Service;

public class Tesseract
{
    public static async Task<string> RunTesseract(string file, string sharpen = null)
    {
        byte[] data;
        if (file.StartsWith("http"))
            data = await Download(file);
        else
            data = await Load(file);

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

    public static async Task<string> RunTesseract(byte[] data, string sharpen = null)
    {
        sharpen = sharpen == null ? string.Empty : $" -sharpen {sharpen}";
        await RunBinarySubprocess("/usr/bin/convert", $"- -alpha off{sharpen} -negate -", data, async (r) => {
            data = await ReadAllBytesAsync(r.BaseStream);
        });

        string results = null;
        await RunBinarySubprocess("/usr/bin/tesseract", "- -", data, async (r) => {
            results = await r.ReadToEndAsync();
        });

        return results;
    }

    private static async Task RunBinarySubprocess(string filename, string arguments, byte[] data, Func<StreamReader, Task> handleOutput, Func<StreamReader, Task> handleError = null)
    {
        handleError ??= (reader) => reader.ReadToEndAsync();

        var subproc = new Process();
        subproc.StartInfo.FileName = filename;
        subproc.StartInfo.Arguments = arguments;
        subproc.StartInfo.RedirectStandardError = true;
        subproc.StartInfo.RedirectStandardInput = true;
        subproc.StartInfo.RedirectStandardOutput = true;
        subproc.StartInfo.UseShellExecute = false;
        subproc.Start();

        var writing = subproc.StandardInput.BaseStream.WriteAsync(data);
        var reading = handleOutput(subproc.StandardOutput);
        var error = handleError(subproc.StandardError);

        await writing;
        subproc.StandardInput.Close();
        await subproc.WaitForExitAsync();
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