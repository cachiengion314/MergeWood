using UnityEngine;
using UnityEngine.Pool;

public class SpawnPuzzleBlocks : MonoBehaviour
{
    public static SpawnPuzzleBlocks Instance { get; private set; }

    [Header("Injected Dependencies")]
    public int StartPuzzleBlockNumber;
    [SerializeField] GameObject puzzleBlock;
    public DragAndDrop CurrentBeingDragged;
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
        _obj.GetComponent<Stats>().puzzleBlockPool = puzzleBlockPool;
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
        for (int i = 0; i < StartPuzzleBlockNumber; i++)
        {
            int x = i % GridWorld.Instance.Grid.GetLength(0);
            Vector2 highGridPos = new(x, GridWorld.Instance.Grid.GetLength(1) - 1);
            Vector2 flooredGridPos = GridWorld.Instance.FindFlooredGridPosAt(highGridPos);
            if (flooredGridPos.x < 0) continue;

            int _puzzleValue = Random.Range(1, 5);
            var puzzleBlockClone = puzzleBlockPool.Get();

            Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, GridWorld.Instance.Offset);
            puzzleBlockClone.transform.position = flooredWorldPos;
            puzzleBlockClone.GetComponent<Stats>().PuzzleValue = _puzzleValue;
            puzzleBlockClone.GetComponent<Stats>().LastLandingPos = flooredWorldPos;

            GridWorld.Instance.SetGridPosValueAt(flooredGridPos, _puzzleValue);
        }
    }
}
