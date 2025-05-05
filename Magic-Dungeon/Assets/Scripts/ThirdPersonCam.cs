using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    public CameraStyle currentStyle;
    public GameObject scope;
    public bool resetGame;
    public TextMeshProUGUI changeBotton;

    [Header("Animator")]
    public GameObject weapon;
    public Animator animator;

    [Header("Script References")]
    public PlayerMovement pm;

    private float elapsedTime;
    public enum CameraStyle
    {
        Basic,
        Combat,
        Topdown
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        resetGame = false;
    }

    private void FixedUpdate()
    {
            if ((Input.GetKey(KeyCode.Alpha1) && currentStyle != CameraStyle.Basic) || resetGame)
            {
                changeBotton.text = "2";
                resetGame = false;
                animator.SetBool("InAttacking", false);
                SwitchCameraStyle(CameraStyle.Basic);
                scope.SetActive(false);
            }
            else if (Input.GetKey(KeyCode.Alpha2) && currentStyle != CameraStyle.Combat && weapon.activeSelf)
            {
                changeBotton.text = "1";
                animator.SetBool("InAttacking", true);
                SwitchCameraStyle(CameraStyle.Combat);
                scope.SetActive(true);
            }
            else if (pm.gameOver)
            {
                SwitchCameraStyle(CameraStyle.Topdown);
                scope.SetActive(false);
            }

        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        if (currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);

            orientation.forward = dirToCombatLookAt.normalized;
            playerObj.forward = dirToCombatLookAt.normalized;
        }
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        topDownCam.SetActive(false);



        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);

        currentStyle = newStyle;
    }
}
