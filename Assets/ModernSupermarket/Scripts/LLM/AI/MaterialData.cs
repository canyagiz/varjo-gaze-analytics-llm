using UnityEngine;

[System.Serializable]
public class MaterialData
{
    public string objectName;
    public float surfaceArea;
    public float volume;

    // Creates a MaterialData instance from a given Renderer
    public static MaterialData FromRenderer(Renderer renderer)
    {
        var data = new MaterialData();

        if (renderer != null)
        {
            // Use GameObject name as identifier
            data.objectName = renderer.gameObject.name;

            Bounds bounds = renderer.bounds;
            Vector3 size = bounds.size;

            // Approximate surface area using bounding box
            data.surfaceArea = 2 * (size.x * size.y + size.x * size.z + size.y * size.z);

            // Approximate volume using bounding box
            data.volume = size.x * size.y * size.z;
        }
        else
        {
            data.objectName = "Unknown";
            data.surfaceArea = 0f;
            data.volume = 0f;
        }

        return data;
    }

    // Converts the material data to a simple string for LLM prompts
    public override string ToString()
    {
        return
            $"- Object Name: {objectName}\n" +
            $"- Surface Area: {surfaceArea:F2} m²\n" +
            $"- Volume: {volume:F3} m³";
    }
}
