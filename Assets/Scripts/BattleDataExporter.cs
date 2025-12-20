using UnityEngine;
using System.IO;

public static class BattleDataExporter
{
    private static readonly string FilePath = Path.Combine(Application.dataPath, "..", "battle_data.csv");

    public static void WriteBattleData(bool isVictory, float duration, int aliveDrones, int aliveTurrets, int remainingHealth)
    {
        if (!SimulationManager.IsSimulationRunning) return;

        int battleIndex = PlayerPrefs.GetInt("BattleIndex", 0);
        PlayerPrefs.SetInt("BattleIndex", battleIndex + 1);

        string victoryStatus = isVictory ? "1" : "0";

        string data = $"{battleIndex}\t{victoryStatus}\t{(int)duration}\t{aliveDrones}\t{aliveTurrets}\t{remainingHealth}";

        try
        {
            if (!File.Exists(FilePath))
            {
                string header = "INDEX_BATTLE\tIS_VICTORY\tDURATION\tALIVE_DRONES\tALIVE_TURRETS\tREMAINING_CUMMULATIVE_HEALTH_POINTS";
                File.WriteAllText(FilePath, header + "\n");
            }

            File.AppendAllText(FilePath, data + "\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to write battle data: {ex.Message}");
        }
    }
}
