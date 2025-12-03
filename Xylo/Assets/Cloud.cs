using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] Vector3 endingPos;
    Vector3 startingPos;
    public bool isMoved;

    void Start() {
        startingPos = transform.position;
    }

    public void DoMoveToEnd() {
        if (isMoved) return;
        
        LeanTween.move(gameObject, endingPos, 1f).setEaseInOutCirc();
        isMoved = true;
    }

    public void DoMoveToStart() {
        if (!isMoved) return;

        LeanTween.move(gameObject, startingPos, 1f).setEaseInOutCirc();
        isMoved = false;
    }
}
