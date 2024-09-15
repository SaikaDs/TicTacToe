using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    [SerializeField]
    GameObject circle;
    [SerializeField]
    GameObject cross;
    [SerializeField]
    GameObject button;

    public int row;
    public int col;

    /// <summary>
    /// 因回合到了而允许点击的状态
    /// </summary>
    bool stateActive;
    /// <summary>
    /// 因位置空闲而允许点击的状态
    /// </summary>
    bool placeActive;

    public void SetContent(ChessType chess_type)
    {
        switch (chess_type)
        {
            case ChessType.Empty:
                circle.SetActive(false);
                cross.SetActive(false);
                placeActive = true;
                break;
            case ChessType.Circle:
                circle.SetActive(true);
                cross.SetActive(false);
                placeActive = false;
                break;
            case ChessType.Cross:
                circle.SetActive(false);
                cross.SetActive(true);
                placeActive = false;
                break;
        }
        RefreshButtonActive();
    }

    /// <summary>
    /// 设置因非自身回合而禁用点击的状态
    /// </summary>
    public void SetStateActive(bool acitve)
    {
        stateActive = acitve;
        RefreshButtonActive();
    }

    void RefreshButtonActive()
    {
        button.SetActive(stateActive && placeActive);
    }

    public void OnClick()
    {
        GameSystem.Instance.PlaceAPieace(row, col);
    }
}
