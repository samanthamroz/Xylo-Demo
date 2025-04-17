using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public abstract void DoClick();
        //must override in children

    public virtual void DoRelease() {
        //do nothing, but can override if necessary
    }
    
    public virtual void DoClickAway() {
        //do nothing, but can override if necessary
    }
}