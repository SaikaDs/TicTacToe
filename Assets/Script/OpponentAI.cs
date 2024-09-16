using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public enum AILevel
{
    Easy,  //���Ѷȣ���ȫ������Ҳ��¾�
    Medium,  //�е��Ѷȣ�һ����ʲ�ȡminimax�㷨���һ�����¾�
    Hard,  //�ߵ��Ѷȣ�90%���ʲ�ȡminimax�㷨���һ�����¾�
}

public class OpponentAI : MonoBehaviour
{
    [Header("ģ��ȴ�ʱ��")]
    public float waitTime = 2.0f;
    [Header("�Ѷ�")]
    public AILevel level = AILevel.Easy;
    bool inThinking = false;
    Coroutine coroutine;
    List<List<ChessType>> boardModelCopy;

    /// <summary>
    /// ��ˮ���ʣ����ѶȾ���
    /// </summary>
    float randomRate
    {
        get
        {
            switch(level)
            {
                case AILevel.Easy:
                    return 1.0f;
                case AILevel.Medium:
                    return 0.5f;
                case AILevel.Hard:
                    return 0.1f;
            }
            return 0.5f;
        }
    }

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

    public void SetLevel(AILevel _level)
    {
        level = _level;
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
        if (level > AILevel.Easy)
        {
            //�������Ѷȣ���Ҫ��ȡ������ʩ
            KeyValuePair<int, int> emergency_place;
            bool emergency = PlaceEmergency(out emergency_place);
            if (emergency)
            {
                place = emergency_place;
            }
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
    /// Ӧ�����£������������ߵĿ�ֱ�ӻ�ʤ��ʧ�ܵ����������ȡʤ��¾�
    /// </summary>
    bool PlaceEmergency(out KeyValuePair<int, int> result)
    {
        result = new KeyValuePair<int, int>();
        //���ж��ܷ�ֱ��Ӯ
        bool can_win = FindWinPlace(GetCamp(), out result);
        if (can_win)
        {
            return true;
        }
        //���ж��Ƿ��ֱ����
        bool will_lose = FindWinPlace(GameSystem.Instance.playerCamp, out result);
        if (will_lose)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ѱ��
    /// </summary>
    /// <param name="PlaceSeriously"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public bool FindWinPlace(ChessType flagged_chess_type, out KeyValuePair<int, int> result)
    {
        result = new();
        int count = GameSystem.rowColCount;
        bool impossible = false;  //����/��/б���Ѿ�����������
        bool has_empty = false;  //����/��/б���ѳ���һ����λ
        int empty_num = -1;
        #region ������
        for (int i = 0; i < count; i++)
        {
            impossible = false;
            has_empty = false;
            empty_num = -1;
            for (int j = 0; j < count; j++)
            {
                var chess_type = boardModelCopy[i][j];
                if (chess_type == ChessType.Empty)
                {
                    if (has_empty)
                    {
                        //����������λ�ˣ����޿���
                        impossible = true;
                        break;
                    }
                    has_empty = true;
                    empty_num = j;
                }
                else if (chess_type != flagged_chess_type)
                {
                    impossible = true;
                    break;
                }
            }
            if (!impossible && has_empty)
            {
                result = new KeyValuePair<int, int>(i, empty_num);
                return true;
            }
        }
        #endregion
        #region ������
        for (int i = 0; i < count; i++)
        {
            impossible = false;
            has_empty = false;
            empty_num = -1;
            for (int j = 0; j < count; j++)
            {
                var chess_type = boardModelCopy[j][i];
                if (chess_type == ChessType.Empty)
                {
                    if (has_empty)
                    {
                        //����������λ�ˣ����޿���
                        impossible = true;
                        break;
                    }
                    has_empty = true;
                    empty_num = j;
                }
                else if (chess_type != flagged_chess_type)
                {
                    impossible = true;
                    break;
                }
            }
            if (!impossible && has_empty)
            {
                result = new KeyValuePair<int, int>(empty_num, i);
                return true;
            }
        }
        #endregion
        #region �鿴���Խ���
        impossible = false;
        has_empty = false;
        empty_num = -1;
        for (int i = 0; i < count; i++)
        {
            var chess_type = boardModelCopy[i][i];
            if (chess_type == ChessType.Empty)
            {
                if (has_empty)
                {
                    //����������λ�ˣ����޿���
                    impossible = true;
                    break;
                }
                has_empty = true;
                empty_num = i;
            }
            else if (chess_type != flagged_chess_type)
            {
                impossible = true;
                break;
            }
        }
        if (!impossible && has_empty)
        {
            result = new KeyValuePair<int, int>(empty_num, empty_num);
            return true;
        }
        #endregion
        #region �鿴���Խ���
        impossible = false;
        has_empty = false;
        empty_num = -1;
        for (int i = 0; i < count; i++)
        {
            var chess_type = boardModelCopy[count - 1 - i][i];
            if (chess_type == ChessType.Empty)
            {
                if (has_empty)
                {
                    //����������λ�ˣ����޿���
                    impossible = true;
                    break;
                }
                has_empty = true;
                empty_num = i;
            }
            else if (chess_type != flagged_chess_type)
            {
                impossible = true;
                break;
            }
        }
        if (!impossible && has_empty)
        {
            result = new KeyValuePair<int, int>(count - 1 - empty_num, empty_num);
            return true;
        }
        #endregion
        return false;
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
