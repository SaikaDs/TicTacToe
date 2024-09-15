using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class OpponentAI : MonoBehaviour
{
    [Header("ģ��ȴ�ʱ��")]
    public float waitTime = 2.0f;

    ChessType GetCamp()
    {
        if (GameSystem.Instance.playerCamp == ChessType.Circle)
        {
            return ChessType.Cross;
        }
        else
        {
            return ChessType.Circle;
        }
    }

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
    }

    void OnGameStateChange(GameState state)
    {
        if ((state == GameState.CrossTurn && GetCamp() == ChessType.Cross) || (state == GameState.CircleTurn && GetCamp() == ChessType.Circle))
        {
            Invoke("PlaceAPieace", waitTime);
        }
    }

    void PlaceAPieace()
    {
        var board_model = GameSystem.Instance.boardModel;
        int count = GameSystem.rowColCount;
        List<KeyValuePair<int, int>> locations = new();
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                ChessType curr_type = board_model[i][j];
                if (curr_type == ChessType.Empty)
                {
                    locations.Add(new KeyValuePair<int, int>(i, j));
                }
            }
        }
        //���ѡ��һ��location
        var random = new System.Random();
        // �� List �����ѡ��һ��Ԫ��
        int index = random.Next(locations.Count);  // ��ȡһ���� 0 �� locations.Count - 1 ���������
        GameSystem.Instance.PlaceAPieace(locations[index].Key, locations[index].Value);
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
