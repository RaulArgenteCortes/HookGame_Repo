using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;

    [Header("Movement Stats")]
    public Vector2 moveInput;
    [SerializeField] float moveAccelerationSpeed;
    [SerializeField] float moveMaxSpeed;
    // Debugging:
    [SerializeField] float bodyLinealSpeedX;
    [SerializeField] float wheelLinealSpeedX;

    [Header("Joint Stats")]
    [SerializeField] float jointDefaultLength;
    [SerializeField] float jointChargedLength;
    [SerializeField] float jointRecoiledLength;
    [SerializeField] float jointLowResistence;
    [SerializeField] float jointHighResistence;
    private float jointCurrentLenght;

    [Header("LayerCheck Stats")]
    [SerializeField] float CheckRadius;
    [SerializeField] LayerMask groundLayer;
    //[SerializeField] LayerMask interactableLayer;
    //private bool bodyOnGround;
    private bool wheelOnGround;
    //private bool wheelIsClippingL;
    //private bool wheelIsClippingR;

    [Header("LayerChecks")]
    //[SerializeField] GameObject bodyCheckBottom;
    //[SerializeField] GameObject wheelCheckTopL;
    //[SerializeField] GameObject wheelCheckTopR;

    [Header("External References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] SphereCollider wheelCollider;
    [SerializeField] SpringJoint joint;

    #region Update Functions
    private void Update()
    {
        LayerCheck();

        bodyLinealSpeedX = bodyRB.linearVelocity.x;
        wheelLinealSpeedX = wheelRB.linearVelocity.x;
    }

    private void LayerCheck()
    {
        wheelOnGround = Physics.CheckSphere(wheelRB.transform.position, CheckRadius + wheelCollider.radius, groundLayer);
    }

    private void FixedUpdate()
    {
        MovePlayer();

        JointController();

        PhysicsController();
    }

    private void MovePlayer()
    {
        // Moves the player if the player is on ground and the speed isn't too fast.
        if (wheelOnGround && Mathf.Abs(bodyRB.linearVelocity.x) < moveMaxSpeed * Mathf.Abs(moveInput.x))
        {
            bodyRB.AddForce(new Vector3(moveAccelerationSpeed * moveInput.x, 0, 0), ForceMode.Force);
        }
    }

    private void JointController()
    {
        jointCurrentLenght = jointDefaultLength;

        // Modifies the joint lenght.
        joint.connectedAnchor = new Vector3(
            0,
            jointCurrentLenght + moveInput.y * 0.05f,
            0
        );

        // Modifies the joint resistence.
        joint.damper = jointLowResistence;
    }

    private void PhysicsController()
    {
        bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
        wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);

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

    #region Input Functions
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            
        }

        if (context.canceled)
        {
            
        }
    }
    #endregion
}
