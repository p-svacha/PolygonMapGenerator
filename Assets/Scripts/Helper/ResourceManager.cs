using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Class used to dynamically load materials from resources on demand. All materials are cached after the first load.
/// </summary>
public static class ResourceManager
{
    public static Color UiBackgroundDefault => new Color(0.082f, 0.094f, 0.114f);
    public static Color UiBackgroundLighter1 => new Color(0.165f, 0.169f, 0.176f);
    public static Color UiBackgroundLighter2 => new Color(0.251f, 0.255f, 0.263f);

    public static Color UiTextDefault => new Color (0.780f, 0.796f, 0.808f);
    public static Color UiTextRed => new Color(0.878f, 0.459f, 0.408f);
    public static Color UiTextGreen => new Color(0.47f, 0.83f, 0.45f);

    public static string WhiteTextColorHex = "#C7CBCE";
    public static string RedTextColorHex = "#E07568";

    private static Dictionary<string, Material> CachedMaterials = new Dictionary<string, Material>();
    public static Material LoadMaterial(string resourcePath)
    {
        // cached
        if (CachedMaterials.TryGetValue(resourcePath, out Material mat)) return mat;

        // not yet cached
        Material newMat = Resources.Load<Material>(resourcePath);
        if (newMat == null) throw new System.Exception($"Failed to load material {resourcePath}.");
        CachedMaterials.Add(resourcePath, newMat);
        return newMat;
    }

    private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();
    public static Texture2D LoadTexture(string resourcePath)
    {
        // cached
        if (CachedTextures.TryGetValue(resourcePath, out Texture2D tex)) return tex;

        // not yet cached
        Texture2D newTex = Resources.Load<Texture2D>(resourcePath);
        if (newTex == null) throw new System.Exception($"Failed to load texture {resourcePath}.");
        CachedTextures.Add(resourcePath, newTex);
        return newTex;
    }

    private static Dictionary<string, GameObject> CachedPrefabs = new Dictionary<string, GameObject>();
    public static GameObject LoadPrefab(string resourcePath)
    {
        // cached
        if (CachedPrefabs.TryGetValue(resourcePath, out GameObject obj)) return obj;

        // not yet cached
        GameObject loadedPrefab = Resources.Load<GameObject>(resourcePath);
        if (loadedPrefab == null) throw new System.Exception($"Failed to load GameObject {resourcePath}.");
        CachedPrefabs.Add(resourcePath, loadedPrefab);
        return loadedPrefab;
    }

    private static Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();
    public static Sprite LoadSprite(string resourcePath)
    {
        // cached
        if (CachedSprites.TryGetValue(resourcePath, out Sprite obj)) return obj;

        // not yet cached
        Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);
        if (loadedSprite == null) throw new System.Exception($"Failed to load Sprite {resourcePath}.");
        CachedSprites.Add(resourcePath, loadedSprite);
        return loadedSprite;
    }

    private static Dictionary<string, AudioClip> CachedAudioClips = new Dictionary<string, AudioClip>();
    public static AudioClip LoadAudioClip(string resourcePath)
    {
        // cached
        if (CachedAudioClips.TryGetValue(resourcePath, out AudioClip obj)) return obj;

        // not yet cached
        AudioClip loadedAudioClip = Resources.Load<AudioClip>(resourcePath);
        if (loadedAudioClip == null) throw new System.Exception($"Failed to load AudioClip {resourcePath}.");
        CachedAudioClips.Add(resourcePath, loadedAudioClip);
        return loadedAudioClip;
    }

    public static void ClearCache()
    {
        CachedMaterials.Clear();
        CachedTextures.Clear();
        CachedPrefabs.Clear();
        CachedSprites.Clear();
        CachedAudioClips.Clear();
    }
}

