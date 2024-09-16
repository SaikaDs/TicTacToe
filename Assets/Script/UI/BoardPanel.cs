using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BoardPanel : MonoBehaviour
{
    [Header("要求从左到右，从上到下排列")]
    [SerializeField]
    List<GameObject> gridList;

    [Header("标识连线")]
    [SerializeField]
    GameObject connectLine;

    [Header("圈颜色")]
    [SerializeField]
    Color circleColor;

    [Header("叉颜色")]
    [SerializeField]
    Color crossColor;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
        GameSystem.Instance.onBoardChange += OnBoardChange;
        GameSystem.Instance.onConnect += OnConnect;

        int count = GameSystem.rowColCount;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                Grid grid = gridList[i * count + j].GetComponent<Grid>();
                grid.row = i;
                grid.col = j;
            }
        }
    }

    void OnGameStateChange(GameState state)
    {
        var player_camp = GameSystem.Instance.playerCamp;
        bool active = (state == GameState.CrossTurn && player_camp == ChessType.Cross) || (state == GameState.CircleTurn && player_camp == ChessType.Circle);
        foreach (GameObject grid in gridList) 
        {
            grid.GetComponent<Grid>().SetStateActive(active);
        }
        connectLine.SetActive(state == GameState.CrossWinner || state == GameState.CircleWinner);
    }

    void OnBoardChange()
    {
        var board_model = GameSystem.Instance.boardModel;
        int count = GameSystem.rowColCount;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                ChessType curr_type = board_model[i][j];
                GameObject grid = gridList[i * count + j];
                grid.GetComponent<Grid>().SetContent(curr_type);
            }
        }
    }

    private void OnConnect(ChessType chess_type, ConnectedType type, int num)
    {
        //设置颜色
        if (chess_type == ChessType.Cross)
        {
            connectLine.GetComponent<Image>().color = crossColor;
        }
        else if (chess_type == ChessType.Circle)
        {
            connectLine.GetComponent<Image>().color = circleColor;
        }
        //设置位置和旋转
        float unit_length = gridList[1].transform.localPosition.x - gridList[0].transform.localPosition.x;
        float center_num = (GameSystem.rowColCount - 1) / 2.0f;
        float x = 0;
        float y = 0;
        float rotation = 0;
        if (type == ConnectedType.Row)
        {
            x = 0;
            y = (center_num - num) * unit_length;
            rotation = 0;
        }
        else if (type == ConnectedType.Col)
        {
            x = (num - center_num) * unit_length;
            y = 0;
            rotation = 90;
        }
        else if (type == ConnectedType.Diagonal)
        {
            x = 0;
            y = 0;
            rotation = -45;
        }
        else if (type == ConnectedType.AntiDiagonal)
        {
            x = 0;
            y = 0;
            rotation = 45;
        }
        connectLine.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
        connectLine.transform.localEulerAngles = new Vector3(0, 0, rotation);
        connectLine.SetActive(true);
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
        GameSystem.Instance.onBoardChange -= OnBoardChange;
        GameSystem.Instance.onConnect += OnConnect;
    }
}
