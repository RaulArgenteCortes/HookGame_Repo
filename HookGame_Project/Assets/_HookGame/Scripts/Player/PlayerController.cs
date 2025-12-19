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
    [SerializeField] float jumpForce;
    private bool chargingJump;

    [Header("Hook Stats")]
    public float hookAngle;

    [Header("Joint Stats")]
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointRecoiledLength;
    private float jointCurrentLength;
    private float jointTargetLength;
    private float jointSpeed;
    private bool mustRecoil;

    [Header("LayerCheck Stats")]
    [SerializeField] float CheckRadius;
    [SerializeField] LayerMask groundLayer;
    private bool bodyOnGround;
    private bool wheelOnGround;
    private bool againstWallL;
    private bool againstWallR;
    private bool wheelIsClippingL;
    private bool wheelIsClippingR;

    [Header("LayerChecks")]
    [SerializeField] GameObject bodyCheckBottom;
    [SerializeField] GameObject bodyCheckLeft;
    [SerializeField] GameObject bodyCheckRight;
    [SerializeField] GameObject wheelCheckTopL;
    [SerializeField] GameObject wheelCheckTopR;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;
    [SerializeField] GameObject connector;
    [SerializeField] GameObject aimer;
    [SerializeField] GameObject bodyMesh;

    #region Awake/Start Functions
    private void Start()
    {
        // Starts the game with the wheel recoiled.
        mustRecoil = true;
        jointTargetLength = 0;
        jointCurrentLength = 0;
        wheelRB.transform.localPosition = Vector3.zero;
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

        PhysicsController();
    }

    private void JointController()
    {
        if (!wheelOnGround && jointCurrentLength >= jointDefaultLength)
        {
            mustRecoil = true;
        }
        else if (bodyOnGround)
        {
            mustRecoil = false;
        }

        // Defines the target lenght and speed of the joint.
        if (mustRecoil || (wheelIsClippingL || wheelIsClippingR))
        {
            jointTargetLength = jointRecoiledLength;
            jointSpeed = (!(wheelIsClippingL || wheelIsClippingR) ? 3 : 30); // Drastically increases the speed in case of clipping.
        }
        else
        {
            if (chargingJump && wheelOnGround)
            {
                jointTargetLength = jointChargedLength;
                jointSpeed = (jointCurrentLength > jointTargetLength ? 1 : 3); // Only slows the speed if it has to recoil.
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

        // Just an object for debugging.
        aimer.transform.rotation = Quaternion.Euler(
            0,
            0,
            hookAngle
        );
    }

    private void LayerCheck()
    {
        bodyOnGround = Physics.CheckSphere(bodyCheckBottom.transform.position, CheckRadius, groundLayer);
        wheelOnGround = Physics.CheckSphere(wheelRB.transform.position, CheckRadius + wheelCollider.radius, groundLayer);

        againstWallL = Physics.CheckSphere(bodyCheckLeft.transform.position, CheckRadius, groundLayer);
        againstWallR = Physics.CheckSphere(bodyCheckRight.transform.position, CheckRadius, groundLayer);

        wheelIsClippingL = Physics.CheckSphere(wheelCheckTopL.transform.position, CheckRadius, groundLayer);
        wheelIsClippingR = Physics.CheckSphere(wheelCheckTopR.transform.position, CheckRadius, groundLayer);
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
                (maxSpeed * moveInput.x)
                    * (againstWallL || againstWallR ? 0.2f : 1) // Reduces the speed when against a wall.
                    + (wheelIsClippingL ? +0.1f : 0) + (wheelIsClippingR ? -0.1f : 0), // Helps to unclip the wheel
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
        transform.rotation = Quaternion.Euler(0, 0, maxTilt * -currentSpeed);   
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

    private void PhysicsController()
    {
        // Creates a local gravity to each part.
        bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
        wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);

        // Prevents the player for gaining unwanted momentum.
        bodyRB.linearVelocity = new Vector3(0, bodyRB.linearVelocity.y, 0);
        bodyRB.angularVelocity = new Vector3(0, bodyRB.angularVelocity.y, 0);
    }
    #endregion

    #region Action Functions
    private void Jump()
    {
        // Jumps depending on how charged is the player.
        bodyRB.AddForce(
            0,
            jumpForce * 100 * (jointCurrentLength >= jointChargedLength ? (jointDefaultLength - jointCurrentLength) : jointChargedLength), // Prevents the player from jumping too much.
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
