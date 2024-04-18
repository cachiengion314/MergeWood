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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        puzzleBlockPool = new ObjectPool<GameObject>(
            CreateBlockPoolObj,
            OnTakeObjFromPool,
            OnReturnObjFromPool,
            OnDestroyPoolObj,
            true, 20, 50
        );

        SpawnBlocks();
    }

    private GameObject CreateBlockPoolObj()
    {
        GameObject _obj = Instantiate(puzzleBlock, transform.position, transform.rotation);
        _obj.GetComponent<PuzzleStats>().puzzleBlockPool = puzzleBlockPool;
        return _obj;
    }

    private void OnTakeObjFromPool(GameObject obj)
    {
        obj.gameObject.SetActive(true);
    }

    private void OnReturnObjFromPool(GameObject obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObj(GameObject obj)
    {
        Destroy(obj.gameObject);
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
            int _puzzleValue = Random.Range(1, 5);

            Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, GridWorld.Instance.Offset);
            puzzleBlockClone.transform.position = flooredWorldPos;
            puzzleBlockClone.GetComponent<PuzzleStats>().PuzzleValue = _puzzleValue;
            puzzleBlockClone.GetComponent<PuzzleStats>().LastLandingPos = flooredWorldPos;

            GridWorld.Instance.SetGridPosValueAt(flooredGridPos, _puzzleValue);

            ActivePuzzleBlocks[(int)flooredGridPos.x, (int)flooredGridPos.y] = puzzleBlockClone;
        }
    }

    public void MovePuzzleBlockTo(Vector2 desWorldPos, Vector2 lastWorldPos)
    {
        var currPuzzleBlock = FindPuzzleBlockAt(lastWorldPos);
        var lasGridPos = GridUtility.ConvertWorldPosToGridPos(lastWorldPos, GridWorld.Instance.Offset);
        var desGridPos = GridUtility.ConvertWorldPosToGridPos(desWorldPos, GridWorld.Instance.Offset);
        ActivePuzzleBlocks[(int)lasGridPos.x, (int)lasGridPos.y] = null;
        ActivePuzzleBlocks[(int)desGridPos.x, (int)desGridPos.y] = currPuzzleBlock;
    }

    public void RemovePuzzleBlockAt(Vector2 worldPos)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.Offset);
        if (GridWorld.Instance.IsGridPosOutsideAt(gridPos)) return;

        var puzzleBlock = ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
        if (puzzleBlock == null) return;
        if (puzzleBlock.activeSelf) puzzleBlockPool.Release(puzzleBlock);
        ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y] = null;
    }

    public GameObject FindPuzzleBlockAt(Vector2 worldPos)
    {
        var gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.Offset);
        return FindPuzzleBlockIn(gridPos);
    }

    GameObject FindPuzzleBlockIn(Vector2 gridPos)
    {
        if (GridWorld.Instance.IsGridPosOutsideAt(gridPos)) return null;
        return ActivePuzzleBlocks[(int)gridPos.x, (int)gridPos.y];
    }
}
