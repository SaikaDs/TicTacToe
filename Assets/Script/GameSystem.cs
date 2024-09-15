using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public enum GameState
{
    Ready,
    CrossTurn,
    CircleTurn,
    CrossWinner,
    CircleWinner,
    Draw,
}

public enum ChessType
{
    Empty,
    Cross,  // 先手
    Circle,
}

public enum ConnectedType
{
    Row,
    Col,
    Diagonal,  //正对角线（左上到右下）
    AntiDiagonal,  //反对角线（右上到左下）
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    /// <summary>
    /// 行列数
    /// </summary>
    public const int rowColCount = 3;

    /// <summary>
    /// 棋盘模型
    /// </summary>
    public List<List<ChessType>> boardModel { get; private set; }

    /// <summary>
    /// 棋盘状态改变（需刷新）
    /// </summary>
    public Action onBoardChange;

    GameState _state;
    public GameState state
    {
        get { return _state; }
        private set
        {
            _state = value;
            onGameStateChange?.Invoke(_state);
        }
    }
    /// <summary>
    /// 游戏进程改变
    /// </summary>
    public Action<GameState> onGameStateChange;

    /// <summary>
    /// 当玩家选择阵营
    /// </summary>
    public Action<ChessType> onChooseCamp;

    /// <summary>
    /// 当已连成线，广播连线信息，参数1指示类型，参数2指示对应行号/列号
    /// </summary>
    public Action<ConnectedType, int> onConnect;

    /// <summary>
    /// 玩家阵营
    /// </summary>
    [SerializeField]
    ChessType _playerCamp;
    public ChessType playerCamp
    {
        get { return _playerCamp; }
        private set { _playerCamp = value; }
    }

    private void Awake()
    {
        Instance = this;
        boardModel = new List<List<ChessType>>();
        boardModel.Capacity = rowColCount;
        for (int i = 0; i < rowColCount; i++)
        {
            List<ChessType> row = new List<ChessType>();
            row.Capacity = rowColCount;
            for (int j = 0; j < rowColCount; j++)
            {
                row.Add(ChessType.Empty);
            }
            boardModel.Add(row);
        }
    }

    void Start()
    {
        RestartGame();
    }

    public void RestartGame()
    {
        //清理棋盘
        foreach (var item in boardModel)
        {
            for (int i = 0; i < item.Count; i++) 
            {
                item[i] = ChessType.Empty;
            }
        }
        onBoardChange?.Invoke();
        state = GameState.Ready;
    }

    /// <summary>
    /// 玩家选择阵营
    /// </summary>
    public void ChooseCamp(ChessType chess_type)
    {
        playerCamp = chess_type;
        onChooseCamp?.Invoke(chess_type);
        state = GameState.CrossTurn;
    }

    public void PlaceAPieace(int row, int col)
    {
        //检查状态
        ChessType chess_type;
        if (state == GameState.CrossTurn)
        {
            chess_type = ChessType.Cross;
        }
        else if (state == GameState.CircleTurn) 
        {
            chess_type = ChessType.Circle;
        }
        else
        {
            Debug.LogError(string.Format("wrong call of PlaceAPieace at {0}", state));
            return;
        }
        //检查目标点是否有效
        var target_type = boardModel[row][col];
        if (target_type != ChessType.Empty) return;
        //落子
        boardModel[row][col] = chess_type;
        //广播
        onBoardChange?.Invoke();
        //推进进程（胜、负、平或者换人下）
        PushGameState();
    }

    void ChangeStateByWinnerChessType(ChessType chess_type)
    {
        if (chess_type == ChessType.Cross)
        {
            state = GameState.CrossWinner;
        }
        else if (chess_type == ChessType.Circle)
        {
            state = GameState.CircleWinner;
        }
        else
        {
            Debug.LogError(string.Format("Invalid Chess Type Connected {0}", chess_type));
            state = GameState.CircleWinner;  //防止阻塞
        }
    }

    void PushGameState()
    {
        bool has_empty = false;  // 判平局用
        ChessType flagged_chess_type;
        bool connected;
        #region 遍历行和判断是否已下满（不break，全量遍历）
        for (int i = 0; i < rowColCount; i++)
        {
            flagged_chess_type = boardModel[i][0];
            connected = true;
            for (int j = 0; j < rowColCount; j++)
            {
                var chess_type = boardModel[i][j];
                if (chess_type == ChessType.Empty)
                {
                    has_empty = true;
                }
                if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
                {
                    connected = false;
                }
            }
            if (connected)
            {
                onConnect?.Invoke(ConnectedType.Row, i);
                ChangeStateByWinnerChessType(flagged_chess_type);
                return;
            }
        }
        #endregion
        #region 遍历列
        for (int i = 0; i < rowColCount; i++)
        {
            flagged_chess_type = boardModel[0][i];
            connected = true;
            for (int j = 0; j < rowColCount; j++)
            {
                var chess_type = boardModel[j][i];
                if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
                {
                    connected = false;
                    break;
                }
            }
            if (connected)
            {
                onConnect?.Invoke(ConnectedType.Col, i);
                ChangeStateByWinnerChessType(flagged_chess_type);
                return;
            }
        }
        #endregion
        #region 查看正对角线
        flagged_chess_type = boardModel[0][0];
        connected = true;
        for (int i = 0; i < rowColCount; i++)
        {
            var chess_type = boardModel[i][i];
            if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
            {
                connected = false;
                break;
            }
        }
        if (connected)
        {
            onConnect?.Invoke(ConnectedType.Diagonal, 0);
            ChangeStateByWinnerChessType(flagged_chess_type);
            return;
        }
        #endregion
        #region 查看反对角线
        flagged_chess_type = boardModel[rowColCount - 1][0];
        connected = true;
        for (int i = 0; i < rowColCount; i++)
        {
            var chess_type = boardModel[rowColCount - 1 - i][i];
            if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
            {
                connected = false;
                break;
            }
        }
        if (connected)
        {
            onConnect?.Invoke(ConnectedType.AntiDiagonal, 0);
            ChangeStateByWinnerChessType(flagged_chess_type);
            return;
        }
        #endregion
        #region 判平局
        if (!has_empty)
        {
            state = GameState.Draw;
            return;
        }
        #endregion
        #region 下一回合
        if (state == GameState.CrossTurn)
        {
            state = GameState.CircleTurn;
        }
        else if (state == GameState.CircleTurn)
        {
            state = GameState.CrossTurn;
        }
        else
        {
            Debug.LogError(string.Format("Invalid Game State {0}", state));
            state = GameState.CrossTurn;  //防止阻塞
        }
        #endregion
    }
}
