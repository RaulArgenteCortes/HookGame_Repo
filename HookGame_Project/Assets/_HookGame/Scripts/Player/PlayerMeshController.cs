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
    [SerializeField] GameObject connector;

    [Header("Transform stats")]
    [SerializeField] float horizontalInput;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] float hookAngle;

    [Header("Script references")]
    [SerializeField] PlayerController playerController;

    #region Awake/Start Functions
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }
    #endregion

    #region Update Functions
    void Update()
    {
        InputChange();

        SpeedChange();

        DistanceChange();

        DirectionChange();

        LookChange();
    }
    
    private void InputChange()
    {
        // Sets the player's horizontal input whit a transition.
        horizontalInput = Mathf.MoveTowards(
            horizontalInput,
            playerController.moveInput.x,
            0.1f * Time.fixedDeltaTime
        );

        // Rotates the disc3 in relation to the speed.
        disc3.transform.localEulerAngles = new Vector3(0, disc3.transform.localEulerAngles.y + (horizontalInput / 2), 0);
    }

    private void SpeedChange()
    {
        // Rotates the wheel in relation to the speed.
        wheel.transform.localEulerAngles = new Vector3(0, wheel.transform.localEulerAngles.y + (playerController.currentSpeed*2), 0);
    }

    private void DistanceChange()
    {
        // Rotates the disc2 in relation to the distance from the wheel.
        disc2.transform.localEulerAngles = new Vector3(0, wheelRB.transform.localPosition.y * 180, 0);

        // Rescales the connector in relation to the distance from the wheel.
        connector.transform.localScale = new Vector3(100, 100, 100 * wheelRB.transform.localPosition.y);
    }

    private void DirectionChange()
    {
        // Sets the hook rotation.
        if (playerController.moveInput != Vector2.zero)
        {
            hookAngle = Snapping.Snap(
                -Mathf.Atan2(-playerController.moveInput.x, playerController.moveInput.y) * Mathf.Rad2Deg,
                45
            );
        }

        // Rotates the disc1 in relation to the hook rotation.
        disc1.transform.localRotation = Quaternion.RotateTowards(
            disc1.transform.localRotation,
            Quaternion.Euler(new Vector3(0, hookAngle, 0)),
            360 * 2 * Time.deltaTime
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
            360 * Time.deltaTime
        );

        // Also changes the other eye.
        eyeB.transform.localEulerAngles = eyeA.transform.localEulerAngles;
    }
    #endregion
}
