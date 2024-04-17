using UnityEngine;

public class SpawnPuzzleBlocks : MonoBehaviour
{
    public static SpawnPuzzleBlocks Instance { get; private set; }

    [Header("Injected dependencies")]
    [SerializeField] GameObject puzzleBlock;

    public GameObject[] PuzzleBlocks { get; private set; }
    public DragAndDrop CurrentBeingDragged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnBlocks();
    }

    void SpawnBlocks()
    {
        PuzzleBlocks = new GameObject[20];

        int index = 0;
        for (int x = 0; x < GridWorld.Instance.Grid.GetLength(0); x++)
        {
            Vector2 highGridPos = new Vector2(x, GridWorld.Instance.Grid.GetLength(1) - 1);
            Vector2 flooredGridPos = GridWorld.Instance.FindFlooredGridPosAt(highGridPos);
            if (flooredGridPos.x < 0) continue;
            GridWorld.Instance.SetGridPosValueAt(flooredGridPos, 1);

            Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, GridWorld.Instance.Offset);
            GameObject puzzleBlockClone = Instantiate(puzzleBlock, flooredWorldPos, Quaternion.identity);
            puzzleBlockClone.GetComponent<Stats>().PuzzleValue = 1;
            puzzleBlockClone.GetComponent<Stats>().LastLandingPos = flooredWorldPos;
            PuzzleBlocks[index] = puzzleBlockClone;

            index++;
        }
    }
}
