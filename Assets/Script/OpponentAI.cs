using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.UI;
using UnityEngine;
using static UnityEditor.Progress;

public class OpponentAI : MonoBehaviour
{
    [Header("ģ��ȴ�ʱ��")]
    public float waitTime = 2.0f;
    [Header("��ˮ����")]
    public float randomRate = 0.5f;
    bool inThinking = false;
    Coroutine coroutine;
    List<List<ChessType>> boardModelCopy;

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
        boardModelCopy = GameSystem.GenerateEmptyBoardModel();
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
        GameSystem.Instance.onBoardChange += OnBoardChange;
    }

    void OnBoardChange()
    {
        //��ʱ������������
        var board_model = GameSystem.Instance.boardModel;
        int count = GameSystem.rowColCount;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                boardModelCopy[i][j] = board_model[i][j];
            }
        }
    }

    void OnGameStateChange(GameState state)
    {
        if ((state == GameState.CrossTurn && GetCamp() == ChessType.Cross) || (state == GameState.CircleTurn && GetCamp() == ChessType.Circle))
        {
            if (inThinking)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(PlaceAPieace());
        }
    }

    IEnumerator PlaceAPieace()
    {
        KeyValuePair<int, int> place;
        if (UnityEngine.Random.value < randomRate)  //��randomRate�ĸ��ʷ�ˮ
        {
            //��ˮʱ�������
            place = PlaceRandomly();
        }
        else
        {
            //������Minimax�㷨ѡ�����Ž�
            place = PlaceSeriously();
        }
        //ģ��ȴ�ʱ��
        yield return new WaitForSeconds(waitTime);
        //����
        GameSystem.Instance.PlaceAPieace(place.Key, place.Value);
    }

    /// <summary>
    /// �������
    /// </summary>
    KeyValuePair<int, int> PlaceRandomly()
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
        return locations[index];
    }

    /// <summary>
    /// �������
    /// </summary>
    KeyValuePair<int, int> PlaceSeriously()
    {
        int bestScore = int.MinValue;
        KeyValuePair<int, int> place = new KeyValuePair<int, int>(0, 0);
        int count = GameSystem.rowColCount;
        ChessType ai_chess_type = GetCamp();
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if (boardModelCopy[i][j] == ChessType.Empty)
                {
                    boardModelCopy[i][j] = ai_chess_type;
                    int score = Minimax(0, false);
                    boardModelCopy[i][j] = ChessType.Empty;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        place = new KeyValuePair<int, int>(i, j);
                    }
                }
            }
        }
        return place;
    }

    /// <summary>
    /// minimax�㷨
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="isMaximizing"></param>
    /// <returns></returns>
    private int Minimax(int depth, bool isMaximizing)
    {
        ChessType ai_chess_type = GetCamp();
        ChessType player_chess_type = GameSystem.Instance.playerCamp;

        var result = GameSystem.Judging(boardModelCopy);
        if (result.resultType == TurnResultType.CrossWinner)
        {
            if (ai_chess_type == ChessType.Cross) return 1;
            else return -1;
        }
        else if (result.resultType == TurnResultType.CircleWinner)
        {
            if (ai_chess_type == ChessType.Circle) return 1;
            else return -1;
        }
        else if (result.resultType == TurnResultType.Draw)
        {
            return 0;
        }

        int count = GameSystem.rowColCount;

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (boardModelCopy[i][j] == ChessType.Empty)
                    {
                        boardModelCopy[i][j] = ai_chess_type;  // ������һ��
                        int score = Minimax(depth + 1, false);
                        boardModelCopy[i][j] = ChessType.Empty;   // ������һ��
                        bestScore = Math.Max(score, bestScore); // ѡ�����÷�
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (boardModelCopy[i][j] == ChessType.Empty)
                    {
                        boardModelCopy[i][j] = player_chess_type;  // ģ����ҳ�����һ��
                        int score = Minimax(depth + 1, true);
                        boardModelCopy[i][j] = ChessType.Empty;   // ������һ��
                        bestScore = Math.Min(score, bestScore); // ѡ����С�÷�
                    }
                }
            }
            return bestScore;
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
        GameSystem.Instance.onBoardChange -= OnBoardChange;
    }
}
