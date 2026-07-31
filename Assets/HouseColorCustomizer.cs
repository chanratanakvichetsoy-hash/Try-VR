using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A reusable color-customization system for house/room objects
/// (bed, floor, pillow, wall, bathroom floor, table, etc.) and lights.
///
/// HOW IT WORKS:
/// 1. You register every customizable object in the "items" list in the Inspector.
/// 2. UI buttons call SelectItem(index) to choose WHICH object you're editing.
/// 3. UI color-swatch buttons call ApplyColor(color) to paint the currently
///    selected item.
/// 4. Colors are applied via MaterialPropertyBlock, which changes the color
///    WITHOUT creating a new Material instance (better performance, no memory
///    leaks, and it won't break material batching/instancing).
/// 5. Choices are optionally saved to PlayerPrefs so they persist next launch.
/// </summary>
public class HouseColorCustomizer : MonoBehaviour
{
    [System.Serializable]
    public class CustomizableItem
    {
        [Tooltip("Friendly name shown in UI, e.g. 'Bed', 'Wall', 'Bathroom Floor'")]
        public string itemName;

        [Tooltip("Assign this if the object uses a Renderer/Material (bed, floor, wall, table, pillow...)")]
        public Renderer targetRenderer;

        [Tooltip("Which material slot on the renderer to recolor (usually 0)")]
        public int materialIndex = 0;

        [Tooltip("Assign this INSTEAD of a renderer if this item is a Light")]
        public Light targetLight;

        // Internal cache, not shown in inspector
        [System.NonSerialized] public MaterialPropertyBlock propBlock;
    }

    [Header("Register every object you want to be colorable")]
    public List<CustomizableItem> items = new List<CustomizableItem>();

    [Header("Palette shown to the player (optional, for reference)")]
    public Color[] palette = new Color[]
    {
        Color.white, Color.black, Color.red, Color.green,
        Color.blue, Color.yellow, new Color(0.6f, 0.4f, 0.2f) // brown
    };

    [Header("Persistence")]
    [Tooltip("If true, colors are saved/loaded automatically using PlayerPrefs")]
    public bool saveColors = true;
    [Tooltip("Unique key prefix so multiple scenes/rooms don't overwrite each other's saves")]
    public string saveKeyPrefix = "HouseColor_";

    private int currentIndex = -1;

    private void Awake()
    {
        // Prepare a MaterialPropertyBlock for each renderer-based item
        foreach (var item in items)
        {
            if (item.targetRenderer != null)
                item.propBlock = new MaterialPropertyBlock();
        }
    }

    private void Start()
    {
        if (saveColors)
            LoadAllColors();
    }

    /// <summary>Call this from a UI button (e.g. "Select Bed") passing its index in the list.</summary>
    public void SelectItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.LogWarning($"HouseColorCustomizer: index {index} out of range.");
            return;
        }
        currentIndex = index;
    }

    /// <summary>Call this from a UI color swatch button, passing the swatch's color.</summary>
    public void ApplyColor(Color color)
    {
        if (currentIndex < 0)
        {
            Debug.LogWarning("HouseColorCustomizer: no item selected. Call SelectItem() first.");
            return;
        }
        ApplyColorToItem(items[currentIndex], color);

        if (saveColors)
            SaveColor(items[currentIndex].itemName, color);
    }

    /// <summary>Directly set a color on a named item without needing SelectItem first.</summary>
    public void ApplyColorToItemByName(string itemName, Color color)
    {
        var item = items.Find(i => i.itemName == itemName);
        if (item == null)
        {
            Debug.LogWarning($"HouseColorCustomizer: no item named '{itemName}'.");
            return;
        }
        ApplyColorToItem(item, color);
        if (saveColors)
            SaveColor(itemName, color);
    }

    private void ApplyColorToItem(CustomizableItem item, Color color)
    {
        if (item.targetRenderer != null)
        {
            item.targetRenderer.GetPropertyBlock(item.propBlock, item.materialIndex);

            // Try URP/HDRP property name first, fall back to Built-in RP name.
            Material mat = item.targetRenderer.sharedMaterials[item.materialIndex];
            if (mat.HasProperty("_BaseColor"))
                item.propBlock.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color"))
                item.propBlock.SetColor("_Color", color);

            item.targetRenderer.SetPropertyBlock(item.propBlock, item.materialIndex);
        }
        else if (item.targetLight != null)
        {
            item.targetLight.color = color;
        }
        else
        {
            Debug.LogWarning($"HouseColorCustomizer: item '{item.itemName}' has no renderer or light assigned.");
        }
    }

    // ---------- Saving / Loading ----------

    private void SaveColor(string itemName, Color color)
    {
        string key = saveKeyPrefix + itemName;
        PlayerPrefs.SetFloat(key + "_r", color.r);
        PlayerPrefs.SetFloat(key + "_g", color.g);
        PlayerPrefs.SetFloat(key + "_b", color.b);
        PlayerPrefs.SetFloat(key + "_a", color.a);
        PlayerPrefs.Save();
    }

    private void LoadAllColors()
    {
        foreach (var item in items)
        {
            string key = saveKeyPrefix + item.itemName;
            if (!PlayerPrefs.HasKey(key + "_r"))
                continue; // no saved color for this item yet

            Color saved = new Color(
                PlayerPrefs.GetFloat(key + "_r"),
                PlayerPrefs.GetFloat(key + "_g"),
                PlayerPrefs.GetFloat(key + "_b"),
                PlayerPrefs.GetFloat(key + "_a", 1f)
            );
            ApplyColorToItem(item, saved);
        }
    }
}
