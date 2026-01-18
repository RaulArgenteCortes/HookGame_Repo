using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    #region Input Functions
    public void OnQuit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Application.Quit();
            Debug.Log("Quit Game");
        }
    }
    #endregion
}
