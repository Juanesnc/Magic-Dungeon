using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsActive : MonoBehaviour
{
    [Header("References")]
    public GameObject lightScene;
    public GameObject particle;
    
    public void ActivateLights() {
        lightScene.SetActive(true);
        particle.SetActive(true);
    }
}
