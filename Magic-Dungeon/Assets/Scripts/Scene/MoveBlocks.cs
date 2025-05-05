using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveBlocks : MonoBehaviour
{
    [Header("References")]
    public float minDistance;
    public float maxDistance;
    public float speed = 0.5f;
    public float waitTime = 2f;

    public bool distanceX;
    public bool distanceY;
    public bool distanceZ;

    [Header("Actived")]
    public bool activated;

    private float targetPosition;
    private bool movingForward = true;

    void Start()
    {
        targetPosition = movingForward ? maxDistance : minDistance;

        if (!activated)
            StartCoroutine(MovePlatform());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();

            StartCoroutine(MovePlatform());

            if (pm)
                pm.gameOver = true;
            StartCoroutine(NextLevel());
        }
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            while (true)
            {
                if (distanceX)
                    transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, targetPosition, speed * Time.deltaTime), transform.position.y, transform.position.z);
                else if (distanceY)
                    transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, targetPosition, speed * Time.deltaTime), transform.position.z);
                else
                    transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.MoveTowards(transform.position.z, targetPosition, speed * Time.deltaTime));

                if (Mathf.Abs(transform.position.x - targetPosition) < 0.01f ||
                Mathf.Abs(transform.position.y - targetPosition) < 0.01f ||
                Mathf.Abs(transform.position.z - targetPosition) < 0.01f)
                {
                    yield return new WaitForSeconds(waitTime);

                    movingForward = !movingForward;
                    targetPosition = movingForward ? maxDistance : minDistance;
                    break;
                }

                yield return null;
            }
        }
    }

    private IEnumerator NextLevel()
    {
        yield return new WaitForSecondsRealtime(5f);
        SceneManager.LoadScene(1);
    }
}
