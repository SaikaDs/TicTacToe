using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCampPanel : MonoBehaviour
{
    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
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

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
