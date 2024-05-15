using UnityEngine;

public static class Utils
{

    public static float GetDotProduct(Vector3 vectorA, Vector3 vectorB)
    {
        Vector3 direction = (vectorA - vectorB).normalized;

        float dotProduct = Vector3.Dot(vectorA, direction);

        return dotProduct;

    }
}
