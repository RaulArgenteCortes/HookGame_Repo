using System;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Physics Stats")]
    // Body:
    [SerializeField] float bodyWeight;
    private Vector3 bodyFixedPos;
    private bool lockBodyPos;
    // Wheel:
    [SerializeField] float wheelWeight;
    private Vector3 wheelFixedPos;
    private bool lockWheelPos;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    public float moveCurrentSpeed;
    [SerializeField] float moveMaxSpeed;
    [SerializeField] float moveAcceleration;
    [SerializeField] float moveMaxTilt;

    [Header("Jump Stats")]
    [SerializeField] float jumpForce;
    private bool chargingJump;

    [Header("Hook Stats")]
    public float hookAngle;
    public bool usingHook;
    [SerializeField] float hookAcceleration;
    [SerializeField] float hookStartSpeed;
    private float hookCurrentSpeed;
    private float hookWheelCurrentPos;
    private float hookBodyCurrentPos;
    private bool hookedSomething;

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
    [SerializeField] LayerMask interactableLayer;
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
        jointTargetLength = jointChargedLength;
        jointCurrentLength = jointChargedLength;
        wheelRB.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        RotateHook();

        LayerCheck();

        ComponentTransform();

        FixPosition();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        JointController();

        HookControllerA();

        PhysicsController();
    }

    private void RotateHook()
    {
        if (moveInput != Vector2.zero && !usingHook)
        {
            // Converts the input from a vector2 into a float and snaps the value at a multiple of 45.
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
        wheelOnGround =
            Physics.CheckSphere(wheelRB.transform.position, CheckRadius + wheelCollider.radius, groundLayer)
            && !usingHook;

        againstWallL = Physics.CheckSphere(bodyCheckLeft.transform.position, CheckRadius, groundLayer);
        againstWallR = Physics.CheckSphere(bodyCheckRight.transform.position, CheckRadius, groundLayer);

        wheelIsClippingL = Physics.CheckSphere(wheelCheckTopL.transform.position, CheckRadius, groundLayer);
        wheelIsClippingR = Physics.CheckSphere(wheelCheckTopR.transform.position, CheckRadius, groundLayer);

        hookedSomething =
            Physics.CheckSphere(wheelRB.transform.position, CheckRadius + wheelCollider.radius, interactableLayer)
            && usingHook;
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

    private void FixPosition()
    {
        if (lockBodyPos)
        {
            transform.position = bodyFixedPos;
            moveCurrentSpeed = 0;
            bodyRB.linearVelocity = Vector3.zero;
            bodyRB.angularVelocity = Vector3.zero;
            wheelRB.linearVelocity = Vector3.zero;
            wheelRB.angularVelocity = Vector3.zero;
        }

        if (lockWheelPos)
        {
            wheelRB.transform.position = wheelFixedPos;
            moveCurrentSpeed = 0;
            bodyRB.linearVelocity = Vector3.zero;
            bodyRB.angularVelocity = Vector3.zero;
            wheelRB.linearVelocity = Vector3.zero;
            wheelRB.angularVelocity = Vector3.zero;
        }
    }

    private void MovePlayer()
    {
        if (wheelOnGround && !usingHook) // Prevents controlling the movement on air.
        {
            // Modifies the player's speed.
            moveCurrentSpeed = Mathf.MoveTowards(
                moveCurrentSpeed,
                (moveMaxSpeed * moveInput.x)
                    * (againstWallL || againstWallR ? 0.2f : 1) // Reduces the speed when against a wall.
                    + (wheelIsClippingL ? +0.1f : 0) + (wheelIsClippingR ? -0.1f : 0), // Helps to unclip the wheel
                moveAcceleration * Time.fixedDeltaTime
            );
        }

        if (!usingHook)
        {
            // Applies the player's speed.
            bodyRB.MovePosition(new Vector3(
                bodyRB.transform.position.x + moveCurrentSpeed / 10, // Divides it by 10 so the player doesn't go too fast.
                bodyRB.transform.position.y,
                0
            ));

            // Tilts the player according to the current speed.
            transform.rotation = Quaternion.Euler(0, 0, moveMaxTilt * -moveCurrentSpeed);
        } 
    }

    private void JointController()
    {
        if (!wheelOnGround)
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

        // Modifies the player's joint length.
        if (!usingHook)
        { 
            jointCurrentLength = Mathf.MoveTowards(
                jointCurrentLength,
                jointTargetLength,
                jointSpeed * Time.deltaTime
            );
        }

        // Applies the player's joint length.
        joint.connectedAnchor = new Vector3(
            joint.connectedAnchor.x,
            jointCurrentLength + (wheelOnGround ? moveInput.y / 10 : 0),
            joint.connectedAnchor.z
        );
    }

    private void HookControllerA()
    {
        if (joint.connectedBody == null && !hookedSomething)
        {
            hookBodyCurrentPos = 0;

            hookCurrentSpeed -= hookAcceleration;
            hookWheelCurrentPos += hookCurrentSpeed;

            wheelRB.transform.localPosition = new Vector3(0, -hookWheelCurrentPos, 0);

            if (wheelRB.transform.localPosition.y >= 0)
            {
                hookCurrentSpeed = hookStartSpeed;

                EndHook();
            }
        }
        else if (joint.connectedBody == null && hookedSomething)
        {
            Debug.Log("Got something!");

            wheelFixedPos = wheelRB.transform.position;

            lockWheelPos = true;
            lockBodyPos = false;

            hookCurrentSpeed -= hookAcceleration;
            hookBodyCurrentPos += hookCurrentSpeed;

            if (hookCurrentSpeed < 0)
            {
                //transform.position += new Vector3(0, -hookBodyCurrentPos, 0);
                //wheelFixedPos -= new Vector3(0, -hookCurrentSpeed, 0);
            }
            else
            {
                //EndHook();
            }
        }
    }

    private void PhysicsController()
    {
        // Creates a local gravity to each part.
        if (!usingHook)
        {
            bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
            wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);
        }

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
            jumpForce * 100 * (jointCurrentLength >= jointChargedLength ? (jointDefaultLength - jointCurrentLength) : jointChargedLength), // Prevents the player from jumping too high.
            0
        );
    }

    private void StartHook()
    {
        hookCurrentSpeed = hookStartSpeed;
        hookWheelCurrentPos = 0;

        bodyFixedPos = transform.position;
        transform.rotation = Quaternion.Euler(0, 0, hookAngle + 180);

        wheelRB.transform.localPosition = Vector3.zero;

        // Practically deactivates the player's joint.
        joint.connectedBody = null;

        lockBodyPos = true;

        usingHook = true;
    }

    private void EndHook()
    {
        usingHook = false;

        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Restores all movement.
        lockBodyPos = false;
        lockWheelPos = false;

        // Restores the joint.
        joint.connectedBody = wheelRB;
        jointCurrentLength = jointDefaultLength;
        wheelRB.transform.localPosition = Vector3.zero;
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
            if (wheelOnGround && !usingHook)
            {
                Jump();
            }

            chargingJump = false;
        }
    }
    #endregion
}
