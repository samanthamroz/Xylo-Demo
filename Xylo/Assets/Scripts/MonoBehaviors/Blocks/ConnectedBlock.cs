using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ConnectedBlock))]
[RequireComponent(typeof(AudioSource))]
public class ConnectedBlock : DraggableBlock {
    public List<GameObject> connectedBlocks = new();

    public override void DoClick() {
        GetComponent<AudioSource>().Play();
        this.ToggleAllHandles(true, false);
        foreach (GameObject block in connectedBlocks) {
            if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                temp.ToggleAllHandles(true, false);
            }
        }
    }
    public override void DoClickAway() {
        this.ToggleAllHandles(false, true);
        foreach (GameObject block in connectedBlocks) {
            if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                temp.ToggleAllHandles(false, true);
            }
        }
    }
    public override void ToggleAllHandles(bool isOn, bool turnInvisible) {
        DraggableBlock d = (DraggableBlock)this;
        for (int i = 0; i < handles.Count; i++) {
            DraggableBlockHandle handle = handles[i];
            handle.gameObject.SetActive(isOn);

            Vector3 amountToMove = new Vector3(handle.direction.x / 4, handle.direction.y / 4, 0);
            Vector3 testPosition = transform.position + amountToMove;

            bool anyCollisions = false;
            foreach (GameObject block in connectedBlocks) {
                if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                    if (temp.IsSelfCollidingAtPosition(block.transform.position + amountToMove)) {
                        anyCollisions = true;
                        break;
                    }
                }
            }
            if (anyCollisions == false) {
                anyCollisions = IsSelfCollidingAtPosition(testPosition);
            }

            if (turnInvisible) {
                handle.ToggleInvisible(anyCollisions);
                foreach (GameObject block in connectedBlocks) {
                    if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                        temp.handles[i].ToggleInvisible(anyCollisions);
                    }
                }
            }
            else {
                handle.ToggleGrey(anyCollisions);
                foreach (GameObject block in connectedBlocks) {
                    if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                        temp.handles[i].ToggleGrey(anyCollisions);
                    }
                }
            }
        }
    }
}