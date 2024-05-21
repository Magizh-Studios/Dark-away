using UnityEngine;

public static class Utils
{

    public static float GetDotProduct(Vector3 itemPos, Vector3 targetForward, Vector3 targetPos)
    {
        Vector3 directionToTarget = (itemPos - targetPos).normalized;

        float dotProduct = Vector3.Dot(targetForward, directionToTarget);

        return dotProduct;

    }
}
