using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform targetFollowTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float lerpSpeed = 1f;

    private readonly Vector3 offsetValue = new Vector3(0f, 7f, -2f);

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetFollowTransform.position + offset, lerpSpeed * Time.deltaTime);
    }

    public void AdjustOffsetBy(Vector3 newOffset)
    {
        offset += newOffset;
    }

    public void RevertOffset()
    {
        offset = offsetValue;
    }
}
