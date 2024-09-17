using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelpPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    GameObject help;

    private void OnEnable()
    {
        help.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        help.SetActive(true);
    }

    // 当鼠标离开控件时调用的方法
    public void OnPointerExit(PointerEventData eventData)
    {
        help.SetActive(false);
    }
}