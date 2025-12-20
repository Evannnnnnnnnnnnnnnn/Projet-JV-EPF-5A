using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Added for SceneManager

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    public static bool IsSimulationRunning { get; private set; } = false;

    private int simulationCount = 20;
    private int currentSimulation = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        REDTurretsSelectEnemy.Reset();
        REDDronesSelectEnemy.Reset();
    }

    public void StartSimulation()
    {
        if (IsSimulationRunning) return;

        IsSimulationRunning = true;
        currentSimulation = 0;
        StartCoroutine(SimulationLoop());
    }

    private IEnumerator SimulationLoop()
    {
        while (currentSimulation < simulationCount)
        {
            // Wait for the end of the current battle
            yield return new WaitUntil(() => !IsSimulationRunning || BattleHasEnded());

            if (!IsSimulationRunning)
            {
                yield break; // Stop the loop if simulation was cancelled
            }
            
            currentSimulation++;

            if (currentSimulation < simulationCount)
            {
                // Restart the scene for the next battle
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                IsSimulationRunning = false;
                Debug.Log("Simulation finished!");
            }
        }
    }

    // This method needs to be called when the battle ends
    public static void NotifyBattleEnded()
    {
        if (Instance != null)
        {
            Instance.battleEnded = true;
        }
    }

    private bool battleEnded = false;
    private bool BattleHasEnded()
    {
        if (battleEnded)
        {
            battleEnded = false; // Reset for the next battle
            return true;
        }
        return false;
    }
}