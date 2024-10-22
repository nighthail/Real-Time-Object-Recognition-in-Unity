using System.Collections.Generic;


public class FiveStarDictionary
{
    // A property to expose the dictionary
    public Dictionary<string, string> WordPairs { get; private set; }

    public FiveStarDictionary()
    {
        // Initialize the dictionary
        WordPairs = new Dictionary<string, string>()
        {
            {"Coffee", "Kaffe"},
            {"Star", "Stjärna"},
            {"Mitten", "Vante"},
            {"Mug", "Temugg"},
            {"Koala", "Koala"},
            {"Berry", "Bär"},
            {"Bear", "Björn"},
            {"Loud Speaker", "Högtalare"},
            {"Space", "Rymd"},
            {"Trash bin", "Soptunna"},
            {"Thief", "Tjuv"},
            {"Chair", "Stol"},
            {"Tree", "Träd"},
            {"Sky", "Himmel"},
            {"Light", "Ljus"},
            {"Water", "Vatten"},
            {"Fire", "Eld"},
            {"Sun", "Sol"},
            {"Moon", "Måne"},
            {"Mountain", "Berg"},
            {"River", "Flod"},
            {"Road", "Väg"},
            {"Friend", "Vän"},
            {"Game", "Spel"},
            {"School", "Skola"},
            {"Food", "Mat"},
        };
    }
}
