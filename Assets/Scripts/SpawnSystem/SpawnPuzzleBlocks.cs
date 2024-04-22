using UnityEngine;
using UnityEngine.Pool;

public class SpawnPuzzleBlocks : MonoBehaviour
{
    public static SpawnPuzzleBlocks Instance { get; private set; }

    [Header("Injected Dependencies")]
    public int StartPuzzleBlockAmount;
    [SerializeField] GameObject puzzleBlock;
    public DragAndDrop CurrentBeingDragged;
    public GameObject[,] ActivePuzzleBlocks;
    private ObjectPool<GameObject> puzzleBlockPool;
    public Vector2Int randomRangePuzzleValue;

    // [Header("Settings")]
    public int TotalPuzzleBlockAmount { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TotalPuzzleBlockAmount = GridWorld.Instance.Grid.GetLength(0) * GridWorld.Instance.Grid.GetLength(1);

        puzzleBlockPool = new ObjectPool<GameObject>(
            CreateBlockPoolObj,
            OnTakeObjFromPool,
            OnReturnObjFromPool,
            OnDestroyPoolObj,
            true, TotalPuzzleBlockAmount, TotalPuzzleBlockAmount
        );

        SpawnBlocks();
    }

    private void Update()
    {
        DrawActivePuzzleBlocks();
    }

    private GameObject CreateBlockPoolObj()
    {
        GameObject _obj = Instantiate(puzzleBlock, transform.position, transform.rotation);
        _obj.GetComponent<PuzzleStats>().puzzleBlockPool = puzzleBlockPool;
        return _obj;
    }

    private void OnTakeObjFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReturnObjFromPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPoolObj(GameObject obj)
    {
        Destroy(obj);
    }

    void SpawnBlocks()
    {
        ActivePuzzleBlocks = new GameObject[GridWorld.Instance.Grid.GetLength(0), GridWorld.Instance.Grid.GetLength(1)];

        for (int i = 0; i < StartPuzzleBlockAmount; i++)
        {
            int x = i % GridWorld.Instance.Grid.GetLength(0);
            Vector2 highGridPos = new(x, GridWorld.Instance.Grid.GetLength(1) - 1);
            Vector2 flooredGridPos = GridWorld.Instance.FindFlooredGridPosAt(highGridPos);
            if (flooredGridPos.x < 0) continue;

            var puzzleBlockClone = puzzleBlockPool.Get();
            int _puzzleValue = Random.Range(randomRangePuzzleValue.x, randomRangePuzzleValue.y);

            Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, GridWorld.Instance.Offset);
            puzzleBlockClone.transform.position = flooredWorldPos;
            puzzleBlockClone.GetComponent<PuzzleStats>().PuzzleValue = _puzzleValue;
            puzzleBlockClone.GetComponent<PuzzleStats>().LastLandingPos = flooredWorldPos;

            GridWorld.Instance.SetGridPosValueAt(flooredGridPos, _puzzleValue);

            ActivePuzzleBlocks[(int)flooredGridPos.x, (int)flooredGridPos.y] = puzzleBlockClone;
        }
    }

    public void CheckDownBlocks()
    {
        for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x += 1)
        {
            for (int y = 0; y < ActivePuzzleBlocks.GetLength(1); y += 1)
            {
                var currPuzzleBlock = ActivePuzzleBlocks[x, y];
                if (currPuzzleBlock == null) continue;
                var gridPos = new Vector2(x, y);
                var worldPos = GridUtility.ConvertGridPosToWorldPos(gridPos, GridWorld.Instance.Offset);

                var currBlock = GetPuzzleBlockAt(worldPos);
                if (currBlock == null) continue;

                var downPos1 = worldPos + Vector2.down;
                if (GridWorld.Instance.GetWorldPosValueAt(downPos1) == 0)
                {
                    var downPos2 = worldPos + Vector2.down * 2;
                    if (
                        GridWorld.Instance.GetWorldPosValueAt(worldPos) ==
                        GridWorld.Instance.GetWorldPosValueAt(downPos2)
                    )
                    {
                        // Passed matching rule at down postion2, we remove currBlock
                        LeanTween.move(currBlock, downPos2, .07f).setOnComplete(() =>
                        {
                            RemovePuzzleBlockRendererAt(worldPos);
                            SetPuzzleBlockAt(worldPos, 0, null);
                            SetPuzzleBlockAt(
                                   downPos2,
                                   GridWorld.Instance.GetWorldPosValueAt(downPos2) + 1,
                                   SpawnPuzzleBlocks.Instance.GetPuzzleBlockAt(downPos2)
                               );
                            CheckDownBlocks();
                        });
                        continue;
                    }
                    // Not passed matching rule, we move currBlock down to empty space
                    LeanTween.move(currBlock, downPos1, .07f).setOnComplete(() =>
                    {
                        SetPuzzleBlockAt(worldPos, 0, null);
                        SetPuzzleBlockAt(
                               downPos1,
                               currBlock.GetComponent<PuzzleStats>().PuzzleValue,
                               currBlock
                           );
                        CheckDownBlocks();
                    });
                }
            }
        }
    }

    public void SetPuzzleBlockOccupiedAt(Vector2 worldPos, GameObject block)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.Offset);
        if (GridWorld.Instance.IsGridPosOutsideAt(gridPos)) return;
        ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y] = block;
    }

    public void RemovePuzzleBlockRendererAt(Vector2 worldPos)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.Offset);
        if (GridWorld.Instance.IsGridPosOutsideAt(gridPos)) return;

        var puzzleBlock = ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
        if (puzzleBlock == null) return;
        if (puzzleBlock.activeSelf) puzzleBlockPool.Release(puzzleBlock);
        ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y] = null;
    }

    public GameObject GetPuzzleBlockAt(Vector2 worldPos)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.Offset);
        return GetPuzzleBlockIn(gridPos);
    }

    GameObject GetPuzzleBlockIn(Vector2 gridPos)
    {
        if (GridWorld.Instance.IsGridPosOutsideAt(gridPos)) return null;
        return ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
    }

    /// <summary>
    /// Sync position value between GridWorld.Grid and SpawnPuzzleBlocks.ActivePuzzleBlocks
    /// </summary>
    /// <param name="desWorldPos"></param>
    /// <param name="lastWorldPos"></param>
    public void SetPuzzleBlockAt(Vector2 desWorldPos, int value, GameObject block)
    {
        GridWorld.Instance.SetWorldPosValueAt(desWorldPos, value);
        SetPuzzleBlockOccupiedAt(desWorldPos, block);

        if (block)
        {
            block.GetComponent<PuzzleStats>().LastLandingPos = desWorldPos;
            block.GetComponent<PuzzleStats>().PuzzleValue = value;
        }
    }

    /// <summary>
    /// For debug only
    /// </summary>
    void DrawActivePuzzleBlocks()
    {
        for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x += 1)
        {
            for (int y = 0; y < ActivePuzzleBlocks.GetLength(1); y += 1)
            {
                Vector2 pos = GridUtility.ConvertGridPosToWorldPos(
                    new Vector2Int(x, y) + new Vector2(ActivePuzzleBlocks.GetLength(0), 0),
                    GridWorld.Instance.Offset
                );

                Utility.DrawQuad(pos, 1, 0);
                if (ActivePuzzleBlocks[x, y] != null)
                {
                    Utility.DrawQuad(pos, .8f, 1);
                }
            }
        }
    }
}
