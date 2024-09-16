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
        dropdown.interactable = state == GameState.Ready;
    }

    void OnDropdownValueChanged(int index)
    {
        // ����index������Ӧ�ķ���
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
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
