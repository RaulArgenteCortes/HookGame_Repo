using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Physics Stats")]
    [SerializeField] float bodyWeight;
    [SerializeField] float wheelWeight;

    [Header("Movement Stats")]
    [SerializeField] Vector2 moveInput;
    [SerializeField] float currentSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float maxTilt;

    [Header("Object References")]
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Rigidbody wheelRB;
    [SerializeField] GameObject bodyMesh;
    [SerializeField] GameObject wheelMesh;

    private void FixedUpdate()
    {
        MovePlayer();

        TiltPlayer();
    }

    private void LateUpdate()
    {
        CreateGravity();
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

    private void CreateGravity()
    {
        bodyRB.AddForce(new Vector3(0, -bodyWeight, 0), ForceMode.Acceleration);
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
