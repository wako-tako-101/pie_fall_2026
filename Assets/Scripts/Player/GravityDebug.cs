using UnityEngine;

public class GravityDebugger : MonoBehaviour
{
    public void OnGroundedChanged(bool grounded)
    {
        Debug.Log("Player Grounded: " + grounded);
    }
}