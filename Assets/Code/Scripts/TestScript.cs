using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private LayerMask layerToCheck;
    private RaycastHit[] colliders = new RaycastHit[10];
    void Update()
    {
        CharacterController charContr = GetComponent<CharacterController>();
        Vector3 p1 = transform.position + charContr.center + Vector3.up * -charContr.height * 0.5F;
        Vector3 p2 = p1 + Vector3.up * charContr.height;

        int colliderCount = Physics.CapsuleCastNonAlloc(p1, p2, charContr.radius, transform.forward, colliders, 1.25f, layerToCheck);

        // Cast character controller shape 10 meters forward to see if it is about to hit anything.
        if (colliderCount > 0)
        {
            for (int i = 0; i < colliderCount; i++)
                Debug.Log(colliders[i].transform.name);
        }

    }


}
