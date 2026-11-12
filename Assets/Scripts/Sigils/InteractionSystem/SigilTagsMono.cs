using UnityEngine;

namespace SigilInteractions
{    public static class SigilTags
    {
        //None means no sigil is loaded
        //scenes save these by number, so add new sigils at the end and never reorder them
        public enum SigilTag {Levitate, Push, Freeze,  Burn, Electricute, None}
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
}
