using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(Collider))]
public class TriggerZone : MonoBehaviour
{
    [SerializeField] private GameObject detectionObject;
    [SerializeField] private TriggerType triggerType;
    [SerializeField] private UnityEvent OnPlayerEntered,OnPlayerExited;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == detectionObject)
        {
            OnPlayerEntered?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(triggerType == TriggerType.Normal)
        {
            OnPlayerExited.Invoke();
        }
        else
        {
            if (other.gameObject == detectionObject && IsExitingFromDesiredDir(other.gameObject.transform))
            {
                OnPlayerExited.Invoke();
            }
        }
       
    }

    private bool IsExitingFromDesiredDir(Transform player)
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.up, dir);
        Debug.Log($"Exiting Dot {dot}");
        return dot > 0;
    }

    private void Update()
    {
        //TestingDot();
    }

    private void TestingDot()
    {
        Vector3 dir = (detectionObject.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.up, dir);
        Debug.Log(dot);
    }
}
public enum TriggerType
{
    Normal,
    DotMode
}
