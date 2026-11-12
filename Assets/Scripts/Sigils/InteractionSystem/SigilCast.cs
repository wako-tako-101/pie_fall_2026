using UnityEngine;
using UnityEngine.InputSystem;
using SigilInteractions;

namespace SigilCasting
{
    //holds the loaded sigil and casts it
    //a drawn sigil is "pending" until the player saves it (cast button) or discards it
    //after a cast the system goes back to None, which does nothing when cast
    public class SigilCast : MonoBehaviour
    {
        //system
        [SerializeField] Transform castSource;
        [Tooltip("The saved sigil. Always starts as None, shown here for debugging.")]
        [SerializeField] SigilTags.SigilTag activeSigilType = SigilTags.SigilTag.None;
        [Tooltip("How far a cast can reach, in meters.")]
        [SerializeField, Min(0f)] float castRange = 5f;
        [Tooltip("Button that throws away a drawn sigil before it is saved.")]
        [SerializeField] InputAction discardAction = new InputAction("Discard", InputActionType.Button, "<XRController>{RightHand}/secondaryButton");
        [Tooltip("Optional particle visuals. Found on a child object if left empty.")]
        [SerializeField] SigilChargeEffect chargeEffect;

        //the sigil that was drawn but not saved yet
        SigilTags.SigilTag pendingSigilType = SigilTags.SigilTag.None;
        SigilDrawer drawer;

        public SigilTags.SigilTag ActiveSigil => activeSigilType;
        public bool HasPendingSigil => pendingSigilType != SigilTags.SigilTag.None;

        //methods
        void Awake()
        {
            drawer = GetComponent<SigilDrawer>();
            if (chargeEffect == null) chargeEffect = GetComponentInChildren<SigilChargeEffect>();
            //nothing is loaded until the player draws and saves a sigil
            activeSigilType = SigilTags.SigilTag.None;
        }
        void OnEnable()
        {
            discardAction.performed += OnDiscard;
            discardAction.Enable();
        }
        void OnDisable()
        {
            discardAction.performed -= OnDiscard;
            discardAction.Disable();
        }
        SigilTags.SigilTag Parse(string sigilName)
        {
            switch (sigilName)
            {
                case "Levitate":
                    return SigilTags.SigilTag.Levitate;
                case "Electricute":
                    return SigilTags.SigilTag.Electricute;
                default:
                    Debug.Log("Cast does not match any implemented sigil");
                    return SigilTags.SigilTag.None;
            }
        }
        //holds a drawn sigil until the player saves or discards it
        //returns false if the name is not an implemented sigil
        public bool Propose(string sigilName)
        {
            pendingSigilType = Parse(sigilName);
            return HasPendingSigil;
        }
        //drops the drawn sigil without touching the saved one
        public void ClearPending()
        {
            pendingSigilType = SigilTags.SigilTag.None;
        }
        //saves a sigil directly, skipping the confirm step
        public void Load(string sigilName)
        {
            SetActive(Parse(sigilName));
        }
        void SetActive(SigilTags.SigilTag sigil)
        {
            activeSigilType = sigil;
            if (chargeEffect == null) return;

            if (sigil == SigilTags.SigilTag.None) chargeEffect.Hide();
            else chargeEffect.ShowLoaded(sigil);
        }
        void SavePending()
        {
            SetActive(pendingSigilType);
            pendingSigilType = SigilTags.SigilTag.None;
            if (drawer != null) drawer.FadeOutStroke();
        }
        void OnDiscard(InputAction.CallbackContext context)
        {
            if (!HasPendingSigil) return;

            pendingSigilType = SigilTags.SigilTag.None;
            if (drawer != null) drawer.FadeOutStroke();
        }
        public void OnCast(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            //the cast button first saves a freshly drawn sigil
            if (HasPendingSigil)
            {
                SavePending();
                return;
            }
            if (activeSigilType == SigilTags.SigilTag.None)
            {
                Debug.Log("No active sigil to cast");
                return;
            }

            Vector3 castOrigin = castSource.position;
            Vector3 castDirection = castSource.forward;

            GameObject target = null;
            Vector3 targetPoint = castOrigin + castDirection * castRange;
            if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, castRange))
            {
                target = hit.collider.gameObject;
                targetPoint = hit.point;
            }

            //the sigil is used up by the cast, whether or not it lands
            SigilTags.SigilTag castSigil = activeSigilType;
            activeSigilType = SigilTags.SigilTag.None;

            if (chargeEffect != null) chargeEffect.Launch(castSigil, targetPoint, target, () => Resolve(castSigil, target));
            else Resolve(castSigil, target);
        }
        //applies the sigil to the target, returns true if it worked
        bool Resolve(SigilTags.SigilTag sigil, GameObject target)
        {
            if (target == null)
            {
                Debug.Log("No object hit by sigil cast");
                return false;
            }

            //check if target is valid
            if (target.TryGetComponent(out SigilTagsMono sigilTag)
                && sigilTag.CompareSigilTag(sigil))
            {
                //get object state and initiate sigil interaction
                if (target.TryGetComponent(out ObjectState objState))
                {
                    objState.Interact(gameObject, sigil);
                    return true;
                }
                Debug.LogError("No ObjectState script attached to " + target.name);
                return false;
            }

            Debug.Log("Target is not a valid sigil target");
            return false;
        }
    }
}
