using UnityEngine;

public class Unparent : MonoBehaviour
{
    void Start()
    {
        transform.parent = null;
    }
}
