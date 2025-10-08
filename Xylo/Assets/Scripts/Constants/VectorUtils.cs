using UnityEngine;
public class VectorUtils
{
    public static Vector3 nullVector = new(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    
    public static bool IsNullVector(Vector3 vector)
    {
        return float.IsInfinity(vector.x) && float.IsInfinity(vector.y) && float.IsInfinity(vector.z);
    }
    public static Vector3 GetAbsVector(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }
    public static Vector3 GetSnapToGridVector(Vector3 originalPosition, Vector3 targetVector)
    {
        float Yincrement = 0.5f;
        float XZincrement = 1f;

        float snappedX = originalPosition.x + Mathf.Round((targetVector.x - originalPosition.x) / XZincrement) * XZincrement;
        float snappedY = originalPosition.y + Mathf.Round((targetVector.y - originalPosition.y) / Yincrement) * Yincrement;
        float snappedZ = originalPosition.z + Mathf.Round((targetVector.z - originalPosition.z) / XZincrement) * XZincrement;

        return new Vector3(snappedX, snappedY, snappedZ);
    }

    public static Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Mathf.Round(vec.x), (float)(Mathf.Round(vec.y * 2) / 2), (float)Mathf.Round(vec.z));
    }
}