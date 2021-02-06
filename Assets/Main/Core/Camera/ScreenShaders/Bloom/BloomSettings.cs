using MPGUI;
using UnityEngine;
using UnityEngine.UI;

public class BloomSettings : ScriptableObject
{
    public bool enableShader = true;
    public int iterations = 8;
    public float filmGrainScale = 1;
    public Vector3 grainSeed = new Vector3(25.36535f, 13.12572f, 96.23642f);

    public DropdownSpawn dropdownType;

    public void DropdownIterations(RectTransform button)
    {
        GUIButtonSet set = dropdownType.SpawnDropdown(button);
        int[] values = new int[] { 1, 2, 4, 8, 16 };

        foreach (int i in values)
        {
            int v = i;
            Image image = set.AddButton(i.ToString(), () => iterations = v)
                .GetComponent<Image>();

            if (image && iterations != i)
                image.color *= 0.5f;
        }
    }

    public void DropdownEnabled(RectTransform button)
    {
        GUIButtonSet set = dropdownType.SpawnDropdown(button);
        Image image;

        image = set.AddButton("Enabled", () => enableShader = true)
            .GetComponent<Image>();

        if (!enableShader)
            image.color *= 0.5f;

        image = set.AddButton("Disabled", () => enableShader = false)
            .GetComponent<Image>();

        if (enableShader)
            image.color *= 0.5f;
    }
}
