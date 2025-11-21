using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    public float currentSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float maxTilt;

    [Header("Jump Stats")]
    [SerializeField] bool chargingJump;
    [SerializeField] float jumpForce;

    [Header("Hook Stats")]
    public float hookAngle;

    [Header("Joint Stats")]
    [SerializeField] float jointCurrentLength;
    [SerializeField] float jointTargetLength;
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointSpeed;
    [SerializeField] bool canRecoil;

    [Header("GroundCheck Stats")]
    [SerializeField] bool bodyOnGround;
    [SerializeField] float bodyCheckRadius;
    [SerializeField] bool wheelOnGround;
    [SerializeField] float wheelCheckRadius;
    // Layers
    [SerializeField] LayerMask groundLayer;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider bodyCollider;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject aimer;
    [SerializeField] GameObject bodyMesh;
    [SerializeField] GameObject wheelMesh;

#region Awake/Start Functions
    private void Start()
    {
        jointCurrentLength = jointDefaultLength;

        bodyCheckRadius = bodyCollider.radius + 0.25f;
        wheelCheckRadius = wheelCollider.radius + 0.25f;
    }
#endregion

#region Update Functions
    private void Update()
    {
        TiltPlayer();

        JointLenght();

        RotateHook();

        LayerCheck();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        ModifyJoint();
    }

    private void LateUpdate()
    {
        CreateGravity();
    }

    private void TiltPlayer()
    {
        // Tilts the player according to the current speed.
        transform.rotation = Quaternion.Euler(
            0,
            0,
            maxTilt * -currentSpeed
        );

        wheelRB.transform.localPosition = new Vector3(
            0,
            wheelRB.transform.localPosition.y,
            0
        );

        // Prevents the body mesh from tilting.
        bodyMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void JointLenght()
    {
        if (!wheelOnGround && jointCurrentLength >= jointDefaultLength)
        {
            canRecoil = true;
        }
        else if (bodyOnGround)
        {
            canRecoil = false;
        }

        // Defines the target lenght and speed.
        if (canRecoil)
        {
            jointTargetLength = 0;
            jointSpeed = 5;
        }
        else
        {
            if (chargingJump)
            {
                jointTargetLength = jointChargedLength;
                jointSpeed = 1;
            }
            else
            {
                jointTargetLength = jointDefaultLength;
                jointSpeed = 10;
            }
        }
    }

    private void RotateHook()
    {
        if (moveInput != Vector2.zero)
        {
            // Converts the input vector2 into a float and snaps the value at a multiple of 45.
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg,
                45
            );
        }

        aimer.transform.rotation = Quaternion.Euler(
            0,
            0,
            hookAngle
        );
    }

    private void LayerCheck()
    {
        bodyOnGround = Physics.CheckSphere(transform.position, bodyCheckRadius, groundLayer);
        wheelOnGround = Physics.CheckSphere(wheelRB.transform.position, wheelCheckRadius, groundLayer);
    }

    private void MovePlayer()
    {
        if (wheelOnGround)  // Prevents controlling the movement on air.
        {
            // Modifies the player's speed.
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed * moveInput.x,
                acceleration * Time.deltaTime
            );
        }

        // Applies the player's speed.
        transform.position = new Vector3(
            transform.position.x + currentSpeed / 10, // Divides it by 10 so the player doesn't go so fast.
            transform.position.y,
            0
        );
    }

    private void ModifyJoint()
    {
        // Modifies the player's joint length.
        jointCurrentLength = Mathf.MoveTowards(
            jointCurrentLength,
            jointTargetLength,
            jointSpeed * Time.deltaTime
        );

        // Applies the player's joint length.
        joint.connectedAnchor = new Vector3(
            joint.connectedAnchor.x,
            jointCurrentLength + (moveInput.y/10),
            joint.connectedAnchor.z
        );
    }

    private void CreateGravity()
    {
        // Creates a local gravity to each part.
        bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
        wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);
    }
#endregion

#region Action Functions
    private void Jump()
    {
        if (wheelOnGround)
        {
            bodyRB.AddForce(
                0,
                jumpForce * 100 * (jointDefaultLength + jointChargedLength - jointCurrentLength),
                0
            );
        } 
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
        }
        else
        {
            chargingJump = false;
        }

        if (context.canceled)
        {
            Jump();
        }
    }
#endregion
}
