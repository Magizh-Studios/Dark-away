using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    public class AlarmController : MonoBehaviour {
        public Light PointLight;
        public Color AlarmColour;
        public float FlashFrequency;

        bool alarmStarted = false;

        public static AlarmController Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<AlarmController>();
                }
                return instance;
            }
        }
        static AlarmController instance;

        public bool IsAlarmState => alarmStarted && WhoTrippedAlarm != null;
        public GameObject WhoTrippedAlarm { get; private set; }

        public void StartAlarm(GameObject whoTrippedAlarm) {
            if (!alarmStarted) {
                WhoTrippedAlarm = whoTrippedAlarm;
                StartCoroutine(alarmRoutine());
            }
        }

        IEnumerator alarmRoutine() {
            alarmStarted = true;
            PointLight.color = AlarmColour;
            var startIntensity = PointLight.intensity;

            while (true) {
                var intensity = (Mathf.Sin(FlashFrequency * Time.time * Mathf.PI * 2f) + 1f) / 2f * startIntensity;
                PointLight.intensity = intensity;
                yield return null;
            }
        }
    }
}
