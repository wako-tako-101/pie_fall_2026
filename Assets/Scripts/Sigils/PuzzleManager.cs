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
    public void WindPlatePressed() => WindPlate = true;
    public void EarthPlatePressed() => EarthPlate = true;
    public void WaterPlatePressed() => WaterPlate = true;
    public void FirePlatePressed() => FirePlate = true;

}
