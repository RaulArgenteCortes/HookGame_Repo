using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;
    public Vector3 bodyLockPosition;
    public Vector3 wheelLockPosition;
    private bool bodyLock;
    private bool wheelLock;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    [SerializeField] float moveAccelerationSpeed;

    [Header("Jump Stats")]
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpForce;
    private bool chargingJump;

    [Header("Hook Stats")]
    public float hookAngle;
    public Vector2 hookAngleVector;
    public bool usingHook;
    public bool canUseHook;
    [SerializeField] float hookMaxLength;
    private bool recoverHook;

    [Header("Joint Stats")]
    public float jointDistance;
    public float jointAngle;
    public Vector2 jointAngleVector;
    [SerializeField] float jointSpring;
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointRecoiledLength;
    private float jointCurrentLenght;
    private bool wheelAtLeft;

    [Header("LayerCheck Stats")]
    public bool hookedSomething;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask interactableLayer;
    private bool bodyOnGround;
    private bool wheelOnGround;
    private bool bodyTouchingGround;

    [Header("LayerChecks")]
    [SerializeField] GameObject bodyCheckBottom;
    [SerializeField] GameObject wheelCheckBottom;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider bodyCollider;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject spawnPoint;
    [SerializeField] GameObject aimer;

    #region Start/Awake Functions
    private void Start()
    {
        // Starts with the hook aiming downwards.
        hookAngle = 180;
        hookAngleVector = new Vector2(0, -1);
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        CalculateAngles();

        LockPosition();

        HookController();

        RespawnController();
    }

    private void CalculateAngles()
    {
        // Calculates the angle of the hook.
        if (moveInput != Vector2.zero && !usingHook)
        {
            // Converts the input from a vector2 into a float and snaps the value at a multiple of 45.
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg,
                45
            );

            // A vector2 version of hookAngle.
            hookAngleVector = new Vector2(
                -Mathf.Sin(hookAngle * Mathf.Deg2Rad),
                Mathf.Cos(hookAngle * Mathf.Deg2Rad)
            );
        }

        // Calculates the angle of the joint.
        jointAngle = Vector2.Angle(bodyRB.transform.up, (wheelRB.transform.position - bodyRB.transform.position));

        // A vector2 version of jointAngle.
        jointAngleVector = new Vector2(
            Mathf.Sin(jointAngle * Mathf.Deg2Rad)
                * (wheelAtLeft ? -1 : 1), // Just a corrector.
            Mathf.Cos(jointAngle * Mathf.Deg2Rad)
        );

        // Just an object for debugging.
        aimer.transform.rotation = Quaternion.Euler(
            0,
            0,
            hookAngle
        );
    }

    private void LockPosition()
    {
        if (bodyLock)
        {
            bodyRB.transform.position = bodyLockPosition;
        }

        if (wheelLock)
        {
            wheelRB.transform.position = wheelLockPosition;
        }
    }

    private void HookController()
    {
        if ((wheelOnGround && !usingHook) || hookedSomething)
        {
            canUseHook = true;
        }

        if (usingHook)
        {
            if (hookedSomething)
            {
                recoverHook = true;

                joint.connectedAnchor = Vector3.zero;

                wheelLockPosition = wheelRB.transform.position;
                wheelLock = true;
                bodyLock = false;
            }

            if (!hookedSomething && jointDistance >= hookMaxLength - 0.1f)
            {
                recoverHook = true;

                joint.connectedAnchor = Vector3.zero;
            }

            if (!hookedSomething && recoverHook && jointDistance < 0.15f)
            {
                EndHook();
            }
        }
    }

    private void RespawnController()
    {
        if (bodyRB.transform.position.y < -10)
        {
            Respawn();
        }
    }

    private void FixedUpdate()
    {
        LayerCheck(); // Putting this here prevents the bools from flashing.

        MovePlayer();

        JointController();

        PhysicsController();
    }

    private void LayerCheck()
    {
        bodyOnGround = Physics.CheckSphere(bodyCheckBottom.transform.position, 0.1f, groundLayer);
        wheelOnGround = Physics.CheckSphere(wheelCheckBottom.transform.position, 0.3f, groundLayer);

        bodyTouchingGround = Physics.CheckSphere(bodyRB.transform.position, 0.05f + bodyCollider.radius, groundLayer);

        hookedSomething =
            Physics.CheckSphere(wheelRB.transform.position, 0.01f + wheelCollider.radius, interactableLayer)
            && usingHook;
    }

    private void MovePlayer()
    {
        // Moves the player if the wheel is on the ground.
        if (wheelOnGround && !usingHook)
        {
            bodyRB.AddForce(new Vector3(moveAccelerationSpeed * moveInput.x, 0, 0), ForceMode.Force);
        }
    }

    private void JointController()
    {
        // Increases the spring joint when something is hooked.
        if (usingHook && wheelLock && !bodyLock)
        {
            joint.spring = jointSpring * 10;
        }
        else
        {
            joint.spring = jointSpring;
        }

        // Calculates the distance between the body and the wheel.
        jointDistance = Vector3.Distance(bodyRB.transform.position, wheelRB.transform.position);

        if (chargingJump && wheelOnGround)
        {
            if (jointCurrentLenght > jointChargedLength)
            {
                jointCurrentLenght = Mathf.MoveTowards(
                    jointCurrentLenght,
                    jointChargedLength,
                    1 * Time.fixedDeltaTime
                );
            }
            else
            {
                jointCurrentLenght = jointChargedLength;
            }            
        }
        else if (!bodyOnGround && !wheelOnGround)
        {
            jointCurrentLenght = jointRecoiledLength;       
        }
        else
        {
            jointCurrentLenght = jointDefaultLength;
        }

        // Modifies the joint lenght.
        if (!usingHook)
        {
            joint.connectedAnchor = new Vector3(
                0,
                jointCurrentLenght * (1 + moveInput.y * 0.05f),
                0
            );
        }

        // Calculates the horizontal position of the wheel.
        wheelAtLeft = wheelRB.transform.position.x < bodyRB.transform.position.x;
    }

    private void PhysicsController()
    {
        if (!usingHook)
        {
            bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
            wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);
        }

        // Reduces the horizontal momentum when there is no input and the wheel is on the ground.
        if (moveInput.x == 0 && wheelOnGround && !usingHook)
        {
            bodyRB.linearVelocity = new Vector3(
                bodyRB.linearVelocity.x * 0.9f,
                bodyRB.linearVelocity.y,
                bodyRB.linearVelocity.z
            );

            wheelRB.linearVelocity = new Vector3(
                wheelRB.linearVelocity.x * 0.9f,
                wheelRB.linearVelocity.y,
                wheelRB.linearVelocity.z
            );
        }
    }
    #endregion

    #region Action Functions
    private void Jump()
    {
        if (wheelOnGround && !usingHook)
        {
            bodyRB.AddForce(new Vector3(
                0,
                jumpForce * (jointDefaultLength - jointDistance),
                0
            ), ForceMode.Impulse);
        } 
    }

    private void StartHook()
    {
        if (!usingHook)
        {
            canUseHook = false;
            usingHook = true;

            bodyLockPosition = bodyRB.transform.position;
            bodyLock = true;
            wheelRB.transform.position = bodyRB.transform.position;
            bodyRB.linearVelocity = Vector3.zero;
            wheelRB.linearVelocity = Vector3.zero;

            // Throws the hook depending on the direction.
            joint.connectedAnchor = new Vector3(
                hookMaxLength * -hookAngleVector.x,
                hookMaxLength * -hookAngleVector.y,
                0
            );
        }  
    }

    public void EndHook()
    {
        bodyRB.linearVelocity = Vector3.zero;
        wheelRB.linearVelocity = Vector3.zero;

        bodyLock = false;
        wheelLock = false;

        if (hookedSomething && bodyTouchingGround) 
        {
            // Makes a walljump depending of the joint's angle.
            bodyRB.AddForce(new Vector3(
                (wallJumpForce/2) * -jointAngleVector.x,
                (wallJumpForce/2) * -jointAngleVector.y
                    + (wallJumpForce/2), // Always adds a vertical force.
                0
            ), ForceMode.Impulse);
        }

        // Pushes the player back if it didn't hook something.
        if (!hookedSomething)
        {
            bodyRB.AddForce(new Vector3(
                5 * -hookAngleVector.x,
                5 * -hookAngleVector.y + 5/2,
                0
            ), ForceMode.Impulse);
        }

        wheelRB.transform.position = bodyRB.transform.position;

        recoverHook = false;
        usingHook = false;
    }

    public void Respawn()
    {
        bodyRB.linearVelocity = Vector3.zero;
        wheelRB.linearVelocity = Vector3.zero;

        bodyLock = false;
        wheelLock = false;

        bodyRB.transform.position = wheelRB.transform.position = spawnPoint.transform.position;

        recoverHook = false;
        usingHook = false;
    }
    #endregion

    #region Input Functions
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            chargingJump = true;
            
            if (canUseHook && !usingHook && !wheelOnGround)
            {
                StartHook();
            }
        }

        if (context.canceled)
        {
            if (usingHook && recoverHook && bodyTouchingGround)
            {
                EndHook();
            }
            else
            {
                Jump();
            } 

            chargingJump = false;
        }
    }
    #endregion
}