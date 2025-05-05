using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("References")]
    public Light flickeringLight;
    public float minIntensity;
    public float maxIntensity;
    public float minFlickerTime;
    public float maxFlickerTime;
    public float changeLight;

    private float originalIntensity;
    private bool flickering;

    private void Start()
    {
        originalIntensity = flickeringLight.intensity;
        flickering = false;
    }

    private void Update()
    {
        float intervale = Random.Range(0f, 1000f);

        if (intervale <= 0.1f && !flickering)
            StartCoroutine(Flicker());


    }

    private void MeltedLight()
    {
        float intervale = Random.Range(0f, 100f);
        if (intervale <= 1f)
        {
            gameObject.SetActive(false);
        }
            
    }

    private IEnumerator Flicker()
    {
        flickering = true;

        while (changeLight < 10f)
        {
            flickeringLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(Random.Range(minFlickerTime / 2f, maxFlickerTime / 2f));
            changeLight += 1f;
        }
        MeltedLight();
        changeLight = 0f;
        flickeringLight.intensity = originalIntensity;
        flickering = false;
    }
}
