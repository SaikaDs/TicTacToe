using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    GameObject cross;
    [SerializeField]
    GameObject circle;
    [SerializeField]
    GameObject winnerText;
    [SerializeField]
    GameObject drawText;
    [SerializeField]
    TextMeshProUGUI text;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
    }


    void OnGameStateChange(GameState state)
    {
        if (state == GameState.CrossWinner)
        {
            gameObject.SetActive(true);
            cross.SetActive(true);
            circle.SetActive(false);
            winnerText.SetActive(true);
            drawText.SetActive(false);
            text.gameObject.SetActive(true);
            if (GameSystem.Instance.playerCamp == ChessType.Cross)
            {
                text.text = "你赢了";
            }
            else
            {
                text.text = "你输了";
            }
        }
        else if (state == GameState.CircleWinner)
        {
            gameObject.SetActive(true);
            cross.SetActive(false);
            circle.SetActive(true);
            winnerText.SetActive(true);
            drawText.SetActive(false);
            text.gameObject.SetActive(true);
            if (GameSystem.Instance.playerCamp == ChessType.Circle)
            {
                text.text = "你赢了";
            }
            else
            {
                text.text = "你输了";
            }
        }
        else if (state == GameState.Draw)
        {
            winnerText.SetActive(false);
            drawText.SetActive(true);
            text.gameObject.SetActive(false);
            gameObject.SetActive(true);
            cross.SetActive(false);
            circle.SetActive(false);
            text.text = "平局";
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
