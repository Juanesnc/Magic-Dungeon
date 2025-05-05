using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemy;

    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player"))
        {
                StartCoroutine(SpawEnemys());
        }
    }

    private IEnumerator SpawEnemys()
    {
        anim.SetBool("EnemyRange", true);
        yield return new WaitForSeconds(1.5f);
        enemy.SetActive(true);
    }
}
