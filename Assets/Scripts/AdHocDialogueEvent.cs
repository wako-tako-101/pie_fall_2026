using UnityEngine;
using UnityEngine.Events;

public class AdHocDialogueEvent : MonoBehaviour
{
    public bool atTerminalForTest = false;
    public bool testDone = false;
    [SerializeField] UnityEvent DoTest;
    public void DoTestShock()
    {
        if (atTerminalForTest && !testDone)
        {
            testDone = true;
            DoTest.Invoke();
        }
    }
    public void AtTerminalIsYes() => atTerminalForTest = true;
}
