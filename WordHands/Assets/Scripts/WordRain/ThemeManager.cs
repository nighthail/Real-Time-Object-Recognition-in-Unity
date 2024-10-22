using System.IO;
using UnityEngine;
using TMPro;
public class ThemeManager : MonoBehaviour
{
    [Header("Word Files")]
    public string goodWordsFileTheme1 = "Assets/Scripts/WordRain/WordList/RightWords_Havet.txt";
    public string badWordsFileTheme1 = "Assets/Scripts/WordRain/WordList/WrongWords.txt";

    public string goodWordsFileTheme2 = "Assets/Scripts/WordRain/WordList/RightWords_Havet.txt";
    public string badWordsFileTheme2 = "Assets/Scripts/WordRain/WordList/WrongWords.txt";

    public string goodWordsFileTheme3 = "Assets/Scripts/WordRain/WordList/RightWords_Havet.txt";
    public string badWordsFileTheme3 = "Assets/Scripts/WordRain/WordList/WrongWords.txt";

    public TMP_Text themeWord;
    private string[] goodWords;
    private string[] badWords;

    public void Start()
    {
        PlayerPrefs.DeleteAll();
        // Pick a random theme at the start
        int themeIndex = Random.Range(1, 4); // Choose between 1, 2, and 3

        LoadTheme(themeIndex);
    }

    public void LoadTheme(int themeIndex)
    {
        // Load word lists based on the selected theme
        switch (themeIndex)
        {
            case 1:
                goodWords = LoadWordsFromFile(goodWordsFileTheme1);
                badWords = LoadWordsFromFile(badWordsFileTheme1);
                themeWord.text = "Havet";
                break;
            case 2:
                goodWords = LoadWordsFromFile(goodWordsFileTheme2);
                badWords = LoadWordsFromFile(badWordsFileTheme2);
                themeWord.text = "Havet 2";
                break;
            case 3:
                goodWords = LoadWordsFromFile(goodWordsFileTheme3);
                badWords = LoadWordsFromFile(badWordsFileTheme3);
                themeWord.text = "Havet 3";
                break;
            default:
                Debug.LogError("Invalid theme index selected.");
                break;
        }
    }

    // Method to load words from a file
    private string[] LoadWordsFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllLines(filePath); // Read all lines and return them as an array
        }
        else
        {
            Debug.LogError("Word list file not found: " + filePath);
            return new string[0]; // Return an empty array if the file is missing
        }
    }

    // Methods to provide the word lists
    public string[] GetGoodWords()
    {
        return goodWords;
    }

    public string[] GetBadWords()
    {
        return badWords;
    }
}
