using System.Collections.Generic;
using System.IO;
namespace Unison.SpeechRecognitionService;

public interface IAudioInputProvider
{
    Stream StartDefaultDevice();         
    void Stop();                         
    List<string> GetAvailableDevices();  
}
