using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AppIconSetup
{
    private const string IconPath = "Assets/AppIcon/DasunMoonstone_Icon.png";
    private const string CoverIconPath = "Assets/AppIcon/DasunMoonstone_BookCover_Icon.png";
    private const string TransparentPath = "Assets/AppIcon/DasunMoonstone_TransparentForeground.png";
    private const string DefaultNewProductName = "AR_PhysicalScience";

    static AppIconSetup()
    {
        EditorApplication.delayCall += OnEditorReady;
    }

    private static void OnEditorReady()
    {
        // Only run automatic setup if productName is still the default/old IT22177100 name or icons need initialization
        if (PlayerSettings.productName == "IT22177100_Research_Component" || PlayerSettings.productName.StartsWith("IT22177100"))
        {
            ApplyFullSetup(IconPath, DefaultNewProductName);
        }
    }

    [MenuItem("Tools/Dasun & Moonstone/Apply App Icon (Close-up Character & Moonstone)", false, 1)]
    public static void ApplyCloseupIcon()
    {
        ApplyFullSetup(IconPath, PlayerSettings.productName.Contains("IT22177100") ? DefaultNewProductName : PlayerSettings.productName);
        EditorUtility.DisplayDialog("App Icon Configured", 
            $"Successfully configured the App Icon using the close-up Dasun & Moonstone artwork!\n\nProduct Name: {PlayerSettings.productName}\n\nAndroid Adaptive Icon: Configured with Background & Transparent Foreground (Unity logo removed).", "OK");
    }

    [MenuItem("Tools/Dasun & Moonstone/Apply App Icon (Full Book Cover Style)", false, 2)]
    public static void ApplyBookCoverIcon()
    {
        ApplyFullSetup(CoverIconPath, PlayerSettings.productName.Contains("IT22177100") ? DefaultNewProductName : PlayerSettings.productName);
        EditorUtility.DisplayDialog("App Icon Configured", 
            $"Successfully configured the App Icon using the full book cover style artwork!\n\nProduct Name: {PlayerSettings.productName}\n\nAndroid Adaptive Icon: Configured with Background & Transparent Foreground (Unity logo removed).", "OK");
    }

    [MenuItem("Tools/Dasun & Moonstone/Set App Name/AR_PhysicalScience (Default)", false, 19)]
    public static void SetNameARPhysicalScience()
    {
        SetProductName("AR_PhysicalScience");
    }

    [MenuItem("Tools/Dasun & Moonstone/Set App Name/Dasun & Moonstone", false, 20)]
    public static void SetNameShort()
    {
        SetProductName("Dasun & Moonstone");
    }

    [MenuItem("Tools/Dasun & Moonstone/Set App Name/Dasun and the Moonstone", false, 21)]
    public static void SetNameFull()
    {
        SetProductName("Dasun and the Moonstone");
    }

    [MenuItem("Tools/Dasun & Moonstone/Set App Name/Dasun&Moonstone (No Spaces)", false, 22)]
    public static void SetNameNoSpaces()
    {
        SetProductName("Dasun&Moonstone");
    }

    public static void SetProductName(string newName)
    {
        PlayerSettings.productName = newName;
        AssetDatabase.SaveAssets();
        Debug.Log($"[AppIconSetup] PlayerSettings.productName set to: '{newName}'");
    }

    public static void ApplyFullSetup(string texturePath, string newProductName)
    {
        try
        {
            // 1. Ensure texture exists and is imported properly
            if (!File.Exists(texturePath))
            {
                Debug.LogWarning($"[AppIconSetup] Texture file not found at: {texturePath}");
                return;
            }

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            ConfigureTextureImporter(texturePath);

            var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (iconTexture == null)
            {
                Debug.LogError($"[AppIconSetup] Failed to load Texture2D at: {texturePath}");
                return;
            }

            // Also ensure the transparent foreground texture is imported
            Texture2D transparentTexture = null;
            if (File.Exists(TransparentPath))
            {
                AssetDatabase.ImportAsset(TransparentPath, ImportAssetOptions.ForceUpdate);
                ConfigureTextureImporter(TransparentPath);
                transparentTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TransparentPath);
            }

            // 2. Set Product Name if still the old name
            if (PlayerSettings.productName.StartsWith("IT22177100") || !string.IsNullOrEmpty(newProductName))
            {
                PlayerSettings.productName = newProductName;
                Debug.Log($"[AppIconSetup] Updated PlayerSettings.productName to: '{PlayerSettings.productName}'");
            }

            // 3. Set Default Icons (BuildTargetGroup.Unknown applies to all platforms by default)
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { iconTexture });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { iconTexture });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new[] { iconTexture });

            // 4. Configure Android Platform Icons (Adaptive and Legacy)
            ConfigureAndroidPlatformIcons(iconTexture, transparentTexture);

            AssetDatabase.SaveAssets();
            Debug.Log($"[AppIconSetup] Successfully set App Icon to '{texturePath}' and Product Name to '{PlayerSettings.productName}'.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AppIconSetup] Error applying setup: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void ConfigureTextureImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.textureType != TextureImporterType.Default)
            {
                importer.textureType = TextureImporterType.Default;
                modified = true;
            }
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                modified = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                modified = true;
            }
            if (importer.npotScale != TextureImporterNPOTScale.None)
            {
                importer.npotScale = TextureImporterNPOTScale.None;
                modified = true;
            }
            if (importer.maxTextureSize < 1024)
            {
                importer.maxTextureSize = 1024;
                modified = true;
            }

            if (modified)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }

    private static void ConfigureAndroidPlatformIcons(Texture2D iconTexture, Texture2D transparentTexture)
    {
        try
        {
            var androidTarget = UnityEditor.Build.NamedBuildTarget.Android;

            // Legacy icons (single layer)
            var platformIcons = PlayerSettings.GetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Legacy);
            if (platformIcons != null && platformIcons.Length > 0)
            {
                for (int i = 0; i < platformIcons.Length; i++)
                {
                    platformIcons[i].SetTexture(iconTexture);
                }
                PlayerSettings.SetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Legacy, platformIcons);
                Debug.Log("[AppIconSetup] Assigned Android Legacy Platform Icons.");
            }

            // Round icons (single layer)
            var roundIcons = PlayerSettings.GetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Round);
            if (roundIcons != null && roundIcons.Length > 0)
            {
                for (int i = 0; i < roundIcons.Length; i++)
                {
                    roundIcons[i].SetTexture(iconTexture);
                }
                PlayerSettings.SetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Round, roundIcons);
                Debug.Log("[AppIconSetup] Assigned Android Round Platform Icons.");
            }

            // Adaptive icons: Set Layer 0 = Background (Dasun), Layer 1 = Foreground (Transparent)
            // This prevents Unity from placing its fallback 3D cube logo on the foreground layer!
            var adaptiveIcons = PlayerSettings.GetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Adaptive);
            if (adaptiveIcons != null && adaptiveIcons.Length > 0)
            {
                for (int i = 0; i < adaptiveIcons.Length; i++)
                {
                    if (transparentTexture != null)
                    {
                        adaptiveIcons[i].SetTextures(new[] { iconTexture, transparentTexture });
                    }
                    else
                    {
                        adaptiveIcons[i].SetTexture(iconTexture);
                    }
                }
                PlayerSettings.SetPlatformIcons(androidTarget, UnityEditor.Android.AndroidPlatformIconKind.Adaptive, adaptiveIcons);
                Debug.Log("[AppIconSetup] Assigned Android Adaptive Platform Icons (Background + Foreground).");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"[AppIconSetup] Android platform icon configuration note: {ex.Message}");
        }
    }
}
