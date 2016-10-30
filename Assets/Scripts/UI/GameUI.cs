using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField]
    private Text levelLabel, stepsLeftLabel, stepsTakenLabel;

    private void Awake()
    {
        Debug.Assert(levelLabel);
        Debug.Assert(stepsLeftLabel);
        Debug.Assert(stepsTakenLabel);

        var gameInstance = Game.Instance;
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
        var gameInstance = destroyedComponent as Game;
        gameInstance.Loading -= OnGameLoading;
        gameInstance.Started -= OnGameStarted;
        gameInstance.Destroyed -= OnGameDestroyed;
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
}