using UnityEngine;

public class CreateInstance : MonoBehaviour
{
    public static CreateInstance instance;

    private void Awake()
    {
        // Makes sure that there's always 1 instance.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
