using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility 
{

    public static float GetDotProduct(Vector3 vectorA, Vector3 vectorB)
    {
        Vector3 direction = (vectorA - vectorB).normalized;

        float dotProduct = Vector3.Dot(vectorA, direction);

        return dotProduct;

    }
}
