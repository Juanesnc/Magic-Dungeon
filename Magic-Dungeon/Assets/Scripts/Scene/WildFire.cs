using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildFire : MonoBehaviour
{
    [Header("References")]
    public GameObject fireObject;
    public ParticleSystem particle;
    public Light lightObject;

    PlayerMovement pm;

    private void Start()
    {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    public void DesactiveWildFire()
    {
        StartCoroutine(QuitarParticulas());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fireObject)
            {
                if (!fireObject.activeSelf)
                {
                    fireObject.SetActive(true);
                    pm.points += 50;
                }
            }

            StartCoroutine(QuitarParticulas());
        }
    }

    private IEnumerator QuitarParticulas()
    {
        particle.Stop();

        float elapsedTime = 0f;

        while (elapsedTime <= 2f)
        {
            elapsedTime += Time.deltaTime;
            lightObject.intensity -= 0.005f;
        }
        yield return new WaitForSecondsRealtime(2f);
        lightObject.intensity = 0f;
        gameObject.SetActive(false);
    }
}
