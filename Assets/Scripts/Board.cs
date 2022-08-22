using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] GameObject rowPrefab;
    public int Width;  //=> Tiles.GetLength(dimension: 0);
    public int Height; // => Tiles.GetLength(dimension: 1);

    // public Row[] rows;
    public List<Row> rows;

    public Tile[,] Tiles { get; private set; }

    // public int Width => Tiles.GetLength(dimension: 0);
    // public int Height => Tiles.GetLength(dimension: 1);

    private readonly List<Tile> _selection = new List<Tile>();

    private const float TweenDuration = 0.25f;

    public Vector3 tileInterval;


    private void Awake() => Instance = this;

#if UNITY_EDITOR
    [SerializeField] bool refresh;
    private void OnValidate()
    {
        if (refresh == true)
        {
            refresh = false;
            Refresh();
            return;
        }

    }
    void Refresh()
    {
        if (rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] == null)
                    continue;
                StartCoroutine(DestroyRoutine(rows[i].gameObject));
            }
            rows = null;
        }

        rows = new List<Row>();
        for (int i = 0; i < Height; i++)
        {
            var rowInstance = Instantiate(rowPrefab);
            rowInstance.transform.SetParent(transform);
            rowInstance.transform.localScale = Vector3.one;

            var row = rowInstance.GetComponent<Row>();
            row.Refresh(Width);
            rows.Add(row);
        }
    }
    // https://answers.unity.com/questions/1318576/destroy-child-objects-onvalidate.html
    IEnumerator DestroyRoutine(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }
