using UnityEngine;

public static class BallisticUtils
{
    // Calcule la position d'impact et le temps de vol pour un tir balistique (mortier)
    // Retourne true si une solution existe, false sinon
    public static bool ComputeImpact(Vector3 start, Vector3 target, float initialSpeed, out Vector3 impactPoint, out float flightTime)
    {
        impactPoint = Vector3.zero;
        flightTime = 0f;
        Vector3 delta = target - start;
        float g = Mathf.Abs(Physics.gravity.y);
        float dxz = new Vector2(delta.x, delta.z).magnitude;
        float dy = delta.y;
        float v2 = initialSpeed * initialSpeed;
        float root = v2 * v2 - g * (g * dxz * dxz + 2 * dy * v2);
        if (root < 0f) return false; // pas de solution
        float sqrt = Mathf.Sqrt(root);
        float t1 = (v2 + sqrt) / (g * dxz);
        float t2 = (v2 - sqrt) / (g * dxz);
        float t = Mathf.Max(t1, t2);
        if (t <= 0f) return false;
        flightTime = dxz / (initialSpeed * Mathf.Cos(Mathf.Atan2(dy, dxz)));
        impactPoint = target; // approximation : on vise la cible
        return true;
    }
}
