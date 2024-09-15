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
                text.text = "׼��";
                break;
            case GameState.CrossTurn:
                cross.SetActive(true);
                if (GameSystem.Instance.playerCamp == ChessType.Cross)
                {
                    text.text = "     �ֵ�����";
                }
                else
                {
                    text.text = "     �ȴ�����";
                }
                break;
            case GameState.CircleTurn:
                circle.SetActive(true);
                if (GameSystem.Instance.playerCamp == ChessType.Circle)
                {
                    text.text = "     �ֵ�����";
                }
                else
                {
                    text.text = "     �ȴ�����";
                }
                break;
            case GameState.CrossWinner:
                if (GameSystem.Instance.playerCamp == ChessType.Cross)
                {
                    text.text = "��ϲ��";
                }
                else
                {
                    text.text = "����ģ�";
                }
                break;
            case GameState.CircleWinner:
                if (GameSystem.Instance.playerCamp == ChessType.Circle)
                {
                    text.text = "��ϲ��";
                }
                else
                {
                    text.text = "����ģ�";
                }
                break;
            case GameState.Draw:
                text.text = "����һ�ѣ�";
                break;
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
