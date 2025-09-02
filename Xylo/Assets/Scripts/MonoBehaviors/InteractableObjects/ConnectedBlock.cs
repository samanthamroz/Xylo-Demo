using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConnectedBlock))]
public class ConnectedBlock : DraggableBlock {
    public List<ConnectedBlock> connectedBlocks = new();

    void Start() {
        foreach (ConnectedBlock block in connectedBlocks) {
            if (!block.connectedBlocks.Contains(GetComponent<ConnectedBlock>())) {
                block.connectedBlocks.Add(GetComponent<ConnectedBlock>());
            }
        }
    }
}