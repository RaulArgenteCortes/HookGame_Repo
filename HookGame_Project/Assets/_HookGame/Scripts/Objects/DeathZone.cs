using UnityEngine;

public class DeathZone : MonoBehaviour
{
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
            playerMovement.Respawn();
        }
    }
}
