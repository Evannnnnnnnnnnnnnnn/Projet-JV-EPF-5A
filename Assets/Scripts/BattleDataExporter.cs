using UnityEngine;
using System.IO;

public static class BattleDataExporter
{
    private static readonly string FilePath = Path.Combine(Application.dataPath, "..", "battle_data.csv");

    public static void ResetAndPrepareFile()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            string header = "INDEX_BATTLE\tIS_VICTORY\tDURATION\tALIVE_DRONES\tALIVE_TURRETS\tREMAINING_CUMMULATIVE_HEALTH_POINTS";
            File.WriteAllText(FilePath, header + "\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to prepare battle data file: {ex.Message}");
        }
    }

    public static void WriteBattleData(int battleIndex, bool isVictory, float duration, int aliveDrones, int aliveTurrets, int remainingHealth)
    {
        if (!SimulationManager.IsSimulationRunning) return;

        string victoryStatus = isVictory ? "1" : "0";

        string data = $"{battleIndex}\t{victoryStatus}\t{(int)duration}\t{aliveDrones}\t{aliveTurrets}\t{remainingHealth}";

        try
        {
            File.AppendAllText(FilePath, data + "\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to write battle data: {ex.Message}");
        }
    }
}