#endif

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Count), rows.Count];
        // tileInterval = Tiles[1, 0].transform.position - Tiles[0, 1].transform.position;
        // print(transform.GetChild(1).GetChild(0).position);
        // print(transform.GetChild(0).GetChild(1).position);
        // tileInterval = transform.GetChild(1).GetChild(0).position - transform.GetChild(0).GetChild(1).position;
        // print(tileInterval);

        SetTilesItem();
    }

    public void SetTilesItem()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                Tiles[x, y] = tile;
            }
        }
        Pop();
    }

    Vector3 clickMousePos = Vector3.zero;
    Vector3 moveMousePos;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var connectedTile in Tiles[0,0].GetConnectedTiles())
            // foreach (var connectedTile in Tiles[0,0].GetConnectedLineTiles())
            {
                connectedTile.icon.transform.DOScale(1.25f, TweenDuration).Play();
            }
        }
        if (Input.GetKey(KeyCode.Mouse0) && _selection.Count == 1)
        {
            if (clickMousePos == Vector3.zero)
            {
                clickMousePos = Input.mousePosition;
                return;
            }
            else
            {
                moveMousePos = Input.mousePosition - clickMousePos;
                print(moveMousePos);
                int x = _selection[0].x;
                int y = _selection[0].y;
                if (Mathf.Abs(moveMousePos.x) > tileInterval.x)
                {
                    if (moveMousePos.x > 0 && x + 1 < Width)
                        Select(Tiles[x + 1, y]);
                    if (moveMousePos.x < 0 && x - 1 >= 0)
                        Select(Tiles[x - 1, y]);
                    clickMousePos = Vector3.zero;
                    return;
                }
                if (Mathf.Abs(moveMousePos.y) > tileInterval.y)
                {
                    if (moveMousePos.y < 0 && y + 1 < Height)
                        Select(Tiles[x, y + 1]);
                    if (moveMousePos.y > 0 && y - 1 >= 0)
                        Select(Tiles[x, y - 1]);
                    clickMousePos = Vector3.zero;
                    return;
                }
            }
        }

    }

    public async void Select(Tile tile)
    {
        if (_selection.Contains(tile) == true)
            return;

        // if (_selection.Contains(tile) == false)
        // {
            if (_selection.Count > 0)
            {
                if (System.Array.IndexOf(_selection[0].Neighbours, tile) != -1)
                {
                  _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        // }


        if (_selection.Count < 2)
            return;

        print($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
                .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool CanPop()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                if (Tiles[x, y].GetConnectedTiles().Count >= 3)
                // if (Tiles[x, y].GetConnectedLineTiles().Skip(1).Count() >= 2)
                    return true;
            }
        }
        return false;
    }

    private async void Pop()
    {
        // 움직인 애들부터 Pop 한다.
        if (_selection != null && _selection.Count == 2)
        {
            await PopTiles(_selection.ToList());
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var popResult = await PopTile(Tiles[x, y]);
                if (popResult == false)
                    continue;

                x = 0;
                y = 0;
            }
        }
    }

    private async Task<bool> PopTile(Tile tile)
    {
        // var tile = Tiles[x, y];
        var connectedTiles = tile.GetConnectedTiles();
        // var connectedTiles = tile.GetConnectedLineTiles();
        
        // if (connectedTiles.Skip(1).Count() < 2)
        if (connectedTiles.Count < 3)
            return false;

        audioSource.PlayOneShot(collectSound);
        ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;

        await DeflateTiles(connectedTiles);
        await DropUpside(connectedTiles);
        // await FillEmpty();

        return true;
    }

    private async Task<bool> PopTiles(List<Tile> tiles)
    {
        List<Tile> totalConnectedTiles = new List<Tile>();
        for (int i = 0; i < tiles.Count; i++)
        {
            var connectedTiles = tiles[i].GetConnectedTiles();
            if (connectedTiles.Count < 3)
                continue;
            totalConnectedTiles.AddRange(connectedTiles);
        }
        if (totalConnectedTiles.Count < 3)
            return false;

        audioSource.PlayOneShot(collectSound);
        // TODO: 타일 종류가 다를때 점수?
        ScoreCounter.Instance.Score += totalConnectedTiles[0].Item.value * totalConnectedTiles.Count;

        await DeflateTiles(totalConnectedTiles);
        await DropUpside(totalConnectedTiles);
        // await FillEmpty();

        return true;
    }

    private async Task DeflateTiles(List<Tile> connectedTiles)
    {
        var deflateSequence = DOTween.Sequence();
        foreach (var connectedTile in connectedTiles)
        {
            deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
        }
        await deflateSequence.Play().AsyncWaitForCompletion();
    }

    private async Task DropUpside(List<Tile> connectedTiles)
    {
        // 연결된 타일들은 정해짐.
        // 연결된 타일들의 값을 비운다.
        // 값이 비워진 타일들을 바로 위의 타일로 채워준다.
        var dropSequence = DOTween.Sequence();
        foreach (var connectedTile in connectedTiles)
        {
            connectedTile.Item = null;
        }
        var emptyTiles = new List<Tile>(connectedTiles);
        var makeNewItemTiles = new List<Tile>();
        while (emptyTiles.Count > 0)
        {
            var emptyTile = emptyTiles[0];

            // 밑에 빈 타일이 있는지 확인.
            // 빈 타일들 중 제일 밑으로 간다.
            int checkX = emptyTile.x;
            int checkY = emptyTile.y + 1;
            Tile checkTile;
            for (; checkY < Height; checkY++)
            {
                checkTile = Tiles[checkX, checkY];
                if (emptyTiles.Contains(checkTile) && checkTile.Item == null)
                {
                    emptyTile = Tiles[checkX, checkY];
                }
                else
                {
                    break;
                }
            }

            // 위의 타일 중 떨어뜨릴 타일을 찾는다.
            int dropX = emptyTile.x;
            int dropY = emptyTile.y - 1;
            Tile dropTile = null;
            int dropLength = 0;
            for (; dropY >= 0; dropY--)
            {
                dropLength++;
                if (Tiles[dropX, dropY].Item == null)
                    continue;
                dropTile = Tiles[dropX, dropY];
                break;
            }
            // 떨어뜨릴 타일이 없음. 새로 만들어줘야 한다.
            if (dropTile == null)
            {
                makeNewItemTiles.Add(emptyTile);
                emptyTiles.Remove(emptyTile);
                continue;
            }

            emptyTile.icon.sprite = dropTile.icon.sprite;
            emptyTile.icon.transform.position = dropTile.icon.transform.position;
            emptyTile.icon.transform.localScale = Vector3.one;
            dropSequence.Join(emptyTile.icon.transform.DOMove(emptyTile.transform.position, TweenDuration));
            // dropSequence.Join(emptyTile.icon.transform.DOMove(emptyTile.transform.position, TweenDuration * dropLength));
            // dropSequence.Join(emptyTile.icon.transform.DOMove(emptyTile.transform.position, TweenDuration * (dropLength * 0.5f + 0.5f)));
            // print($"dropLength:{dropLength}");

            emptyTile.Item = dropTile.Item;
            dropTile.Item = null;
            emptyTiles.Remove(emptyTile);
            emptyTiles.Add(dropTile);
        }




        // 아이템을 새로 만들어줘야할 타일들.
        while (makeNewItemTiles.Count > 0)
        {
            var emptyTile = makeNewItemTiles[0];
            // 밑에 빈 타일이 있는지 확인.
            int checkX = emptyTile.x;
            int checkY = emptyTile.y + 1;
            Tile checkTile;
            for (; checkY < Height; checkY++)
            {
                checkTile = Tiles[checkX, checkY];
                if (makeNewItemTiles.Contains(checkTile) && checkTile.Item == null)
                    emptyTile = Tiles[checkX, checkY];
                else
                    break;
            }
            int length = 1;
            // 위에 빈 타일들 가져오기.
            var columnTiles = new List<Tile>();
            columnTiles.Add(emptyTile);
            checkY = emptyTile.y - 1;
            for (; checkY >= 0; checkY--)
            {
                // checkTile = Tiles[checkX, checkY];
                // if (makeNewItemTiles.Contains)
                // if (checkTile.Item == null)
                // length
                columnTiles.Add(Tiles[checkX, checkY]);
                length++;
            }

            // 아이템 만들어서 떨어뜨리기.
            Tile curTile;
            // print($"tileInterval: {tileInterval}");
            while (columnTiles.Count > 0)
            {
                curTile = columnTiles[0];

                curTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                curTile.icon.transform.position = curTile.transform.position + (Vector3.up * tileInterval.y * length);
                curTile.icon.transform.localScale = Vector3.one;
                dropSequence.Join(curTile.icon.transform.DOMove(curTile.transform.position, TweenDuration));
                // dropSequence.Join(curTile.icon.transform.DOMove(curTile.transform.position, TweenDuration * length));

                columnTiles.RemoveAt(0);
                makeNewItemTiles.Remove(curTile);
            }
        }



        await dropSequence.Play().AsyncWaitForCompletion();
    }

    private async Task FillEmpty()
    {
        // var inflateSequence = DOTween.Sequence();
        // foreach (var connectedTile in connectedTiles)
        // {
        //     connectedTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

        //     inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
        // }
        // await inflateSequence.Play().AsyncWaitForCompletion();
        
        var inflateSequence = DOTween.Sequence();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Tiles[x, y].Item != null)
                    continue;
                
                Tiles[x, y].Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                inflateSequence.Join(Tiles[x, y].icon.transform.DOScale(Vector3.one, TweenDuration));
            }
        }
        await inflateSequence.Play().AsyncWaitForCompletion();
    }
}
