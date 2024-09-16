using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
    }

    void OnGameStateChange(GameState state)
    {
        if ((state == GameState.CrossWinner) || (state == GameState.CircleWinner) || (state == GameState.Draw))
        {
            animator.Play("ready_twinkle");
        }
        else
        {
            animator.Play("idle");
        }
    }

    private void OnDestroy()
    {
        GameSystem.Instance.onGameStateChange -= OnGameStateChange;
    }
}
