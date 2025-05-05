using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEgg : MonoBehaviour
{
    [Header("References")]
    public GameObject chill;
    public GameObject egg1;
    public GameObject egg2;
    public GameObject egg3;

    private void Update() {
        if(!egg1.activeSelf && !egg2.activeSelf && !egg3.activeSelf)
            chill.gameObject.SetActive(true);
    }

}
