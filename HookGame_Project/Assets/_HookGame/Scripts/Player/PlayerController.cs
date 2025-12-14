using System;
using UnityEditor.Analytics;
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
    [SerializeField] bool wheelIsClipping;

    [Header("LayerCheck Stats")]
    [SerializeField] float CheckRadius;
    private float bodyCheckRadius;
    private float wheelCheckRadius;
    [SerializeField] bool bodyOnGround;
    [SerializeField] bool wheelOnGround;
    // Layers
    [SerializeField] LayerMask groundLayer;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject connector;
    [SerializeField] GameObject bodyCheck;
    [SerializeField] GameObject wheelCheckTop;
    [SerializeField] GameObject aimer;
    [SerializeField] GameObject bodyMesh;

    #region Awake/Start Functions
    private void Start()
    {
        // Starts the game with the wheel recoiled.
        canRecoil = true;
        jointTargetLength = 0;
        jointCurrentLength = 0;
        wheelRB.transform.localPosition = Vector3.zero;

        // Defines the layercheck stats.
        bodyCheckRadius = CheckRadius;
        wheelCheckRadius = wheelCollider.radius + CheckRadius;
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        JointController();

        RotateHook();

        LayerCheck();

        ComponentTransform();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        JointModifier();

        CreateGravity();
    }

    private void JointController()
    {
        if (!wheelOnGround && jointCurrentLength >= jointDefaultLength)
        {
            canRecoil = true;
        }
        else if (bodyOnGround)
        {
            canRecoil = false;
        }

        // Defines the target lenght and speed of the joint.
        if (canRecoil || wheelIsClipping)
        {
            jointTargetLength = 0;
            jointSpeed = (!wheelIsClipping ? 5 : 50); // Drastically increases the speed in case of clipping.
        }
        else
        {
            if (chargingJump && wheelOnGround)
            {
                jointTargetLength = jointChargedLength;
                jointSpeed = (jointCurrentLength > jointTargetLength ? 1 : 5); // Only slows the speed if it has to recoil.
            }
            else
            {
                jointTargetLength = jointDefaultLength;
                jointSpeed = jumpForce; // Drastically increases the speed for an illusion of push.
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
        bodyOnGround = Physics.CheckSphere(bodyCheck.transform.position, bodyCheckRadius, groundLayer);
        wheelOnGround = Physics.CheckSphere(wheelRB.transform.position, wheelCheckRadius, groundLayer);

        wheelIsClipping = Physics.CheckSphere(wheelCheckTop.transform.position, wheelCheckRadius - CheckRadius, groundLayer);
    }

    private void ComponentTransform()
    {
        // Prevents the wheel from moving horizontally and streching too much.
        wheelRB.transform.localPosition = new Vector3(0, wheelRB.transform.localPosition.y, 0);

        // Prevents the body mesh from tilting.
        bodyMesh.transform.rotation = Quaternion.Euler(-90, 0, 0);

        // Modiffies the connector position.
        connector.transform.localPosition = wheelRB.transform.localPosition / 2;
    }

    private void MovePlayer()
    {
        if (wheelOnGround) // Prevents controlling the movement on air.
        {
            // Modifies the player's speed.
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed * moveInput.x,
                acceleration * Time.fixedDeltaTime
            );
        }

        // Applies the player's speed.
        bodyRB.MovePosition(new Vector3(
            bodyRB.transform.position.x + currentSpeed/10, // Divides it by 10 so the player doesn't go too fast.
            bodyRB.transform.position.y,
            0
        ));

        // Tilts the player according to the current speed.
        transform.rotation = Quaternion.Euler(
            0,
            0,
            maxTilt * -currentSpeed
        );

        // Prevents the player for gaining unwanted momentum.
        bodyRB.linearVelocity = new Vector3(0, bodyRB.linearVelocity.y, 0);
        bodyRB.angularVelocity = new Vector3(0, bodyRB.angularVelocity.y, 0);
    }

    private void JointModifier()
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
            jointCurrentLength + (wheelOnGround ? moveInput.y/10 : 0),
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
        // Jumps depending on how charged is the player.
        bodyRB.AddForce(
            0,
            jumpForce * 100 * (jointDefaultLength - jointCurrentLength),
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

        if (context.canceled)
        {
            if (wheelOnGround)
            {
                Jump();
            }

            chargingJump = false;
        }
    }
    #endregion
}
