using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerMeshController : MonoBehaviour
{
    [Header("Mesh Components")]
    [SerializeField] GameObject disc3;
    [SerializeField] GameObject disc2;
    [SerializeField] GameObject disc1;
    [SerializeField] GameObject eyeA;
    [SerializeField] GameObject eyeB;
    [SerializeField] GameObject wheel;
    [SerializeField] GameObject spike;
    [SerializeField] GameObject connector;

    [Header("Transform stats")]
    [SerializeField] Rigidbody wheelRB;
    private float horizontalInput;
    private float hookAngle;

    [Header("Script references")]
    private PlayerController playerController;

    #region Awake/Start Functions
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    #endregion

    #region Update Functions
    private void Update()
    {
        // Changes that need to be made on Update().
        UpdateChanges();
    }

    private void FixedUpdate()
    {
        InputChange();

        SpeedChange();

        DistanceChange();

        DirectionChange();

        LookChange();

        HookChange();
    }

    private void UpdateChanges()
    {
        // Sets the player's horizontal input whit a transition.
        horizontalInput = Mathf.MoveTowards(
            horizontalInput,
            playerController.moveInput.x,
            3 * Time.fixedDeltaTime
        );

        // Sets the hook rotation adapted to the mesh.
        if (playerController.moveInput != Vector2.zero && !playerController.usingHook)
        {
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(-playerController.moveInput.x, playerController.moveInput.y) * Mathf.Rad2Deg,
                45
            );
        }
    }


    private void InputChange()
    {
        // Rotates the disc3 in relation to the speed.
        disc3.transform.localEulerAngles = new Vector3(0, disc3.transform.localEulerAngles.y + (horizontalInput * 3), 0);
    }

    private void SpeedChange()
    {
        // Rotates the wheel in relation to the speed.
        if (!playerController.usingHook)
        {
            wheel.transform.localEulerAngles = new Vector3(0, wheel.transform.localEulerAngles.y + (playerController.moveCurrentSpeed * 10), 0);
        }
        else
        {
            wheel.transform.localEulerAngles = Vector3.zero;
        }   
    }

    private void DistanceChange()
    {
        // Rotates the disc2 in relation to the distance from the wheel.
        disc2.transform.localEulerAngles = new Vector3(0, wheelRB.transform.localPosition.y * 180, 0);

        // Rescales the connector in relation to the distance from the wheel.
        connector.transform.localScale = new Vector3(1, 1, wheelRB.transform.localPosition.y);
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
                playerController.moveInput.y * 30,
                0,
                -playerController.moveInput.x * 30 + 180
            )),
            360 * 0.5f * Time.deltaTime
        );

        // Also rotates the other eye.
        eyeB.transform.localEulerAngles = -eyeA.transform.localEulerAngles;
    }

    private void HookChange()
    {
        // Makes the spike visible if using the hook
        if (!playerController.usingHook)
        {
            spike.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            spike.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    #endregion
}
