using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {
    public class GunWithClip : MonoBehaviour
    {
        public float FireInterval;
        public int ClipSize;
        public float ReloadTime;
        public GameObject Nozzle;
        public GameObject Bullet;
        public GameObject FireEffect;

        int clipRemaining;
        float cooldown;

        public bool IsReady { get { return cooldown <= float.Epsilon; } }
        public bool IsEmptyClip { get { return clipRemaining == 0; } }
        public bool IsReloading { get; private set; }
        public float ReloadFraction { get { return IsReloading ? (1f - cooldown / ReloadTime) : 1f; } }

        public void Fire()
        {
            if (IsReady && !IsEmptyClip)
            {
                var bullet = Instantiate(Bullet, Nozzle.transform.position, Nozzle.transform.rotation) as GameObject;
                var raySensor = bullet.GetComponent<RaySensor>();
                raySensor.IgnoreList.Clear();
                raySensor.IgnoreList.Add(gameObject);
                var effect = Instantiate(FireEffect, Nozzle.transform.position, Nozzle.transform.rotation) as GameObject;
                effect.transform.SetParent(Nozzle.transform);
                cooldown = FireInterval;
                clipRemaining--;
            }
        }

        public void Reload()
        {
            if (IsReady && clipRemaining < ClipSize && !IsReloading)
            {
                StopCoroutine("FiringRoutine");
                StartCoroutine("ReloadRoutine");
            }
        }

        void Start()
        {
            StartCoroutine("FiringRoutine");
        }

        IEnumerator FiringRoutine()
        {
            cooldown = 0f;
            clipRemaining = ClipSize;
            IsReloading = false;
            while (true)
            {
                cooldown = Mathf.Max(cooldown - Time.deltaTime, 0f);
                yield return null;
            }
        }

        IEnumerator ReloadRoutine()
        {
            IsReloading = true;
            cooldown = ReloadTime;
            while (true)
            {
                cooldown = Mathf.Max(cooldown - Time.deltaTime, 0f);
                if (IsReady)
                {
                    StartCoroutine("FiringRoutine");
                    yield break;
                }
                yield return null;
            }
        }
    }
}