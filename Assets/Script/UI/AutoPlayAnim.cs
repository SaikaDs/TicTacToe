using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayAnim : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Animator>().Play("show");
    }
}
