using UnityEngine;
using System.Linq;

// Ce script doit être placé sur un GameObject vide de la scène
public class selectTarget : MonoBehaviour
{
    // Instance statique pour un accès facile depuis n'importe quel script
    public static selectTarget Instance { get; private set; }

    // La cible actuelle que toutes les unités doivent attaquer
    [Header("Cible Actuelle")]
    [Tooltip("La cible unique que toutes les troupes alliées doivent attaquer.")]
    public Transform CurrentTarget;

    [Header("Configuration")]
    [Tooltip("Le Tag (étiquette) des ennemis à cibler (ex: 'GreenArmy').")]
    [SerializeField] private string m_EnemyTag = "GreenArmy";

    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de ce manager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        // Initialiser la première cible au démarrage
        SelectNewTarget();
    }

    private void Update()
    {
        // Vérifie si la cible actuelle est morte ou n'existe plus
        // Le cas le plus simple est de vérifier si la référence est nulle
        if (CurrentTarget == null)
        {
            SelectNewTarget();
        }
        // NOTE: Si le 'ArmyManager.ArmyElementHasBeenKilled' est appelé,
        // vous pourriez aussi appeler 'SelectNewTarget()' depuis la fonction Die() du Drone pour une réaction instantanée.
    }

    // Fonction pour trouver et définir une nouvelle cible
    public void SelectNewTarget()
    {
        // 1. Trouver tous les GameObjects avec le tag ennemi
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(m_EnemyTag);

        if (enemies.Length > 0)
        {
            // 2. LOGIQUE DE SÉLECTION DÉTERMINISTE
            // Pour assurer que toutes les unités sélectionnent le MÊME ennemi (Focus Fire),
            // on choisit l'ennemi le plus proche de la position du Manager (ou d'un point central)

            GameObject closestEnemy = enemies
                .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
                .FirstOrDefault();

            if (closestEnemy != null)
            {
                // Définir la nouvelle cible pour toutes les unités
                CurrentTarget = closestEnemy.transform;
                Debug.Log($"Nouvelle cible sélectionnée : {closestEnemy.name}");
            }
            else
            {
                CurrentTarget = null;
                Debug.Log("Plus d'ennemis trouvés. Victoire ou fin de combat.");
            }
        }
        else
        {
            CurrentTarget = null;
            Debug.Log("Plus d'ennemis trouvés. Victoire ou fin de combat.");
        }
    }
}