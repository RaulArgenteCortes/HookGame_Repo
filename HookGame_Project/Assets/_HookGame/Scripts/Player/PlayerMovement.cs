using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;
    [SerializeField] Vector3 bodyLockPosition;
    [SerializeField] Vector3 wheelLockPosition;
    [SerializeField] bool bodyLock;
    [SerializeField] bool wheelLock;

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
    [SerializeField] float hookMaxLength;
    private Vector2 hookAngleVector;
    private bool recoverHook;
    private bool canUseHook;

    [Header("Joint Stats")]
    public float jointDistance;
    [SerializeField] float jointStrenght;
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointRecoiledLength;
    private float jointCurrentLenght;

    [Header("LayerCheck Stats")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask interactableLayer;
    private bool bodyOnGround;
    private bool wheelOnGround;
    private bool bodyTouchingInteractable;
    private bool hookedSomething;

    [Header("LayerChecks")]
    [SerializeField] GameObject bodyCheckBottom;
    [SerializeField] GameObject wheelCheckBottom;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider bodyCollider;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject aimer;

    #region Start/Awake Functions
    private void Start()
    {
        // Starts with the hook aiming downwards.
        hookAngle = 180;
        hookAngleVector = new Vector2(0, -1);

        joint.spring = jointStrenght;
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        LayerCheck();

        ComponentTransform();

        LockPosition();

        HookController();
    }

    private void LayerCheck()
    {
        bodyOnGround = Physics.CheckSphere(bodyCheckBottom.transform.position, 0.2f, groundLayer);
        wheelOnGround = Physics.CheckSphere(wheelCheckBottom.transform.position, 0.2f, groundLayer);

        bodyTouchingInteractable = Physics.CheckSphere(bodyRB.transform.position, 0.05f + bodyCollider.radius, interactableLayer);
        
        hookedSomething =
            Physics.CheckSphere(wheelRB.transform.position, 0.05f + wheelCollider.radius, interactableLayer)
            && usingHook;
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

            // A version of the move input that is never set to 0.
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
        if (wheelOnGround && !usingHook)
        {
            canUseHook = true;
        }

        if (usingHook)
        {
            if (hookedSomething)
            {
                recoverHook = true;

                joint.anchor = Vector3.zero;

                wheelLockPosition = wheelRB.transform.position;
                wheelLock = true;
                bodyLock = false;

                joint.spring = jointStrenght * 2;
            }

            if (!hookedSomething && jointDistance >= hookMaxLength - 0.1f)
            {
                recoverHook = true;

                wheelRB.linearVelocity = Vector3.zero;

                joint.anchor = Vector3.zero;
            }

            if (!hookedSomething && recoverHook && jointDistance < 0.1f)
            {
                EndHook();
            }
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
        if (wheelOnGround && Mathf.Abs(bodyRB.linearVelocity.x) < moveMaxSpeed * Mathf.Abs(moveInput.x) && !usingHook)
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
        else
        {
            joint.connectedAnchor = Vector3.zero;
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
        if (wheelOnGround || (hookedSomething && recoverHook && jointDistance < 1f))
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
            joint.anchor = new Vector3(
                hookMaxLength * hookAngleVector.x,
                hookMaxLength * hookAngleVector.y,
                0
            );
        }  
    }

    private void EndHook()
    {
        bodyRB.linearVelocity = Vector3.zero;
        wheelRB.linearVelocity = Vector3.zero;

        bodyLock = false;
        wheelLock = false;

        recoverHook = false;
        usingHook = false;

        joint.spring = jointStrenght;
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
            if (/*wheelOnGround && */!usingHook)
            {
                chargingJump = true;
            }
            
            if (canUseHook && !usingHook && !wheelOnGround)
            {
                StartHook();
            }

            if (usingHook && recoverHook && bodyTouchingInteractable)
            {
                EndHook();
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
