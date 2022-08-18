using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    
    public List<Tile> tiles;

#if UNITY_EDITOR
    // [SerializeField] bool refresh;
    // private void OnValidate()
    // {
    //     if (refresh == true)
    //     {
    //         refresh = false;
    //         Refresh();
    //         return;
    //     }
    // }
    public void Refresh(int width)
    {
        tiles = new List<Tile>();
        for (int i = 0; i < width; i++)
        {
            var tileInstance = Instantiate(tilePrefab);
            tileInstance.transform.SetParent(transform);
            tileInstance.transform.localScale = Vector3.one;
            tiles.Add(tileInstance.GetComponent<Tile>());
        }
    }
#endif
}
