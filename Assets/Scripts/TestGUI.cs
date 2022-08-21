using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestGUI : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] Button resetButton;
    
    void Start()
    {
        resetButton.onClick.AddListener(() =>
        {
            board.SetTilesItem();
        });
    }

    // void Update()
    // {
        
    // }
}
