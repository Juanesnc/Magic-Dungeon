using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenDoor : MonoBehaviour
{
    PlayerMovement pm;
    Animator anim;
    void Start()
    {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        pm.points += 1000;
        StartCoroutine(QuitarEscombros());
    }

    private void DesactivateDoor() {
        foreach(Transform child in transform)
        {
            if(child.name.Contains("Plane.0") && child.name != "Plane.016")
                child.gameObject.SetActive(false);
        }
    }

    private IEnumerator QuitarEscombros() {
        
        yield return new WaitForSeconds(10f);
        DesactivateDoor();
    }
}
