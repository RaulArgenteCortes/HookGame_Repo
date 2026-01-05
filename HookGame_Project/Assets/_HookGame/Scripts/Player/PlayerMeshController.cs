using UnityEngine;

//[RequireComponent(typeof(PlayerMovement))]
public class PlayerMeshController : MonoBehaviour
{
    [Header("Mesh Components")]
    [SerializeField] GameObject wheelMesh;
    [SerializeField] GameObject disc3;
    [SerializeField] GameObject disc2;
    [SerializeField] GameObject disc1;
    [SerializeField] GameObject eyeA;
    [SerializeField] GameObject eyeB;
    [SerializeField] GameObject wheel;
    [SerializeField] GameObject spike;
    [SerializeField] GameObject connector;

    [Header("Transform stats")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    private float horizontalInput;
    private float hookAngle;

    [Header("Script references")]
    private PlayerMovement playerMovement;

    #region Awake/Start Functions
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        hookAngle = 180;
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        // Some variables need to be updated here.
        VariableUpdates();
    }

    private void VariableUpdates()
    {
        // Sets the hook rotation adapted to the mesh.
        if (playerMovement.moveInput != Vector2.zero && !playerMovement.usingHook)
        {
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(-playerMovement.moveInput.x, playerMovement.moveInput.y) * Mathf.Rad2Deg,
                45
            );
        }
    } 

    private void FixedUpdate()
    {
        InputChange();

        SpeedChange();

        DistanceChange();

        DirectionChange();

        LookChange();

        WheelChange();

        HookChange();
    }

    private void InputChange()
    {
        horizontalInput = Mathf.MoveTowards(
            horizontalInput,
            playerMovement.moveInput.x,
            3 * Time.fixedDeltaTime
        );

        // Rotates the disc3 in relation to the speed.
        disc3.transform.localEulerAngles = new Vector3(0, disc3.transform.localEulerAngles.y + (horizontalInput * 3), 0);
    }

    private void SpeedChange()
    {
        // Rotates the wheel in relation to the speed.
        if (!playerMovement.usingHook)
        {
            wheel.transform.localEulerAngles += new Vector3(0, wheelRB.linearVelocity.x * 5, 0);
        }
        else
        {
            wheel.transform.localEulerAngles = Vector3.zero;
        }

        // Prevents the rotation of the wheel from going to high or low.
        if (wheel.transform.localEulerAngles.y > 360)
        {
            wheel.transform.localEulerAngles += new Vector3(0, -360, 0);
        }
        if (wheel.transform.localEulerAngles.y < -360)
        {
            wheel.transform.localEulerAngles += new Vector3(0, +360, 0);
        }
    }

    private void DistanceChange()
    {
        // Rotates the disc2 in relation to the distance from the wheel.
        disc2.transform.localEulerAngles = new Vector3(0, playerMovement.jointDistance * 180, 0);
    }

    private void DirectionChange()
    {
        // Rotates the disc1 in relation to the hook rotation.
        disc1.transform.localRotation = Quaternion.RotateTowards(
            disc1.transform.localRotation,
            Quaternion.Euler(new Vector3(0, hookAngle, 0)),
            360 * 1.5f * Time.fixedDeltaTime
        );
    }

    private void LookChange()
    {
        // Rotates the eyeA in relation to the player input.
        eyeA.transform.localRotation = Quaternion.RotateTowards(
            eyeA.transform.localRotation,
            Quaternion.Euler(new Vector3(
                playerMovement.moveInput.y * 30,
                0,
                -playerMovement.moveInput.x * 30 + 180
            )),
            360 * 0.5f * Time.fixedDeltaTime
        );

        // Also rotates the other eye.
        eyeB.transform.localEulerAngles = -eyeA.transform.localEulerAngles;
    }

    private void WheelChange()
    {
        // Rotates the wheel mesh towards the body.
        wheelMesh.transform.rotation = Quaternion.Euler(
            Mathf.Atan2(
                -(bodyRB.transform.position.y - wheelMesh.transform.position.y),
                -(bodyRB.transform.position.x - wheelMesh.transform.position.x)
            ) * Mathf.Rad2Deg,
            -90,
            90
        );
    }

    private void HookChange()
    {
        // Makes the spike visible if using the hook
        if (!playerMovement.usingHook)
        {
            spike.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            spike.transform.localScale = Vector3.one;
        }

        // Rescales the connector in relation to the distance from the wheel.
        connector.transform.localScale = new Vector3(1, 1, playerMovement.jointDistance);
    }
    #endregion
}
