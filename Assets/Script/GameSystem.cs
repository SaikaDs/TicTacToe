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
    Cross,  // ����
    Circle,
}

public enum ConnectedType
{
    Row,
    Col,
    Diagonal,  //���Խ��ߣ����ϵ����£�
    AntiDiagonal,  //���Խ��ߣ����ϵ����£�
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    /// <summary>
    /// ������
    /// </summary>
    public const int rowColCount = 3;

    /// <summary>
    /// ����ģ��
    /// </summary>
    public List<List<ChessType>> boardModel { get; private set; }

    /// <summary>
    /// ����״̬�ı䣨��ˢ�£�
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
    /// ��Ϸ���̸ı�
    /// </summary>
    public Action<GameState> onGameStateChange;

    /// <summary>
    /// �����ѡ����Ӫ
    /// </summary>
    public Action<ChessType> onChooseCamp;

    /// <summary>
    /// ���������ߣ��㲥������Ϣ������1ָʾ���ͣ�����2ָʾ��Ӧ�к�/�к�
    /// </summary>
    public Action<ConnectedType, int> onConnect;

    /// <summary>
    /// �����Ӫ
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
        //��������
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
    /// ���ѡ����Ӫ
    /// </summary>
    public void ChooseCamp(ChessType chess_type)
    {
        playerCamp = chess_type;
        onChooseCamp?.Invoke(chess_type);
        state = GameState.CrossTurn;
    }

    public void PlaceAPieace(int row, int col)
    {
        //���״̬
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
        //���Ŀ����Ƿ���Ч
        var target_type = boardModel[row][col];
        if (target_type != ChessType.Empty) return;
        //����
        boardModel[row][col] = chess_type;
        //�㲥
        onBoardChange?.Invoke();
        //�ƽ����̣�ʤ������ƽ���߻����£�
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
            state = GameState.CircleWinner;  //��ֹ����
        }
    }

    void PushGameState()
    {
        bool has_empty = false;  // ��ƽ����
        ChessType flagged_chess_type;
        bool connected;
        #region �����к��ж��Ƿ�����������break��ȫ��������
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
        #region ������
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
        #region �鿴���Խ���
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
        #region �鿴���Խ���
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
        #region ��ƽ��
        if (!has_empty)
        {
            state = GameState.Draw;
            return;
        }
        #endregion
        #region ��һ�غ�
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
            state = GameState.CrossTurn;  //��ֹ����
        }
        #endregion
    }
}
