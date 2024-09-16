using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public enum AILevel
{
    Easy,  //简单难度，完全随机，且不堵救
    Medium,  //中等难度，一半概率采取minimax算法，且会紧急堵救
    Hard,  //高等难度，90%概率采取minimax算法，且会紧急堵救
}

public class OpponentAI : MonoBehaviour
{
    [Header("模拟等待时间")]
    public float waitTime = 2.0f;
    [Header("难度")]
    public AILevel level = AILevel.Easy;
    bool inThinking = false;
    Coroutine coroutine;
    List<List<ChessType>> boardModelCopy;

    /// <summary>
    /// 放水概率，由难度决定
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
        //及时复制棋盘数据
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
        if (UnityEngine.Random.value < randomRate)  //有randomRate的概率放水
        {
            //放水时，随机下
            place = PlaceRandomly();
        }
        else
        {
            //否则，用Minimax算法选择最优解
            place = PlaceSeriously();
        }
        if (level > AILevel.Easy)
        {
            //简单以上难度，就要采取紧急措施
            KeyValuePair<int, int> emergency_place;
            bool emergency = PlaceEmergency(out emergency_place);
            if (emergency)
            {
                place = emergency_place;
            }
        }
        //模拟等待时间
        yield return new WaitForSeconds(waitTime);
        //下棋
        GameSystem.Instance.PlaceAPieace(place.Key, place.Value);
    }

    /// <summary>
    /// 随机地下
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
        //随机选择一个location
        var random = new System.Random();
        // 从 List 中随机选择一个元素
        int index = random.Next(locations.Count);  // 获取一个从 0 到 locations.Count - 1 的随机索引
        return locations[index];
    }

    /// <summary>
    /// 应急地下，出现两子连线的可直接获胜或失败的情况，优先取胜或堵救
    /// </summary>
    bool PlaceEmergency(out KeyValuePair<int, int> result)
    {
        result = new KeyValuePair<int, int>();
        //先判断能否直接赢
        bool can_win = FindWinPlace(GetCamp(), out result);
        if (can_win)
        {
            return true;
        }
        //再判断是否会直接输
        bool will_lose = FindWinPlace(GameSystem.Instance.playerCamp, out result);
        if (will_lose)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 寻找
    /// </summary>
    /// <param name="PlaceSeriously"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public bool FindWinPlace(ChessType flagged_chess_type, out KeyValuePair<int, int> result)
    {
        result = new();
        int count = GameSystem.rowColCount;
        bool impossible = false;  //此行/列/斜线已经不可能连线
        bool has_empty = false;  //此行/列/斜线已出现一个空位
        int empty_num = -1;
        #region 遍历行
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
                        //出现两个空位了，已无可能
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
        #region 遍历列
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
                        //出现两个空位了，已无可能
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
        #region 查看正对角线
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
                    //出现两个空位了，已无可能
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
        #region 查看反对角线
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
                    //出现两个空位了，已无可能
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
    /// 认真地下
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
    /// minimax算法
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
                        boardModelCopy[i][j] = ai_chess_type;  // 尝试这一步
                        int score = Minimax(depth + 1, false);
                        boardModelCopy[i][j] = ChessType.Empty;   // 撤销这一步
                        bestScore = Math.Max(score, bestScore); // 选择最大得分
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
                        boardModelCopy[i][j] = player_chess_type;  // 模拟玩家尝试这一步
                        int score = Minimax(depth + 1, true);
                        boardModelCopy[i][j] = ChessType.Empty;   // 撤销这一步
                        bestScore = Math.Min(score, bestScore); // 选择最小得分
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
