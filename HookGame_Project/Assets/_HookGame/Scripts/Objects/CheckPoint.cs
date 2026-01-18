using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("Mesh Stats")]
    [SerializeField] GameObject mesh;
    [SerializeField] Material materialOff;
    [SerializeField] Material materialOn;

    [Header("Player References")]
    [SerializeField] GameObject playerSpawn;

    private void Awake()
    {
        playerSpawn = GameObject.FindWithTag("PlayerSpawn");
    }

    private void Update()
    {
        if (playerSpawn.transform.position == transform.position)
        {
            mesh.GetComponent<Renderer>().material = materialOn;
        }
        else
        {
            mesh.GetComponent<Renderer>().material = materialOff;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerSpawn.transform.position = transform.position;
        }
    }
}
