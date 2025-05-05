using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDoor : MonoBehaviour
{
    [Header("References")]
    public GameObject puerta;
    public GameObject boos;
    public bool newDoor;
    public bool isFinalDoor;

    Animator anim;
    public int resistencia;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (newDoor)
        {
            anim.SetBool("Disparo", true);
        }
    }

    private void Update() {

        if(!boos && isFinalDoor)
        {
            Debug.Log("Final");
            gameObject.SetActive(false);
            puerta.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFinalDoor)
        {
            if (resistencia <= 0)
            {
                gameObject.SetActive(false);
                puerta.SetActive(true);
            }
            else
            {
                resistencia--;
                anim.Play("GolpePuerta", 0, 0.0f);
                anim.SetBool("Disparo", true);
            }
        }
    }
}
