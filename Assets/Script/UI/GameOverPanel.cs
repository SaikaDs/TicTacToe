using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    GameObject cross;
    [SerializeField]
    GameObject circle;
    [SerializeField]
    GameObject winnerText;
    [SerializeField]
    GameObject drawText;
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    AudioClip winAudio;
    [SerializeField]
    AudioClip loseAudio;
    [SerializeField]
    AudioClip drawAudio;

    AudioSource audioSource;
    const float gameOverAudioDelayTime = 1.0f;

    private void Awake()
    {
        GameSystem.Instance.onGameStateChange += OnGameStateChange;
        audioSource = GetComponent<AudioSource>();
    }


    void OnGameStateChange(GameState state)
    {
        if (state == GameState.CrossWinner)
        {
            gameObject.SetActive(true);
            cross.SetActive(true);
            circle.SetActive(false);
            winnerText.SetActive(true);
            drawText.SetActive(false);
            if (GameSystem.Instance.isTwoPlayers)
            {
                text.gameObject.SetActive(false);
                audioSource.clip = winAudio;
                audioSource.PlayDelayed(gameOverAudioDelayTime);
            }
            else
            {
                text.gameObject.SetActive(true);
                if (GameSystem.Instance.playerCamp == ChessType.Cross)
                {
                    text.text = "你赢了";
                    audioSource.clip = winAudio;
                    audioSource.PlayDelayed(gameOverAudioDelayTime);

                }
                else
                {
                    text.text = "你输了";
                    audioSource.clip = loseAudio;
                    audioSource.PlayDelayed(gameOverAudioDelayTime);
                }
            }

        }
        else if (state == GameState.CircleWinner)
        {
            gameObject.SetActive(true);
            cross.SetActive(false);
            circle.SetActive(true);
            winnerText.SetActive(true);
            drawText.SetActive(false);
            text.gameObject.SetActive(true);
            if (GameSystem.Instance.isTwoPlayers)
            {
                text.gameObject.SetActive(false);
                audioSource.clip = winAudio;
                audioSource.PlayDelayed(gameOverAudioDelayTime);
            }
            else
            {
                if (GameSystem.Instance.playerCamp == ChessType.Circle)
                {
                    text.text = "你赢了";
                    audioSource.clip = winAudio;
                    audioSource.PlayDelayed(gameOverAudioDelayTime);
                }
                else
                {
                    text.text = "你输了";
                    audioSource.clip = loseAudio;
                    audioSource.PlayDelayed(gameOverAudioDelayTime);
                }
            }
        }
        else if (state == GameState.Draw)
        {
            winnerText.SetActive(false);
            drawText.SetActive(true);
            text.gameObject.SetActive(false);
            gameObject.SetActive(true);
            cross.SetActive(false);
            circle.SetActive(false);
            text.text = "平局";
            audioSource.clip = drawAudio;
            audioSource.PlayDelayed(gameOverAudioDelayTime);
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
