using System;
using System.Threading.Tasks;
namespace Unison.SpeechRecognitionService;

public interface ISpeechRecognizer
{
    event Action<string> OnTextRecognized;
    Task RecognitionStart(string path);
    Task RecognitionStop();
}