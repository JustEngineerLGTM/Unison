namespace Unison.SpeechRecognitionService;
using System.Text.Json;
public class VoskParserUtility
{

public static class VoskResultParser
{
    public static string? Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("text", out var textElement))
                return textElement.GetString();
        }
        catch
        {
            
        }

        return string.Empty;
    }
}

}