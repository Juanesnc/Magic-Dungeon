using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Rotador : MonoBehaviour
{
    [Header("References")]
    public float speed = 0.5f;
    public float minY = 0.2f;
    public float maxY = 0.5f;
    public GameObject armaPrincipal;
    public Image imageMana;
    public bool isFinal;
    public Image imageFinal;
    public TextMeshProUGUI textMeshPro;
    public Image imageChangeButton;

    private float originalX;
    private float originalZ;
    private bool movingUp = true;
    private PlayerMovement pm;

    void Start()
    {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
        originalX = transform.position.x;
        originalZ = transform.position.z;
    }

    // Update is called once per framesa
    void Update()
    {
        transform.Rotate(new Vector3(45, 0, 0) * Time.deltaTime * speed);
    }

    void FixedUpdate()
    {
        if (!isFinal)
        {
            float newY = transform.position.y;

            if (movingUp)
            {
                newY += speed * Time.deltaTime;

                if (newY >= maxY)
                {
                    newY = maxY;
                    movingUp = false;
                }
            }
            else
            {
                newY -= speed * Time.deltaTime;

                if (newY <= minY)
                {
                    newY = minY;
                    movingUp = true;
                }
            }
            transform.position = new Vector3(originalX, newY, originalZ);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isFinal)
            {
                imageMana.gameObject.SetActive(true);
                armaPrincipal.SetActive(true);
                imageChangeButton.gameObject.SetActive(true);
                gameObject.SetActive(false);

                LightsActive[] allLights = FindObjectsOfType<LightsActive>();

                foreach (LightsActive lights in allLights)
                {
                    lights.ActivateLights();
                }
            }
            else
            {
                imageFinal.gameObject.SetActive(true);
                textMeshPro.text = "Win Score: " + pm.points;
                StartCoroutine(FinalGame());
            }
        }
    }

    private IEnumerator FinalGame()
    {
        yield return new WaitForSecondsRealtime(10f);
        Application.Quit();
    }
}
