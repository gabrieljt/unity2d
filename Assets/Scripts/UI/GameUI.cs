using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour, IDestroyable, IDisposable
{
    [SerializeField]
    private Text levelLabel, stepsLeftLabel, stepsTakenLabel;

    private Action<MonoBehaviour> destroyed = delegate { };

    public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

    private void Awake()
    {
        Debug.Assert(levelLabel);
        Debug.Assert(stepsLeftLabel);
        Debug.Assert(stepsTakenLabel);

        var gameInstance = Game.Instance;
        gameInstance.LevelStarted += OnGameLevelStarted;
        gameInstance.LevelReloaded += OnGameLevelReloaded;

        SetEnabled(false);
        SetValues();
    }

    private void OnGameLevelStarted()
    {
        SetEnabled(true);
        SetValues();
    }

    private void OnGameLevelReloaded()
    {
        SetEnabled(false);
    }

    private void SetEnabled(bool enabled)
    {
        enabled = levelLabel.enabled = stepsLeftLabel.enabled = stepsTakenLabel.enabled = enabled;
    }

    private void SetValues()
    {
        var gameInstance = Game.Instance;
        SetLevelLabel(gameInstance.Params.Level);
        SetStepsLeftLabel(gameInstance.Params.StepsLeft);
        SetStepsTakenLabel(GameParams.TotalStepsTaken);
    }

    private void SetLevelLabel(int level)
    {
        levelLabel.text = "Dungeon Level: " + level;
    }

    private void SetStepsLeftLabel(int stepsLeft)
    {
        stepsLeftLabel.text = "Steps Left: " + stepsLeft;
    }

    private void SetStepsTakenLabel(int steps)
    {
        stepsTakenLabel.text = "Steps Taken: " + steps;
    }

    private void LateUpdate()
    {
        SetValues();
        return;
    }

    public void Dispose()
    {
        var gameInstance = Game.Instance;
        if (gameInstance)
        {
            gameInstance.LevelStarted -= OnGameLevelStarted;
            gameInstance.LevelReloaded -= OnGameLevelReloaded;
        }
    }

    public void OnDestroy()
    {
        Dispose();
    }
}