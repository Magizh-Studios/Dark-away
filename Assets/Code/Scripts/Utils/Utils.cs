using UnityEngine;

public static class Utils
{

    public static float GetDotProduct(Vector3 vectorA, Vector3 vectorB)
    {
        Vector3 direction = (vectorB - vectorA).normalized;

        float dotProduct = Vector3.Dot(vectorA, direction);
        Debug.Log(dotProduct);

        return dotProduct;

    }
}
