using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlaySound : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<AudioSource>().Play();
    }
}
