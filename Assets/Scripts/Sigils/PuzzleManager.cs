using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public bool FirePlate = false;
    public bool WaterPlate = false;
    public bool EarthPlate = false;
    public bool WindPlate = false;
    [SerializeField] GameObject door;

    public void CompleteSequence()
    {
        if (FirePlate && WaterPlate && EarthPlate && WindPlate)
        {
            door.SetActive(false);
        }
    }
    public void FirePlateDown()
    {
        FirePlate = true;
    }
    public void WaterPlateDown()
    {
        WaterPlate = true;
    }
    public void EarthPlateDown()
    {
        EarthPlate = true;
    }
    public void WindPlateDown()
    {
        WindPlate = true;
    }
}
