using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;
    public Item Item
    {
        get => _item;
        set
        {
            if (_item == value)
                return;
            _item = value;
            icon.sprite = _item?.sprite;
        }
    }
    public Image icon;
    public Button button;

    public Tile Left => (x > 0) ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => (y > 0) ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => (x < Board.Instance.Width - 1) ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Bottom => (y < Board.Instance.Height - 1) ? Board.Instance.Tiles[x, y + 1] : null;

    // public System.Func<List<Tile>, List<Tile>> GetConnectedTilesFunc;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom,
    };

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    // public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    // {
    //     if (GetConnectedTilesFunc == null)
    //         GetConnectedTilesFunc = (exclude) => GetConnectedTilesAll(exclude);
    //         // GetConnectedTilesFunc = (exclude) => GetConnectedLineTiles(exclude);
    //     // print(GetConnectedTilesFunc);
    //     return GetConnectedTilesFunc(exclude);
    // }

    // public List<Tile> GetConnectedTilesAll(List<Tile> exclude = null)
    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> {this,};
        bool root = false;
        if (exclude == null)
        {
            exclude = new List<Tile>();
            root = true;
        }
        exclude.Add(this);

        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item)
                continue;
            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }
        // 연결된 타일을 모두 구했을 때 일직선으로 세개 이상 연결된 타일만 남긴다.
        if (root == true)
        {
            var lineResult = new List<Tile>();
            // var candidate = new List<Tile>(result);
            // while (candidate.Count > 0)
            while (result.Count > 0)
            {
                var line = CheckConnectedLine(result[0]);
                if (line != null)
                {
                    print($"line.Count:{line.Count}");
                    lineResult.AddRange(line);
                    foreach(var tile in line) 
                        result.Remove(tile);
                }
                else
                {
                    result.RemoveAt(0);
                }
            }
            return lineResult;
        }
        return result;
    }
    List<Tile> CheckConnectedLine(Tile tile)
    {
        int x = tile.x;
        int y = tile.y;
        Item item = tile.Item;
        Tile[,] boardTiles = Board.Instance.Tiles;
        // 가로.
        {
            //좌2 좌1 0
            if ((x - 2 >= 0 && boardTiles[x - 2, y].Item == item) && (boardTiles[x - 1, y].Item == item))
                return new List<Tile> { boardTiles[x - 2, y], boardTiles[x - 1, y], tile };
            //좌1 0 우1
            if ((x - 1 >= 0 && boardTiles[x - 1, y].Item == item) && (x + 1 < Board.Instance.Width && boardTiles[x + 1, y].Item == item))
                return new List<Tile> { boardTiles[x - 1, y], boardTiles[x + 1, y], tile };
            //0 우1 우2
            if ((x + 2 < Board.Instance.Width && boardTiles[x + 2, y].Item == item) && (boardTiles[x + 1, y].Item == item))
                return new List<Tile> { boardTiles[x + 1, y], boardTiles[x + 2, y], tile };
        }
        // 세로.
        {
            //상2 상1 0
            if ((y - 2 >= 0 && boardTiles[x, y - 2].Item == item) && (boardTiles[x, y - 1].Item == item))
                return new List<Tile> { boardTiles[x, y - 2], boardTiles[x, y - 1], tile };
            //상1 0 하1
            if ((y - 1 >= 0 && boardTiles[x, y - 1].Item == item) && (y + 1 < Board.Instance.Height && boardTiles[x, y + 1].Item == item))
                return new List<Tile> { boardTiles[x, y - 1], boardTiles[x, y + 1], tile };
            //0 하1 하2
            if ((y + 2 < Board.Instance.Height && boardTiles[x, y + 2].Item == item) && (boardTiles[x, y + 1].Item == item))
                return new List<Tile> { boardTiles[x, y + 1], boardTiles[x, y + 2], tile };
        }
        return null;
    }

    public enum Direction
    {
        None,
        Horizontal,
        Vertical,
    }

    // public List<Tile> GetConnectedLineTiles(List<Tile> exclude = null, Direction direction = Direction.None)
    // {
    //     var result = new List<Tile> { this, };
    //     if (exclude == null)
    //     {
    //         exclude = new List<Tile> { this, };
    //     }
    //     else
    //     {
    //         exclude.Add(this);
    //     }
    //     if (direction == Direction.None || direction == Direction.Horizontal)
    //     {
    //         if (Left != null && !exclude.Contains(Left) && Left.Item == Item)
    //             result.AddRange(Left.GetConnectedLineTiles(exclude, Direction.Horizontal));
    //         if (Right != null && !exclude.Contains(Right) && Right.Item == Item)
    //             result.AddRange(Right.GetConnectedLineTiles(exclude, Direction.Horizontal));
    //     }
    //     if (direction == Direction.None && result.Count > 1)
    //         return result;

    //     if (direction == Direction.None || direction == Direction.Vertical)
    //     {
    //         if (Top != null && !exclude.Contains(Top) && Top.Item == Item)
    //             result.AddRange(Top.GetConnectedLineTiles(exclude, Direction.Vertical));
    //         if (Bottom != null && !exclude.Contains(Bottom) && Bottom.Item == Item)
    //             result.AddRange(Bottom.GetConnectedLineTiles(exclude, Direction.Vertical));
    //     }
    //     return result;
    // }
}
