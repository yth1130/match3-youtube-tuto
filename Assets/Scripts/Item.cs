using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public sealed class Item : ScriptableObject
{
    public int value; // 점수.
    public Sprite sprite;
}
