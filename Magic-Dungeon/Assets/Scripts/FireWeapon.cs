using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireWeapon : MonoBehaviour
{

    [Header("Light Check")]
    public LayerMask whatIsLight;

    private bool inLight;
    [Header("Weapon References")]
    public TextMeshProUGUI ammoText;
    public GameObject firstProjectile;
    public float numProjectiles;
    public float timeBetweenAttacks;
    public float rechargeTime;
    public float minMana;

    private bool attacking;
    private bool recharge;
    private float startRechargeTime;
    [Header("References")]
    public ThirdPersonCam combatCam;
    public Transform player;
    public Transform orientation;
    public Slider sliderMana;
    public Slider potionMana;
    public ParticleSystem particle;

    private bool alreadyAttacked;
    private float finalTimeBetweenAttacks;
    private float maxMana;
    private float sliderPotionManaStart;
    private Coroutine currentCoroutineWeapon;
    private Coroutine attackCoroutine;
    private Coroutine resetCoroutine;
    private Animator anim;
    private PlayerMovement pm;

    // Update is called once per frame
    private void Start()
    {
        anim = GetComponentInParent<Animator>();
        pm = GetComponentInParent<PlayerMovement>();
        particle.Stop();
        finalTimeBetweenAttacks = timeBetweenAttacks;
        maxMana = sliderMana.maxValue;
        inLight = false;
        recharge = false;
        attacking = false;
        startRechargeTime = rechargeTime;
        sliderPotionManaStart = potionMana.value;
        sliderMana.value = 0f;
    }

    private void Update()
    {
        if (potionMana.value < 30)
            potionMana.value += Time.deltaTime;
        sliderMana.value = numProjectiles;
        if (Input.GetKeyDown(KeyCode.Q) && potionMana.value == sliderPotionManaStart && sliderMana.value < maxMana)
        {
            pm.restricted = true;
            anim.Play("DrinkPoti", 0, 0f);
            potionMana.value = 0;
            particle.Play();
            StartCoroutine(Recharge(rechargeTime));
        }
    }
    private void FixedUpdate()
    {
        if (combatCam.currentStyle == ThirdPersonCam.CameraStyle.Combat && numProjectiles > 0 && Input.GetKey(KeyCode.X) && !alreadyAttacked && !pm.beaten)
        {
            pm.restricted = true;
            alreadyAttacked = true;
            attackCoroutine = StartCoroutine(Attacking(firstProjectile, 25f));
        }

        if (pm.beaten)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
            anim.SetBool("FireAttack", false);
            pm.restricted = false;
            alreadyAttacked = false;
            pm.beaten = false;
        }
    }

    private IEnumerator Attacking(GameObject projectile, float projectileVelocity)
    {
        numProjectiles -= minMana;
        anim.SetBool("FireAttack", true);

        yield return new WaitForSecondsRealtime(timeBetweenAttacks);

        Vector3 launchPosition = transform.position + orientation.forward * 1f;

        Vector3 centerScreenPoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 25f));

        Vector3 launchDirection = (centerScreenPoint - launchPosition).normalized;

        Quaternion launchRotation = Quaternion.LookRotation(-launchDirection);

        Rigidbody rb = Instantiate(projectile, launchPosition, launchRotation).GetComponent<Rigidbody>();

        rb.AddForce(launchDirection * projectileVelocity, ForceMode.Impulse);

        anim.SetBool("FireAttack", false);
        pm.restricted = false;
        resetCoroutine = StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSecondsRealtime(finalTimeBetweenAttacks);
        alreadyAttacked = false;
        pm.beaten = false;
    }

    private IEnumerator Recharge(float time)
    {
        float duration = 10f;
        float elapsedTime = 0f;

        while (numProjectiles < sliderMana.maxValue && elapsedTime < duration - 0.2f)
        {
            if (elapsedTime == 1f)
                pm.restricted = false;
            recharge = true;
            if (recharge)
            {
                numProjectiles += 2f;
                yield return new WaitForSecondsRealtime(0.2f);
                elapsedTime += 0.2f;
                recharge = false;
            }
        }
        particle.Stop();
    }
}
