using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour, IDestroyable, IDisposable
{
    [SerializeField]
    private Game gameInstance;

    [SerializeField]
    private Text levelLabel, stepsLeftLabel, stepsTakenLabel;

    private Action<IDestroyable> destroyed = delegate { };

    public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

    private void Awake()
    {
        Debug.Assert(levelLabel);
        Debug.Assert(stepsLeftLabel);
        Debug.Assert(stepsTakenLabel);

        gameInstance = Game.Instance;
        gameInstance.Loading += OnGameLoading;
        gameInstance.Started += OnGameStarted;
        gameInstance.Updated += OnGameUpdated;
        gameInstance.Destroyed += OnGameDestroyed;

        SetEnabled(false);
        SetValues();
    }

    private void OnGameLoading()
    {
        SetEnabled(false);
    }

    private void OnGameStarted()
    {
        SetEnabled(true);
        SetValues();
    }

    private void OnGameUpdated()
    {
        SetValues();
        return;
    }

    private void OnGameDestroyed(IDestroyable destroyedComponent)
    {
        gameInstance.Loading -= OnGameLoading;
        gameInstance.Started -= OnGameStarted;
        gameInstance.Updated -= OnGameUpdated;
        gameInstance.Destroyed -= OnGameDestroyed;
    }

    private void SetEnabled(bool enabled)
    {
        enabled = levelLabel.enabled = stepsLeftLabel.enabled = stepsTakenLabel.enabled = enabled;
    }

    private void SetValues()
    {
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

    public void Dispose()
    {
        if (gameInstance)
        {
            gameInstance.Loading -= OnGameLoading;
            gameInstance.Started -= OnGameStarted;
            gameInstance.Updated -= OnGameUpdated;
            gameInstance.Destroyed -= OnGameDestroyed;
        }
    }

    public void OnDestroy()
    {
        Destroyed(this);
        Dispose();
    }
}