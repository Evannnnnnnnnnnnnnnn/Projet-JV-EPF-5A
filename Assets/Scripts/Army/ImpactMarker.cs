using UnityEngine;

public class ImpactMarker : MonoBehaviour
{
    [SerializeField] private Color m_MarkerColor = Color.green;
    [SerializeField] private float m_MarkerSize = 0.5f;
    [SerializeField] private float m_HeightOffset = 0.05f; // pour éviter qu’elle passe sous la map

    void Start()
    {
        // Créer une sphère si elle n'existe pas encore
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Utiliser la sphère par défaut de Unity
        filter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

        // Créer un matériau
        Material material = new Material(Shader.Find("Standard"));
        material.color = m_MarkerColor;
        renderer.material = material;

        // Ajuster la taille (aplatie)
        transform.localScale = new Vector3(m_MarkerSize, 0.05f * m_MarkerSize, m_MarkerSize);

        // S'assurer qu’elle est légèrement au-dessus du sol
        transform.position += Vector3.up * m_HeightOffset;
    }
}
