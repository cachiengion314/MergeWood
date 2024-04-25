using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Gameplay,
    Gameover,
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Injected Dependencies")]
    [SerializeField] GridWorld gridWorld;

    [Header("Events")]
    public Action<GameState> onGameStateChanged;

    [Header("Settings")]
    GameState _gameState;
    public int StartPuzzleBlockAmount;
    [Range(0, 10)]
    public float StartSpawnTime;
    [Range(0, 10)]
    public float IntervalSpawnTime;
    public bool shouldStartSpawnTimer = true;
    public bool shouldIntervalSpawnTimer = true;
    float intervalSpawnTimer;
    float startSpawnTimer;
    bool isCheckGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        intervalSpawnTimer = IntervalSpawnTime;
        startSpawnTimer = StartSpawnTime;

        SetGameState(GameState.Menu);
    }

    private void Update()
    {
        StartSpawnTimerInvoker();
        IntervalSpawnTimerInvoker();
        CheckGameOver();
    }

    public void SetGameState(GameState state)
    {
        _gameState = state;
        onGameStateChanged?.Invoke(state);
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    public void CheckGameOver()
    {
        if (_gameState == GameState.Gameover) return;
        if (isCheckGameOver) return;

        if (PuzzleManager.Instance.IsHighestRowHasPuzzle())
        {
            isCheckGameOver = true;
            LeanTween.delayedCall(IntervalSpawnTime - .1f, () =>
            {
                if (PuzzleManager.Instance.IsHighestRowHasPuzzle())
                {
                    SetGameState(GameState.Gameover);
                    return;
                }
                isCheckGameOver = false;
            });
        }
    }

    void StartSpawnTimerInvoker()
    {
        if (_gameState != GameState.Gameplay || !shouldStartSpawnTimer) return;

        startSpawnTimer -= Time.deltaTime;
        if (startSpawnTimer <= 0)
        {
            shouldStartSpawnTimer = false;

            PuzzleManager.Instance.SpawnBlocks(StartPuzzleBlockAmount);
            PuzzleManager.Instance.CheckDownBlocks();
        }
    }

    void IntervalSpawnTimerInvoker()
    {
        if (_gameState != GameState.Gameplay || !shouldIntervalSpawnTimer)
        {
            intervalSpawnTimer = IntervalSpawnTime;
            return;
        };
        if (PuzzleManager.Instance.CurrentBeingDragged || PuzzleManager.Instance.IsTweening) return;

#if UNITY_EDITOR
        Utility.Print("intervalSpawnTimer " + intervalSpawnTimer);
#endif

        intervalSpawnTimer -= Time.deltaTime;
        if (intervalSpawnTimer <= 0)
        {
            intervalSpawnTimer = IntervalSpawnTime;

            PuzzleManager.Instance.MoveRowBlocksAt(0, Vector2.up);
            PuzzleManager.Instance.SpawnBlocks(gridWorld.Grid.GetLength(0));
            PuzzleManager.Instance.CheckDownBlocks();
        }
    }
}
