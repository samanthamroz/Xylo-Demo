using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationBubble : InteractableObject
{
    public GameObject jumpTo;
    public override void DoClick() {
        //CameraManager.self.MoveLookAtPosition(jumpTo.transform.position);
    }
}
