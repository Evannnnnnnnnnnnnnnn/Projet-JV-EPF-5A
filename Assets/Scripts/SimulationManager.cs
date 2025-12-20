using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }
    public static bool IsSimulationRunning { get; private set; } = false;
    public static int CurrentBattleIndex { get; private set; } = 0;

    private int simulationCount = 20;
    private int currentSimulation = 0;
    private bool simulationShouldStart = false;

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

        if (simulationShouldStart)
        {
            simulationShouldStart = false;
            IsSimulationRunning = true;
            currentSimulation = 0;
            CurrentBattleIndex = 0;
            StartCoroutine(SimulationLoop());
        }
    }

    public void StartSimulation()
    {
        if (IsSimulationRunning) return;

        BattleDataExporter.ResetAndPrepareFile();
        simulationShouldStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator SimulationLoop()
    {
        while (currentSimulation < simulationCount)
        {
            CurrentBattleIndex = currentSimulation;
            yield return new WaitUntil(() => !IsSimulationRunning || BattleHasEnded());

            if (!IsSimulationRunning)
            {
                yield break;
            }
            
            currentSimulation++;

            if (currentSimulation < simulationCount)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                IsSimulationRunning = false;
                Debug.Log("Simulation finished!");
            }
        }
    }

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
            battleEnded = false;
            return true;
        }
        return false;
    }
}
