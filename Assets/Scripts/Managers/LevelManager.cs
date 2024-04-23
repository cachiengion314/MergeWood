using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Injected Dependencies")]
    public PuzzleManager puzzleManager;
    public int StartPuzzleBlockAmount;
    public float IntervalSpawnTime;
    public bool shouldIntervalContinue = true;
    float timer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = IntervalSpawnTime;
    }

    private void Update()
    {
        if (!shouldIntervalContinue)
        {
            timer = IntervalSpawnTime;
            return;
        };

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnBlocks(12);
            timer = IntervalSpawnTime;
        }
    }

    void SpawnBlocks(int amount)
    {
     
    }
}
