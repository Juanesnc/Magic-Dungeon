using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitObjects : MonoBehaviour
{
    [Header("References")]
    public int resistencia;
    public Material destruido;
    public enum TypeBox { normal, reforzed, chain, boxChain, egg }
    public TypeBox typeBox;
    public GameObject ObjectFall;
    public GameObject chain1;
    public GameObject chain2;
    public AudioSource audioSource;

    PlayerMovement pm;
    Animator anim;
    BoxCollider bc;
    Rigidbody rb;
    Renderer[] renderers;

    void Start()
    {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();

        if (typeBox != TypeBox.chain && typeBox != TypeBox.boxChain)
        {
            rb = GetComponent<Rigidbody>();
            bc = GetComponent<BoxCollider>();
        }
        anim = GetComponent<Animator>();
        renderers = GetComponentsInChildren<Renderer>();

    }

    private void Update()
    {
        if (typeBox == TypeBox.boxChain)
        {

            int count = CountCollider(chain1) / 2 + CountCollider(chain2) / 2;

            if (count <= 0)
                ObjectFall.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private int CountCollider(GameObject chain)
    {

        int activechain = resistencia;

        BoxCollider collider = chain.GetComponent<BoxCollider>();

        if (!collider.enabled)
        {
            activechain--;
        }

        return activechain;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (typeBox == TypeBox.reforzed)
        {
            if (other.CompareTag("DisparoCargado"))
            {
                if (resistencia <= 1)
                {
                    StartCoroutine(DesactivatedObject());
                }
                else
                {
                    resistencia--;
                    anim.Play("Golpe", 0, 0.0f);
                    anim.SetBool("Disparo", true);
                }
            }
        }
        else if (typeBox == TypeBox.normal)
        {
            if (other.CompareTag("DisparoCargado"))
            {
                StartCoroutine(DesactivatedObject());
            }
            else if (other.CompareTag("Disparo"))
            {
                if (resistencia <= 1)
                {
                    pm.points += 100;
                    audioSource.Play();
                    StartCoroutine(DesactivatedObject());
                }
                else
                {
                    resistencia--;
                    anim.Play("Golpe", 0, 0.0f);
                    anim.SetBool("Disparo", true);
                }
            }
        }
        else if (typeBox == TypeBox.chain)
        {
            pm.points += 100;
            anim.Play("Destruido", 0, 0.0f);
            anim.SetBool("Disparo", true);
            BoxCollider collider = GetComponent<BoxCollider>();
            collider.enabled = false;

            StartCoroutine(DejarCaerObjeto());
        }
        else if (typeBox == TypeBox.egg)
        {
            StartCoroutine(DesactivatedObject());
        }
    }

    public IEnumerator DesactivatedObject()
    {
        if (typeBox != TypeBox.egg)
        {
            rb.mass = 0.01f;

            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = destruido;
            }

            yield return new WaitForSeconds(2f);
        }

        gameObject.SetActive(false);
    }

    public IEnumerator DejarCaerObjeto()
    {

        foreach (Renderer renderer in renderers)
        {
            if (renderer.name.Contains("Cadena.0") && renderer.name != "Cadenas")
                renderer.sharedMaterial = destruido;
        }

        yield return new WaitForSeconds(2f);

        foreach (Transform child in transform)
        {
            if (child.name.Contains("Cadena.0") && child.name != "Cadena_cell")
                child.gameObject.SetActive(false);
        }
    }
}
