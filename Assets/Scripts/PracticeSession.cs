using UnityEngine;

/// <summary>
/// Stores transient practice session state between scene loads.
/// Keeps track of starting level, remaining runs, and return scene.
/// </summary>
public static class PracticeSession
{
    public static bool isActive = false;
    public static int startLevelIndex = 0;
    public static int runsRemaining = 0;
    public static string returnScene = "Lobby Scene";

    public static void StartPractice(int levelIndex, int runs, string returnSceneName)
    {
        isActive = true;
        startLevelIndex = Mathf.Max(0, levelIndex);
        runsRemaining = Mathf.Max(1, runs);
        returnScene = returnSceneName;
    }

    public static void OnRunCompleted()
    {
        runsRemaining = Mathf.Max(0, runsRemaining - 1);
    }

    public static void EndPractice()
    {
        isActive = false;
        runsRemaining = 0;
    }

    public static bool HasMoreRuns() => isActive && runsRemaining > 0;
}
