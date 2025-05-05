using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float slowSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public int numJumps;
    int originalJumps;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlatform;
    public LayerMask whatIsBox;
    public LayerMask whatIsVoid;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;
    public Animator anim;
    public GameObject combatCam;
    public float points = 0;

    float horizontalInput;
    float verticalInput;
    float finalHorizontalInput;
    float finalVerticalInput;
    
    bool isMoving;

    Vector3 lastPosition; 
    Vector3 moveDirection;
    Vector3 directionAnimation;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        slow,
        freeze,
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        air
    }

    public bool slow;
    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;
    public bool beaten;
    public bool freeze;

    public bool restricted;
    public bool gameOver;

    SaveSystem saveSystem;
    ThirdPersonCam thirdPersonCam;

    private void Start()
    {
        saveSystem = FindObjectOfType<SaveSystem>();
        thirdPersonCam = FindObjectOfType<ThirdPersonCam>();
        rb = GetComponent<Rigidbody>();

        saveSystem.SaveGameButton();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        lastPosition = transform.position;

        originalJumps = numJumps;
    }

    private void Update()
    {
        LayerMask combinedMask = whatIsGround | whatIsPlatform | whatIsBox;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, combinedMask);
        
        isMoving = (transform.position != lastPosition);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
        {
            if (readyToJump)
            {
                anim.SetBool("Jump", false);
                anim.SetBool("IsAir", false);
            }
            rb.drag = groundDrag;
        }
        else
            rb.drag = 0;

        if (rb.velocity.magnitude > 0.5f)
            anim.SetBool("Move", true);
        else
            anim.SetBool("Move", false);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        if (!OnSlope())
            rb.AddForce(Vector3.down * -Physics.gravity.y * 0.5f);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        finalHorizontalInput = horizontalInput;
        finalVerticalInput = verticalInput;

        directionAnimation = new Vector3(horizontalInput, 0f, verticalInput);

        if(combatCam.activeSelf && state != MovementState.air)
            ControllerAnimationCombat();

        if (slow)
            StartCoroutine(ResetSpeed());

        if (Input.GetKey(jumpKey) && readyToJump && numJumps > 0)
        {
            readyToJump = false;

            Jump();
            
            if (numJumps == 0)
                anim.Play("Jump", 0, 0.0f);
            else
                anim.SetBool("Jump", true);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // if (Input.GetKeyDown(crouchKey) && !wallrunning)
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        //     rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        // }

        // if (Input.GetKeyUp(crouchKey) && !wallrunning)
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        //     rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        // }

        if (grounded)
            numJumps = originalJumps;
    }

    private void StateHandler()
    {
        if (slow)
        {
            state = MovementState.slow;
            desiredMoveSpeed = slowSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        else if (grounded && Input.GetKey(sprintKey) && isMoving && !restricted)
        {
            anim.SetBool("Running", true);
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            anim.SetBool("Running", false);
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            anim.SetBool("IsAir", true);
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            if (!slow)
                StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        if (freeze)
        {
            state = MovementState.freeze;
            rb.isKinematic = true;
        }else {
            rb.isKinematic = false;
        }

        lastPosition = transform.position;

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (restricted | freeze) return;
        if (climbingScript.exitingWall) return;

        moveDirection = orientation.forward * finalVerticalInput + orientation.right * finalHorizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y < 0)
                rb.AddForce(Vector3.down * 10f * moveSpeed, ForceMode.Force);
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void ControllerAnimationCombat()
    {
        if(directionAnimation.magnitude > 0.1f)
        {
            anim.SetBool("StayAttack", false);
            anim.SetFloat("MoveX", horizontalInput);
            anim.SetFloat("MoveZ", verticalInput);
        }
        else
        {
            anim.SetBool("StayAttack", true);
            anim.SetFloat("MoveX", 0f);
            anim.SetFloat("MoveZ", 0f);
        }
    }

    private void Jump()
    {
        numJumps -= 1;

        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (!grounded && rb.velocity.y < 0) return false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (whatIsVoid == (whatIsVoid | (1 << other.gameObject.layer)))
        {
            gameOver = true;
            StartCoroutine(ResetGame());
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Web"))
            slow = true;
        if(other.gameObject.layer == LayerMask.NameToLayer("whatIsCheckPoint"))
        {
            other.gameObject.SetActive(false);
            saveSystem.SaveGameButton();
        }
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSecondsRealtime(3f);
        desiredMoveSpeed = walkSpeed;
        slow = false;
    }

    private IEnumerator ResetGame()
    {
        yield return new WaitForSecondsRealtime(10f);
        saveSystem.LoadGameButton();
        freeze = false;
        gameOver = false;
        thirdPersonCam.resetGame = true;
        anim.Play("Idle01", 0, 0f);
    }
}
