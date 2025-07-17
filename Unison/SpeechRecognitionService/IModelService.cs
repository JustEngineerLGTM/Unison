// Unison.SpeechRecognitionService/IModelService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unison.ViewModels;

public interface IModelService
{
    Task<List<VoskModel>?> GetAvailableModelsAsync();
    Task<VoskModel?> GetAvailableModelByNameAsync(string modelName);
    Task<bool> IsModelDownloadedAsync(string modelName); 
    Task DownloadModelAsync(VoskModel model, IProgress<int> progress);
}