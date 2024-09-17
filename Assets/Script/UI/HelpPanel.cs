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

    // ������뿪�ؼ�ʱ���õķ���
    public void OnPointerExit(PointerEventData eventData)
    {
        help.SetActive(false);
    }
}