using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BoardPanel : MonoBehaviour
{
    [Header("要求从左到右，从上到下排列")]
    [SerializeField]
    List<GameObject> gridList;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
        GameSystem.Instance.onBoardChange += OnBoardChange;

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

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
        GameSystem.Instance.onBoardChange -= OnBoardChange;
    }
}
