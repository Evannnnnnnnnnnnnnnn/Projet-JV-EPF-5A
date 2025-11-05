using UnityEngine;

public class DangerZone
{
    public Vector3 position;
    public float radius;
    public float intensity;
    public float timeToLive;
    public float maxTime;

    public DangerZone(Vector3 pos, float rad, float inten, float ttl)
    {
        position = pos;
        radius = rad;
        intensity = inten;
        timeToLive = ttl;
        maxTime = ttl;
    }

    public float CurrentIntensity => intensity * Mathf.Clamp01(timeToLive / maxTime);

    public void Update(float deltaTime)
    {
        timeToLive -= deltaTime;
    }

    public bool IsAlive => timeToLive > 0f;
}
