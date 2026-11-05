using UnityEngine;

public static class SigilTags
{
    public enum SigilTag {Levitate, Push, Freeze,  Burn, Electricute}
}
public class SigilTagsMono : MonoBehaviour
{
    [field: SerializeField] public SigilTags.SigilTag SigilTag {private set; get;}

    public bool CompareSigilTag(SigilTags.SigilTag otherTag)
    {
        if (otherTag == SigilTag) return true;
        else return false;
    }
}
