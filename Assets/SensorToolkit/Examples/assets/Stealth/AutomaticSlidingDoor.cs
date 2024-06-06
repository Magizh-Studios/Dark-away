using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    public class AutomaticSlidingDoor : MonoBehaviour
    {
        public Sensor ObjectSensor;
        public GameObject LeftDoor;
        public GameObject RightDoor;
        public float SlideAmount;
        public float Speed;

        Vector3 leftStart;
        Vector3 rightStart;

        void Start()
        {
            leftStart = LeftDoor.transform.localPosition;
            rightStart = RightDoor.transform.localPosition;
            StartCoroutine(ClosingState());
        }

        IEnumerator ClosingState()
        {
            Start:

            if (ObjectSensor.GetNearestDetection() != null)
            {
                StartCoroutine(OpeningState()); yield break;
            }

            LeftDoor.transform.localPosition = Vector3.Lerp(LeftDoor.transform.localPosition, leftStart, Time.deltaTime * Speed);
            RightDoor.transform.localPosition = Vector3.Lerp(RightDoor.transform.localPosition, rightStart, Time.deltaTime * Speed);

            yield return null;
            goto Start;
        }

        IEnumerator OpeningState()
        {
            Start:

            if (ObjectSensor.GetNearestDetection() == null)
            {
                StartCoroutine(ClosingState()); yield break;
            }

            LeftDoor.transform.localPosition = Vector3.Lerp(LeftDoor.transform.localPosition, leftStart - Vector3.right * SlideAmount, Time.deltaTime * Speed);
            RightDoor.transform.localPosition = Vector3.Lerp(RightDoor.transform.localPosition, rightStart + Vector3.right * SlideAmount, Time.deltaTime * Speed);

            yield return null;
            goto Start;
        }
    }
}