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

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> {this,};
        if (exclude == null)
        {
            exclude = new List<Tile>{this,};
        }
        else
        {
            exclude.Add(this);
        }
        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item)
                continue;
            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }
        return result;
    }

    public enum Direction
    {
        None,
        Horizontal,
        Vertical,
    }

    public List<Tile> GetConnectedLineTiles(List<Tile> exclude = null, Direction direction = Direction.None)
    {
        var result = new List<Tile> { this, };
        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }
        if (direction == Direction.None || direction == Direction.Horizontal)
        {
            if (Left != null && !exclude.Contains(Left) && Left.Item == Item)
                result.AddRange(Left.GetConnectedLineTiles(exclude, Direction.Horizontal));
            if (Right != null && !exclude.Contains(Right) && Right.Item == Item)
                result.AddRange(Right.GetConnectedLineTiles(exclude, Direction.Horizontal));
        }
        if (direction == Direction.None && result.Count > 1)
            return result;
            
        if (direction == Direction.None || direction == Direction.Vertical)
        {
            if (Top != null && !exclude.Contains(Top) && Top.Item == Item)
                result.AddRange(Top.GetConnectedLineTiles(exclude, Direction.Vertical));
            if (Bottom != null && !exclude.Contains(Bottom) && Bottom.Item == Item)
                result.AddRange(Bottom.GetConnectedLineTiles(exclude, Direction.Vertical));
        }
        return result;
    }
}
