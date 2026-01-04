using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;
    [SerializeField] Vector3 bodyLockPosition;
    [SerializeField] Vector3 wheelLockPosition;
    [SerializeField] bool lockBody;
    [SerializeField] bool lockWheel;
    // Debug:
    [SerializeField] float bodyLinearVelocityX;
    [SerializeField] float wheelLinearVelocityX;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    [SerializeField] float moveAccelerationSpeed;
    [SerializeField] float moveMaxSpeed;

    [Header("Jump Stats")]
    [SerializeField] float jumpForce;
    private bool chargingJump;

    [Header("Hook Stats")]
    public float hookAngle;
    public bool usingHook;
    [SerializeField] float hookLength;
    [SerializeField] private Vector2 hookAngleVector;

    [Header("Joint Stats")]
    public float jointDistance;
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointRecoiledLength;
    private float jointCurrentLenght;

    [Header("LayerCheck Stats")]
    [SerializeField] float CheckRadius;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask interactableLayer;
    private bool bodyOnGround;
    private bool wheelOnGround;

    [Header("LayerChecks")]
    [SerializeField] GameObject bodyCheckBottom;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject aimer;

    #region Update Functions
    private void Update()
    {
        LayerCheck();

        ComponentTransform();

        LockPosition();

        bodyLinearVelocityX = bodyRB.linearVelocity.x;
        wheelLinearVelocityX = wheelRB.linearVelocity.x;
    }

    private void LayerCheck()
    {
        wheelOnGround = Physics.CheckSphere(wheelRB.transform.position, CheckRadius + wheelCollider.radius, groundLayer);
    }

    private void ComponentTransform()
    {
        // Calculates the distance between the body and the wheel.
        jointDistance = Vector3.Distance(bodyRB.transform.position, wheelRB.transform.position);

        // Prevents the wheel from moving too much.
        if (!usingHook && !wheelOnGround && (bodyRB.transform.position.y - wheelRB.transform.position.y) < wheelCollider.radius)
        {
            wheelRB.transform.position = new Vector3(
                bodyRB.transform.position.x,
                wheelRB.transform.position.y,
                wheelRB.transform.position.z
            );
        }

        // Calculates the angle of the hook.
        if (moveInput != Vector2.zero && !usingHook)
        {
            // Converts the input from a vector2 into a float and snaps the value at a multiple of 45.
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg,
                45
            );

            hookAngleVector = moveInput;
        }

        // Just an object for debugging.
        aimer.transform.rotation = Quaternion.Euler(
            0,
            0,
            hookAngle
        );
    }

    private void LockPosition()
    {
        if (lockBody)
        {
            bodyRB.transform.position = bodyLockPosition;
        }

        if (lockWheel)
        {
            wheelRB.transform.position = wheelLockPosition;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();

        JointController();

        PhysicsController();
    }

    private void MovePlayer()
    {
        // Moves the player if the wheel is on the ground and the speed isn't too fast.
        if (wheelOnGround && Mathf.Abs(bodyRB.linearVelocity.x) < moveMaxSpeed * Mathf.Abs(moveInput.x))
        {
            bodyRB.AddForce(new Vector3(moveAccelerationSpeed * moveInput.x, 0, 0), ForceMode.Force);
        }
    }

    private void JointController()
    {
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
    }

    private void PhysicsController()
    {
        if (!usingHook)
        {
            bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
            wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);
        }

        // Reduces the momentum when there is no input and the wheel is on the ground.
        if (moveInput.x == 0 && wheelOnGround)
        {
            bodyRB.linearVelocity = new Vector3(
                bodyRB.linearVelocity.x * 0.9f,
                bodyRB.linearVelocity.y,
                bodyRB.linearVelocity.z * 0
            );

            wheelRB.linearVelocity = new Vector3(
                wheelRB.linearVelocity.x * 0.9f,
                wheelRB.linearVelocity.y,
                wheelRB.linearVelocity.z * 0
            );
        }
    }
    #endregion

    #region Action Functions
    private void Jump()
    {
        if (wheelOnGround)
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
            usingHook = true;

            bodyLockPosition = bodyRB.transform.position;
            lockBody = true;
            wheelRB.transform.position = bodyRB.transform.position;
            wheelRB.linearVelocity = Vector3.zero;

            joint.anchor = new Vector3(
                hookLength * hookAngleVector.x,
                hookLength * hookAngleVector.y,
                0
            );
        }  
    }

    private void EndHook()
    {
        lockBody = false;
        lockWheel = false;
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
            if (wheelOnGround && !usingHook)
            {
                chargingJump = true;
            }
            else if (!usingHook)
            {
                StartHook();
            }
        }

        if (context.canceled)
        {
            Jump();

            chargingJump = false;
        }
    }
    #endregion
}
