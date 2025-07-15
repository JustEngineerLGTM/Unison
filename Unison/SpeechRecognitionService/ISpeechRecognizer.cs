using System.Threading.Tasks;
namespace Unison.SpeechRecognitionService;

public interface ISpeechRecognizer
{
    Task RecognitionStart();
    Task RecognitionStop();
}