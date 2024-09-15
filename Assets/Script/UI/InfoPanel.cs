using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    [SerializeField]
    GameObject circle;
    [SerializeField]
    GameObject cross;
    [SerializeField]
    TextMeshProUGUI text;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
    }

    void OnGameStateChange(GameState state)
    {
        circle.SetActive(false);
        cross.SetActive(false);
        switch (state)
        {
            case GameState.Ready:
                text.text = "准备";
                break;
            case GameState.CrossTurn:
                cross.SetActive(true);
                if (GameSystem.Instance.playerCamp == ChessType.Cross)
                {
                    text.text = "     轮到你了";
                }
                else
                {
                    text.text = "     等待对手";
                }
                break;
            case GameState.CircleTurn:
                circle.SetActive(true);
                if (GameSystem.Instance.playerCamp == ChessType.Circle)
                {
                    text.text = "     轮到你了";
                }
                else
                {
                    text.text = "     等待对手";
                }
                break;
            case GameState.CrossWinner:
                if (GameSystem.Instance.playerCamp == ChessType.Cross)
                {
                    text.text = "恭喜！";
                }
                else
                {
                    text.text = "别灰心！";
                }
                break;
            case GameState.CircleWinner:
                if (GameSystem.Instance.playerCamp == ChessType.Circle)
                {
                    text.text = "恭喜！";
                }
                else
                {
                    text.text = "别灰心！";
                }
                break;
            case GameState.Draw:
                text.text = "再来一把！";
                break;
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
