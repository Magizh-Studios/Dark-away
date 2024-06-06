using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public enum LookMode { LookAt,LookAtInverted,CameraForward,CameraForwardInverted }

    [SerializeField] private LookMode lookMode;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
    void LateUpdate()
    {
        switch(lookMode)
        {
            case LookMode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case LookMode.LookAtInverted:
                Vector3 dir = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dir);
                break;
            case LookMode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case LookMode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}
