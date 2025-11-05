using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class DangerMapManager : MonoBehaviour
{
    [Header("Danger Map Settings")]
    public int textureResolution = 256;
    public float worldSize = 100f;
    public Gradient dangerGradient; // Vert à rouge
    public Material overlayMaterial; // Matériau à appliquer sur le sol
    public float fadeSpeed = 1f; // vitesse de disparition des zones

    private Texture2D dangerTexture;
    private float[,] dangerValues;
    private List<DangerZone> activeZones = new List<DangerZone>();

    void Awake()
    {
        dangerTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        dangerValues = new float[textureResolution, textureResolution];
        ClearDangerMap();
        ApplyTexture();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        // Met à jour les zones actives
        for (int i = activeZones.Count - 1; i >= 0; i--)
        {
            activeZones[i].Update(dt * fadeSpeed);
            if (!activeZones[i].IsAlive)
                activeZones.RemoveAt(i);
        }
        // Met à jour la texture
        UpdateDangerMapFromZones();
    }

    public void ClearDangerMap()
    {
        for (int x = 0; x < textureResolution; x++)
            for (int y = 0; y < textureResolution; y++)
                dangerValues[x, y] = 0f;
    }

    public void AddDangerZone(Vector3 worldPos, float radius, float intensity, float duration)
    {
        activeZones.Add(new DangerZone(worldPos, radius, intensity, duration));
    }

    private void UpdateDangerMapFromZones()
    {
        ClearDangerMap();
        foreach (var zone in activeZones)
            AddDangerZoneToMap(zone.position, zone.radius, zone.CurrentIntensity);
        ApplyTexture();
    }

    private void AddDangerZoneToMap(Vector3 worldPos, float radius, float intensity)
    {
        int centerX = Mathf.RoundToInt((worldPos.x / worldSize + 0.5f) * textureResolution);
        int centerY = Mathf.RoundToInt((worldPos.z / worldSize + 0.5f) * textureResolution);
        int rad = Mathf.RoundToInt(radius / worldSize * textureResolution);
        for (int x = centerX - rad; x <= centerX + rad; x++)
        {
            for (int y = centerY - rad; y <= centerY + rad; y++)
            {
                if (x < 0 || y < 0 || x >= textureResolution || y >= textureResolution) continue;
                float dx = (x - centerX) / (float)rad;
                float dy = (y - centerY) / (float)rad;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > 1f) continue;
                float value = Mathf.Lerp(intensity, 0f, dist); // dégradé radial
                dangerValues[x, y] = Mathf.Clamp01(dangerValues[x, y] + value);
            }
        }
    }

    public void ApplyTexture()
    {
        for (int x = 0; x < textureResolution; x++)
        {
            for (int y = 0; y < textureResolution; y++)
            {
                Color c = dangerGradient.Evaluate(dangerValues[x, y]);
                dangerTexture.SetPixel(x, y, c);
            }
        }
        dangerTexture.Apply();
        if (overlayMaterial != null)
            overlayMaterial.mainTexture = dangerTexture;
    }

    // Pour ajout externe (ex: lors d'un tir de mortier)
    public void RegisterMortarShot(Vector3 impactPos, float radius, float intensity, float duration)
    {
        AddDangerZone(impactPos, radius, intensity, duration);
    }
}
