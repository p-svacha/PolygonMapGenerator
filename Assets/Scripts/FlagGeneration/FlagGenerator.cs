using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FlagGenerator : MonoBehaviour
{

    public Image Flag;

    public void GenerateFlag()
    {
        string newFilePath = Application.dataPath + "/Resources/Flags/flag.png";
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = Application.dataPath + "/../RelatedProjects/FlagGeneration/bin/Debug/FlagGeneration.exe";
        startInfo.Arguments = newFilePath;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        process.StartInfo = startInfo;
        process.Start();

        Sprite flagSprite = LoadNewSprite(newFilePath);
        Flag.sprite = flagSprite;
    }

    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

}
