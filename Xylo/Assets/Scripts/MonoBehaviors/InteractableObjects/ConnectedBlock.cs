using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConnectedBlock))]
public class ConnectedBlock : DraggableBlock {
    public List<GameObject> connectedBlocks = new();

    void Start() {

    }
}