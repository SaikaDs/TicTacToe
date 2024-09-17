using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCampPanel : MonoBehaviour
{
    [SerializeField]
    GameObject selectCampParent;
    [SerializeField]
    GameObject justStartButton;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
        GameSystem.Instance.onTwoPlayersChange += OnTwoPlayersChange;
    }

    private void OnEnable()
    {
        RefreshState();
    }

    public void SelectCross()
    {
        GameSystem.Instance.ChooseCamp(ChessType.Cross);
    }

    public void SelectCircle()
    {
        GameSystem.Instance.ChooseCamp(ChessType.Circle);
    }

    void OnGameStateChange(GameState state)
    {
        if (state == GameState.Ready)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnTwoPlayersChange()
    {
        RefreshState();
    }

    void RefreshState()
    {
        bool is_two = GameSystem.Instance.isTwoPlayers;
        selectCampParent.SetActive(!is_two);
        justStartButton.SetActive(is_two);
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
        GameSystem.Instance.onTwoPlayersChange -= OnTwoPlayersChange;
    }
}
