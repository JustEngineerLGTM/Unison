// Unison.SpeechRecognitionService/ModelService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unison.ViewModels;

public class ModelService : IModelService
{
    private readonly string _basePath = "models";
    private const string VoskModelsUrl = "https://alphacephei.com/vosk/models/model-list.json";
    private static readonly HttpClient HttpClient = new(); 

    public async Task<List<VoskModel>?> GetAvailableModelsAsync()
    {
        var json = await HttpClient.GetStringAsync(VoskModelsUrl);
        return JsonSerializer.Deserialize<List<VoskModel>>(json);
    }

    public async Task<VoskModel?> GetAvailableModelByNameAsync(string modelName)
    {
        var models = await GetAvailableModelsAsync();
        return models?.FirstOrDefault(model => model.Name == modelName);
    }

   
    public async Task<bool> IsModelDownloadedAsync(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName)) return false;

        var extractedModelPath = Path.Combine(_basePath, modelName);
        var zipPath = Path.Combine(_basePath, modelName + ".zip");
        

        if (!Directory.Exists(extractedModelPath))
        {
            return false;
        }
        
        if (!File.Exists(zipPath))
        {
            return true; 
        }

        var modelInfo = await GetAvailableModelByNameAsync(modelName);
        if (modelInfo?.Md5 is null) return false;

        using var stream = File.OpenRead(zipPath);
        var hashBytes = await MD5.HashDataAsync(stream);
     
        var calculatedMd5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return calculatedMd5 == modelInfo.Md5;
    }

    public async Task DownloadModelAsync(VoskModel model, IProgress<int> progress)
    {
        var dir = Path.Combine(_basePath, model.Name);
        Directory.CreateDirectory(dir);

        var zipPath = Path.Combine(_basePath, model.Name + ".zip");

        using (var resp = await HttpClient.GetAsync(model.Url, HttpCompletionOption.ResponseHeadersRead))
        {
            resp.EnsureSuccessStatusCode();
            
            var totalBytes = resp.Content.Headers.ContentLength ?? 0;
            await using (var contentStream = await resp.Content.ReadAsStreamAsync())
            {
                await using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    var buffer = new byte[81920];
                    long totalBytesRead = 0;
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        if (totalBytes > 0)
                        {
                            progress.Report((int)(100 * totalBytesRead / totalBytes));
                        }
                    }
                } 
            }
        }
        await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, dir, true));
        progress.Report(100);
    }
}