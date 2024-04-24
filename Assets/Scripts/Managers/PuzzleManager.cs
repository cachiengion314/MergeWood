using System;
using UnityEngine;
using UnityEngine.Pool;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Injected Dependencies")]
    [Tooltip("gridWorld will be injected throught Instantiate method, not now.")]
    [SerializeField] GridWorld gridWorld;
    [SerializeField] PuzzleData[] puzzleDataSet;
    [SerializeField] GameObject puzzleBlock;
    public DragAndDrop CurrentBeingDragged;
    public GameObject[,] ActivePuzzleBlocks;
    private ObjectPool<GameObject> puzzleBlockPool;
    public Vector2Int randomRangePuzzleValue;

    [Header("Settings")]
    public int currentPuzzleThemeIndex;
    // Settting
    public int TotalPuzzleBlockAmount { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TotalPuzzleBlockAmount
            = gridWorld.Grid.GetLength(0) * gridWorld.Grid.GetLength(1);
        puzzleBlockPool = new ObjectPool<GameObject>(
            CreateBlockPoolObj,
            OnTakeObjFromPool,
            OnReturnObjFromPool,
            OnDestroyPoolObj,
            true, TotalPuzzleBlockAmount, TotalPuzzleBlockAmount
        );
        ActivePuzzleBlocks
            = new GameObject[gridWorld.Grid.GetLength(0), gridWorld.Grid.GetLength(1)];
    }

    private void Update()
    {
#if UNITY_EDITOR
        DrawActivePuzzleBlocks();
#endif
    }

    private GameObject CreateBlockPoolObj()
    {
        GameObject _obj = Instantiate(puzzleBlock, transform.position, transform.rotation);
        _obj.GetComponent<PuzzleStats>().puzzleBlockPool = puzzleBlockPool;
        _obj.GetComponent<DragAndDrop>().gridWorld = gridWorld;
        _obj.GetComponent<PuzzleStats>().gridWorld = gridWorld;

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

    public void SpawnBlocks(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int x = i % gridWorld.Grid.GetLength(0);
            Vector2 highGridPos = new(x, gridWorld.Grid.GetLength(1) - 1);
            Vector2 flooredGridPos = gridWorld.FindFlooredGridPosAt(highGridPos);
            if (flooredGridPos.x < 0) continue;

            Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, gridWorld.Offset);

            var puzzleBlockClone = puzzleBlockPool.Get();
            int _puzzleValue = UnityEngine.Random.Range(randomRangePuzzleValue.x, randomRangePuzzleValue.y);
            int _upPuzzleValue = gridWorld.GetValueAt(flooredWorldPos + Vector2.up);
            while (_puzzleValue == _upPuzzleValue)
            {
                _puzzleValue = UnityEngine.Random.Range(randomRangePuzzleValue.x, randomRangePuzzleValue.y);
            }

            puzzleBlockClone.transform.position = flooredWorldPos;
            puzzleBlockClone.GetComponent<PuzzleStats>().PuzzleValue = _puzzleValue;
            puzzleBlockClone.GetComponent<PuzzleStats>().LastLandingPos = flooredWorldPos;

            gridWorld.SetGridPosValueAt(flooredGridPos, _puzzleValue);

            ActivePuzzleBlocks[(int)flooredGridPos.x, (int)flooredGridPos.y] = puzzleBlockClone;
        }
    }

    public Sprite GetSpriteBaseOn(int puzzleValue)
    {
        if (puzzleValue - 1 >= puzzleDataSet[currentPuzzleThemeIndex].renderers.Length)
        {
            return puzzleDataSet[currentPuzzleThemeIndex].renderers[0];
        }
        return puzzleDataSet[currentPuzzleThemeIndex].renderers[puzzleValue - 1];
    }

    /// <summary>
    /// rowIndex aka y index
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="dir"></param>
    public void MoveRowBlocksAt(int rowIndex, Vector2 moveDir)
    {
        for (int y = ActivePuzzleBlocks.GetLength(1) - 1; y >= rowIndex; y--)
        {
            for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x++)
            {
                var gridPos = new Vector2(x, y);
                var currBlock = ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
                if (currBlock == null) continue;

                var desGridPos = gridPos + moveDir;
                var currWorldPos = gridWorld.ConvertGridPosToWorldPos(gridPos);
                var desWorldPos = gridWorld.ConvertGridPosToWorldPos(desGridPos);
                MoveTo(desWorldPos, currWorldPos, currBlock, () => { });
            }
        }
    }

    public bool IsHighestRowHasPuzzle()
    {
        var lastRow = ActivePuzzleBlocks.GetLength(1) - 1;
        for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x += 1)
        {
            var currBlock = ActivePuzzleBlocks[x, lastRow];
            if (currBlock == null) continue;

            return true;
        }
        return false;
    }

    public void CheckDownBlocks()
    {
        for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x += 1)
        {
            for (int y = 0; y < ActivePuzzleBlocks.GetLength(1); y += 1)
            {
                var currBlock = ActivePuzzleBlocks[x, y];
                if (currBlock == null) continue;

                var gridPos = new Vector2(x, y);
                var currWorldPos = GridUtility.ConvertGridPosToWorldPos(gridPos, gridWorld.Offset);

                var downPos1 = currWorldPos + Vector2.down;
                // check empty space
                if (gridWorld.GetValueAt(downPos1) == 0)
                {
                    var downPos2 = currWorldPos + Vector2.down * 2;
                    if (
                        gridWorld.GetValueAt(currWorldPos) ==
                        gridWorld.GetValueAt(downPos2)
                    )
                    {
                        // Passed matching rule at down postion2
                        MatchTo(downPos2, currWorldPos, currBlock, CheckDownBlocks);
                        return;
                    }
                    // Not passed matching rule, we move currBlock down to empty space
                    MoveTo(downPos1, currWorldPos, currBlock, CheckDownBlocks);
                    return;
                }
                // check occupied space
                if (
                    gridWorld.GetValueAt(downPos1) ==
                    gridWorld.GetValueAt(currWorldPos)
                )
                {
                    // Passed matching rule at down postion1
                    MatchTo(downPos1, currWorldPos, currBlock, CheckDownBlocks);
                    return;
                }
            }
        }
    }

    public void MoveTo(Vector2 desWorldPos, Vector2 currWorldPos, GameObject currBlock, Action callback)
    {
        SetPuzzleBlockValueAt(currWorldPos, 0, null);
        SetPuzzleBlockValueAt(
                desWorldPos,
                currBlock.GetComponent<PuzzleStats>().PuzzleValue,
                currBlock
        );
        LeanTween.move(currBlock, desWorldPos, .07f).setOnComplete(() =>
        {
            callback?.Invoke();
        });
    }

    public void MatchTo(Vector2 desWorldPos, Vector2 currWorldPos, GameObject currBlock, Action callback)
    {
        SetPuzzleBlockValueAt(currWorldPos, 0, null);
        SetPuzzleBlockValueAt(
                desWorldPos,
                gridWorld.GetValueAt(desWorldPos) + 1,
                GetPuzzleBlockAt(desWorldPos)
        );

        LeanTween.move(currBlock, desWorldPos, .14f).setOnComplete(() =>
        {
            currBlock.GetComponent<PuzzleStats>().PoolDestroy();
            callback?.Invoke();
        });
    }

    public void SetPuzzleBlockOccupiedAt(Vector2 worldPos, GameObject block)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, gridWorld.Offset);
        if (gridWorld.IsGridPosOutsideAt(gridPos)) return;
        ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y] = block;
    }

    public GameObject GetPuzzleBlockAt(Vector2 worldPos)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, gridWorld.Offset);
        return GetPuzzleBlockIn(gridPos);
    }

    GameObject GetPuzzleBlockIn(Vector2 gridPos)
    {
        if (gridWorld.IsGridPosOutsideAt(gridPos)) return null;
        return ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
    }

    /// <summary>
    /// Sync position value between GridWorld.Grid and PuzzleManager.ActivePuzzleBlocks
    /// </summary>
    /// <param name="desWorldPos"></param>
    /// <param name="lastWorldPos"></param>
    public void SetPuzzleBlockValueAt(Vector2 desWorldPos, int value, GameObject block)
    {
        gridWorld.SetValueAt(desWorldPos, value);
        SetPuzzleBlockOccupiedAt(desWorldPos, block);

        if (block)
        {
            block.GetComponent<PuzzleStats>().LastLandingPos = desWorldPos;
            block.GetComponent<PuzzleStats>().PuzzleValue = value;
        }
    }

    /// <summary>
    /// only for debug
    /// </summary>
    void DrawActivePuzzleBlocks()
    {
        for (int x = 0; x < ActivePuzzleBlocks.GetLength(0); x += 1)
        {
            for (int y = 0; y < ActivePuzzleBlocks.GetLength(1); y += 1)
            {
                Vector2 pos = GridUtility.ConvertGridPosToWorldPos(
                    new Vector2Int(x, y) + new Vector2(ActivePuzzleBlocks.GetLength(0), 0),
                    gridWorld.Offset
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
