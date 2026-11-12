using UnityEngine;

namespace SigilInteractions
{
    [RequireComponent(typeof(ObjectState), typeof(Rigidbody))]
    public class LevitateState : MonoBehaviour
    {
        Transform originalPosition;
        GameObject casterReference;
        Vector3 offset;
        bool inPosition = false;
        [SerializeField] float levitateSpeed = 6f;
        public void ChangeState(GameObject caster)
        {
            TryGetComponent(out ObjectState objState);

            if (objState.GetState() != ObjectStateTags.States.WindControlled)
            {
                //if not in a wind controlled state, initiate said state
                objState.SetState(ObjectStateTags.States.WindControlled);
                
                //start wind control sequence
                InitiateWindControl(caster);           
            }
            else if (objState.GetState() == ObjectStateTags.States.WindControlled && !IsSameCaster(caster))
            {
                return;
            }
            else
            {
                //when no longer wind controlled return state to normal
                objState.SetState(ObjectStateTags.States.Normal);
                
                //reset inPosition flag
                inPosition = false;

                //clear reference when no longer wind controlled
                casterReference = null;

                //clear offset
                offset = Vector3.zero;

                //enable gravity when the object is no longer in wind controlled state
                TryGetComponent(out Rigidbody rb);
                if (rb != null) rb.useGravity = true;
            }
        }
        void Update()
        {
            if (GetComponent<ObjectState>().GetState() == ObjectStateTags.States.WindControlled)
            {
                if (!inPosition)
                {
                    TryGetComponent(out Rigidbody rb);
                    if (rb != null)
                    {
                        //position object in front of caster
                        rb.linearVelocity = new Vector3(0, casterReference.transform.position.y - transform.position.y, 0) * levitateSpeed;
                        transform.position = new Vector3(casterReference.transform.position.x + offset.x, transform.position.y, casterReference.transform.position.z + offset.z);
                        
                        //once reached position, set inPosition flag to true
                        if (transform.position.y >= casterReference.transform.position.y) inPosition = true;
                    }
                }
                else SyncMovement(offset);
            }
        }
        void InitiateWindControl(GameObject caster)
        {
            //get the origin reference and offset for the relative position
            GetOrigin(caster);
            offset = originalPosition.position - casterReference.transform.position;
            offset.y = 0;

            //disable gravity while in wind controlled state
            TryGetComponent(out Rigidbody rb);
            if (rb != null) rb.useGravity = false;
        }
        void SyncMovement(Vector3 offset)
        {
            //use caster reference to set position of object always in front of the controller
            //in reference to the original position of the object
            transform.position = casterReference.transform.position + offset;
            Debug.Log("Object position set to: " + casterReference.transform.position + offset);
        }
        public void GetOrigin(GameObject origin) 
        {
            //save this object's original position and obtain reference to the caster
            originalPosition = transform;
            casterReference = origin;
        }
        public bool IsSameCaster(GameObject caster)
        {
            return casterReference == caster;
        }
    }
}