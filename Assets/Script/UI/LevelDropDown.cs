using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDropDown : MonoBehaviour
{
    [SerializeField]
    OpponentAI opponentAI;
    TMP_Dropdown dropdown;
    [SerializeField]
    GameObject title;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
    }

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnGameStateChange(GameState state)
    {
        bool ready = state == GameState.Ready;
        dropdown.interactable = ready;
        title.SetActive(ready);
    }

    void OnDropdownValueChanged(int index)
    {
        GameSystem.Instance.isTwoPlayers = false;
        // 根据index调用相应的方法
        switch (index)
        {
            case 0:
                opponentAI.SetLevel(AILevel.Easy);
                break;
            case 1:
                opponentAI.SetLevel(AILevel.Medium);
                break;
            case 2:
                opponentAI.SetLevel(AILevel.Hard);
                break;
            case 3:
                GameSystem.Instance.isTwoPlayers = true;
                break;
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
