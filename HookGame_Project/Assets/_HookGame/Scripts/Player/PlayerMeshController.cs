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
    [SerializeField] float rotateInput;
    [SerializeField] Rigidbody wheelRB;

    [Header("Script references")]
    [SerializeField] PlayerController playerController;

    #region Update Functions
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
    }
    
    private void InputChange()
    {
        rotateInput = Mathf.MoveTowards(
            rotateInput,
            playerController.moveInput.x / 2,
            0.1f * Time.fixedDeltaTime
        );

        disc3.transform.localEulerAngles = new Vector3(0, disc3.transform.localEulerAngles.y + rotateInput, 0);
    }

    private void SpeedChange()
    {
        wheel.transform.localEulerAngles = new Vector3(0, wheel.transform.localEulerAngles.y + (playerController.currentSpeed*2), 0);
    }

    private void DistanceChange()
    {
        disc2.transform.localEulerAngles = new Vector3(0, wheelRB.transform.localPosition.y * 180, 0);
    }

    private void DirectionChange()
    {
        //eyeA.transform.localEulerAngles = new Vector3(0, wheelRB.transform.localPosition.y * 180, 0);

        // Also changes the other eye.
        eyeB.transform.localEulerAngles = eyeA.transform.localEulerAngles;
    }
    #endregion
}
