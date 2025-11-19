using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float playerWeight;
    [SerializeField] float wheelWeight;

    [Header("Movement Stats")]
    [SerializeField] Vector2 moveInput;
    [SerializeField] float moveDirection;

    [Header("Object References")]
    [SerializeField] Rigidbody playerRB;
    [SerializeField] Rigidbody wheelRB;

    private void FixedUpdate()
    {
        MovePlayer();

        //TiltPlayer();
    }

    private void LateUpdate()
    {
        CreateGravity();
    }

    private void MovePlayer()
    {
        moveDirection = moveInput.x;

        transform.position = new Vector3 (
            transform.position.x + moveDirection/10,
            transform.position.y,
            transform.position.z
        );
    }

    private void TiltPlayer()
    {
        transform.position = new Vector3(
            transform.position.x + moveDirection / 10,
            transform.position.y,
            transform.position.z
        );
    }

    private void CreateGravity()
    {
        playerRB.AddForce(new Vector3(0, -playerWeight, 0), ForceMode.Acceleration);
        wheelRB.AddForce(new Vector3(0, -wheelWeight, 0), ForceMode.Acceleration);
    }

    #region Input Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        
    }
    #endregion
}
