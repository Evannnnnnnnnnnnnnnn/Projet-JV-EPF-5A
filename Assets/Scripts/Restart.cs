using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public SimulationManager simulationManager;

    public void RestartSameLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartSimulation()
    {
        if (simulationManager != null)
        {
            simulationManager.StartSimulation();
        }
        else
        {
            Debug.LogError("SimulationManager not set on Restart script!");
        }
    }
}
