using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [Header("Bouncer Stats")]
    [SerializeField] float bounceForce;

    [Header("Mesh Stats")]
    [SerializeField] GameObject mesh;
    [SerializeField] Material materialOff;
    [SerializeField] Material materialOn;

    [Header("Player References")]
    [SerializeField] GameObject player;
    [SerializeField] Rigidbody playerRB;
    [SerializeField] PlayerMovement playerMovement;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");

        playerRB = player.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerMovement.hookedSomething)
            {
                playerMovement.EndHook();

                playerRB.AddForce(new Vector3(
                    (bounceForce/2) * playerMovement.hookAngleVector.x,
                    (bounceForce/4) * -playerMovement.hookAngleVector.y + bounceForce,
                    0
                ), ForceMode.Impulse);
            }
        }

        // Changes the material
        if (other.CompareTag("PlayerWheel"))
        {
            if (playerMovement.hookedSomething)
            {
                mesh.GetComponent<Renderer>().material = materialOn;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerMovement.hookedSomething)
            {
                playerMovement.EndHook();
            }
        }

        if (other.CompareTag("PlayerWheel"))
        {
            playerMovement.wheelLockPosition = transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Changes the material
        if (other.CompareTag("PlayerWheel"))
        {
            mesh.GetComponent<Renderer>().material = materialOff;
        }

        if (other.CompareTag("Player"))
        {
            playerMovement.canUseHook = true;
        }
    }
}