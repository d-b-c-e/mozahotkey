using BarRaider.SdTools;

namespace MozaStreamDeck.Plugin;

class Program
{
    static void Main(string[] args)
    {
        // Connect to Stream Deck using StreamDeck-Tools
        SDWrapper.Run(args);
    }
}
