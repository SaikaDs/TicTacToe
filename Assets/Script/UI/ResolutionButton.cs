using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionButton : MonoBehaviour
{
    int currIndex = 0;
    [SerializeField]
    TextMeshProUGUI text;

    private void OnEnable()
    {
        Refresh();
    }

    public void ChangeResolution()
    {
        if (currIndex >= 4)
        {
            currIndex = 0;
        }
        else
        {
            currIndex++;
        }
        Refresh();
    }

    void Refresh()
    {
        switch (currIndex)
        {
            case 0:
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                text.text = "分辨率:1280*720";
                break;
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                text.text = "分辨率:1920*1080";
                break;
            case 2:
                Screen.SetResolution(2560, 1440, FullScreenMode.Windowed);
                text.text = "分辨率:2560*1440";
                break;
            case 3:
                Screen.SetResolution(2670, 1200, FullScreenMode.Windowed);
                text.text = "分辨率:2670*1200";
                break;
            case 4:
                Screen.SetResolution(2048, 1536, FullScreenMode.Windowed);
                text.text = "分辨率:2048*1536";
                break;
        }
    }
}
