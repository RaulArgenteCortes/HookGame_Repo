using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    [SerializeField] float currentSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float maxTilt;

    [Header("Jump Stats")]
    [SerializeField] bool chargingJump;
    [SerializeField] float jointCurrentLength;
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointSpeed;
    [SerializeField] float jumpForce;

    [Header("Hook Stats")]
    public float hookAngle;

    [Header("Object References")]
    [SerializeField] GameObject wheelGO;
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject aimer;
    [SerializeField] GameObject bodyMesh;
    [SerializeField] GameObject wheelMesh;

    #region Awake/Start Functions
    private void Awake()
    {
        
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        RotateHook();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        TiltPlayer();

        ModifyJoint();
    }

    private void LateUpdate()
    {
        CreateGravity();
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

    private void MovePlayer()
    {
        // Modifies the player's speed.
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            maxSpeed * moveInput.x,
            acceleration * Time.deltaTime
        );

        // Applies the player's speed.
        transform.position = new Vector3(
            transform.position.x + currentSpeed/10, // Divides it by 10 so the player doesn't go so fast.
            transform.position.y,
            0
        );
    }

    private void TiltPlayer()
    {
        // Tilts the player according to the current speed.
        transform.rotation = Quaternion.Euler(
            0,
            0,
            maxTilt * -currentSpeed
        );

        // Prevents the body mesh from tilting.
        bodyMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void ModifyJoint()
    {
        // Modifies the player's joint length.
        if (!chargingJump)
        {
            jointCurrentLength = Mathf.MoveTowards(
                jointCurrentLength,
                jointDefaultLength,
                jointSpeed * 10 * Time.deltaTime
            );
        }
        else
        {
            jointCurrentLength = Mathf.MoveTowards(
                jointCurrentLength,
                jointChargedLength,
                jointSpeed * Time.deltaTime
            );
        }

        // Applies the player's joint length.
        joint.connectedAnchor = new Vector3(
            joint.connectedAnchor.x,
            jointCurrentLength,
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
        bodyRB.AddForce(
            0,
            jumpForce * 100 * (jointDefaultLength + jointChargedLength - jointCurrentLength),
            0
        );
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
            Debug.Log("Jump!");
        }
    }
    #endregion
}
