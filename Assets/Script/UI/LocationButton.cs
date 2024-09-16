using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocationButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    GameObject circle;
    [SerializeField]
    GameObject cross;

    private void OnEnable()
    {
        circle.SetActive(false);
        cross.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameSystem.Instance.state == GameState.CircleTurn)
        {
            circle.SetActive(true);
        }
        if (GameSystem.Instance.state == GameState.CrossTurn)
        {
            cross.SetActive(true);
        }
    }

    // ������뿪�ؼ�ʱ���õķ���
    public void OnPointerExit(PointerEventData eventData)
    {
        circle.SetActive(false);
        cross.SetActive(false);
    }
}
