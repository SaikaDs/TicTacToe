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


public enum TurnResultType
{
    CrossWinner,
    CircleWinner,
    Draw,
    Continue,
}

public struct TurnResult
{
    public TurnResultType resultType;
    public ConnectedType connectedType;  //如果已连成线，指示连线情况
    public int num;  //如果是横竖连线，指定行号/列号

    public void SetResultByChessType(ChessType chess_type)
    {
        if (chess_type == ChessType.Cross)
        {
            resultType = TurnResultType.CrossWinner;
        }
        else if (chess_type == ChessType.Circle)
        {
            resultType = TurnResultType.CircleWinner;
        }
        else
        {
            Debug.LogError(string.Format("invalid chess_type in SetResultByChessType: {0}", chess_type));
        }
    }
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
    /// 当已连成线，广播连线信息，参数0指示阵营，参数1指示类型，参数2指示对应行号/列号
    /// </summary>
    public Action<ChessType, ConnectedType, int> onConnect;

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

    bool _isTwoPlayers;


    public Action onTwoPlayersChange;
    public bool isTwoPlayers
    {
        get { return _isTwoPlayers; }
        set
        {
            bool need_signal = _isTwoPlayers != value;
            _isTwoPlayers = value;
            if (need_signal) onTwoPlayersChange?.Invoke();
        }
    }

    private void Awake()
    {
        Instance = this;
        _isTwoPlayers = false;
        boardModel = GenerateEmptyBoardModel();
    }

    void Start()
    {
        RestartGame();
    }

    public static List<List<ChessType>> GenerateEmptyBoardModel()
    {
        List<List<ChessType>> board = new List<List<ChessType>>();
        board.Capacity = rowColCount;
        for (int i = 0; i < rowColCount; i++)
        {
            List<ChessType> row = new List<ChessType>();
            row.Capacity = rowColCount;
            for (int j = 0; j < rowColCount; j++)
            {
                row.Add(ChessType.Empty);
            }
            board.Add(row);
        }
        return board;
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
        if (target_type != ChessType.Empty)
        {
            Debug.LogError(string.Format("PlaceAPieace at non-empty place {0} {1}", row, col));
            return;
        }
        //落子
        boardModel[row][col] = chess_type;
        //广播
        onBoardChange?.Invoke();
        //推进进程（胜、负、平或者换人下）
        PushGameState();
    }

    void PushGameState()
    {
        var result = Judging(boardModel);
        if (result.resultType == TurnResultType.CircleWinner)
        {
            onConnect?.Invoke(ChessType.Circle, result.connectedType, result.num);
            state = GameState.CircleWinner;
            return;
        }
        else if (result.resultType == TurnResultType.CrossWinner)
        {
            onConnect?.Invoke(ChessType.Cross, result.connectedType, result.num);
            state = GameState.CrossWinner;
            return;
        }
        else if (result.resultType == TurnResultType.Draw)
        {
            state = GameState.Draw;
            return;
        }
        //下一回合
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
    }

    public static TurnResult Judging(List<List<ChessType>> board)
    {
        bool has_empty = false;  // 判平局用
        ChessType flagged_chess_type;
        bool connected;
        #region 遍历行和判断是否已下满（不break，全量遍历）
        for (int i = 0; i < rowColCount; i++)
        {
            flagged_chess_type = board[i][0];
            connected = true;
            for (int j = 0; j < rowColCount; j++)
            {
                var chess_type = board[i][j];
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
                var result = new TurnResult
                {
                    connectedType = ConnectedType.Row,
                    num = i,
                };
                result.SetResultByChessType(flagged_chess_type);
                return result;
            }
        }
        #endregion
        #region 遍历列
        for (int i = 0; i < rowColCount; i++)
        {
            flagged_chess_type = board[0][i];
            connected = true;
            for (int j = 0; j < rowColCount; j++)
            {
                var chess_type = board[j][i];
                if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
                {
                    connected = false;
                    break;
                }
            }
            if (connected)
            {
                var result = new TurnResult
                {
                    connectedType = ConnectedType.Col,
                    num = i,
                };
                result.SetResultByChessType(flagged_chess_type);
                return result;
            }
        }
        #endregion
        #region 查看正对角线
        flagged_chess_type = board[0][0];
        connected = true;
        for (int i = 0; i < rowColCount; i++)
        {
            var chess_type = board[i][i];
            if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
            {
                connected = false;
                break;
            }
        }
        if (connected)
        {
            var result = new TurnResult
            {
                connectedType = ConnectedType.Diagonal,
            };
            result.SetResultByChessType(flagged_chess_type);
            return result;
        }
        #endregion
        #region 查看反对角线
        flagged_chess_type = board[rowColCount - 1][0];
        connected = true;
        for (int i = 0; i < rowColCount; i++)
        {
            var chess_type = board[rowColCount - 1 - i][i];
            if (chess_type == ChessType.Empty || chess_type != flagged_chess_type)
            {
                connected = false;
                break;
            }
        }
        if (connected)
        {
            var result = new TurnResult
            {
                connectedType = ConnectedType.AntiDiagonal,
            };
            result.SetResultByChessType(flagged_chess_type);
            return result;
        }
        #endregion
        #region 判平局
        if (!has_empty)
        {
            var result = new TurnResult
            {
                resultType = TurnResultType.Draw,
            };
            return result;
        }
        #endregion
        return new TurnResult
        {
            resultType = TurnResultType.Continue,
        };
    }
}
