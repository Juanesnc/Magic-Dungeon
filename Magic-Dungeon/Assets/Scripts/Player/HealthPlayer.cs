using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPlayer : MonoBehaviour
{
    [Header("References")]
    public int health;
    public Slider slider;
    public LayerMask whatIsLight;
    public Slider sliderPotion;
    public ParticleSystem particle;

    private bool curedPlayer;
    float sliderPotionStart;
    ThirdPersonCam thirdPersonCam;
    Animator anim;
    PlayerMovement pm;
    SaveSystem saveSystem;

    private void Start()
    {
        pm = gameObject.GetComponentInParent<PlayerMovement>();
        saveSystem = FindObjectOfType<SaveSystem>();
        thirdPersonCam = FindObjectOfType<ThirdPersonCam>();
        anim = GetComponent<Animator>();
        particle.Stop();
        curedPlayer = false;
        slider.maxValue = health;
        sliderPotionStart = sliderPotion.value;
    }

    private void Update()
    {
        if (sliderPotion.value < 60)
            sliderPotion.value += Time.deltaTime;
        slider.value = health;
        if (Input.GetKeyDown(KeyCode.F) && sliderPotion.value == sliderPotionStart && slider.maxValue != health)
        {
            pm.restricted = true;
            sliderPotion.value = 0;
            anim.Play("DrinkPoti", 0, 0f);
            particle.Play();
            StartCoroutine(HealdPlayer());
        }
    }
    public void TakeDamage(int damage)
    {
        if (!pm.restricted && !pm.gameOver)
        {
            anim.Play("GetHit", 0, 0f);
            health -= damage;
            pm.restricted = true;
            StartCoroutine(ResetDamage());
        }

        if (health <= 0)
        {
            pm.freeze = true;
            pm.gameOver = true;
            anim.Play("DiePlayer", 0, 0f);
            StartCoroutine(ResetGame());
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int heal)
    {
        health = heal;
    }

    private void OnTriggerStay(Collider other)
    {
        if (whatIsLight == (whatIsLight | (1 << other.gameObject.layer)))
        {
            if (!curedPlayer && health < 480)
            {
                curedPlayer = true;
                StartCoroutine(HealdPlayer());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (whatIsLight == (whatIsLight | (1 << other.gameObject.layer)))
        {
            curedPlayer = false;
        }
    }

    private IEnumerator HealdPlayer()
    {
        float duration = 10f;
        float elapsedTime = 0f;

        while (health < slider.maxValue && elapsedTime < duration - 0.2f)
        {

            if (elapsedTime == 1f)
                pm.restricted = false;
            curedPlayer = true;
            if (curedPlayer)
            {
                health += 5;
                yield return new WaitForSecondsRealtime(0.2f);
                elapsedTime += 0.2f;
                curedPlayer = false;
            }
        }
        particle.Stop();
    }

    private IEnumerator ResetDamage()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        pm.restricted = false;
    }

    private IEnumerator ResetGame()
    {
        yield return new WaitForSecondsRealtime(5f);
        saveSystem.LoadGameButton();
        pm.freeze = false;
        pm.gameOver = false;
        thirdPersonCam.resetGame = true;
        anim.Play("Idle01", 0, 0f);
    }
}
