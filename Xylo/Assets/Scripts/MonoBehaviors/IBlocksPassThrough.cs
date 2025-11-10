using UnityEngine;

public interface IBlocksPassThrough {
    public bool CanPassThrough(Vector3 movementDirection);
}