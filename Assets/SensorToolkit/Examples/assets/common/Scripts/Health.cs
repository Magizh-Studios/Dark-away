using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    public class Health : MonoBehaviour
    {
        public float MaxHP;
        public GameObject Corpse;

        Rigidbody rb;

        public float HP { get; private set; }

        public void Impact(float amount, Vector3 impactForce, Vector3 impactPoint)
        {
            HP -= amount;
            if (HP <= 0f)
            {
                var corpse = Instantiate(Corpse, transform.position, transform.rotation) as GameObject;
                corpse.transform.SetParent(transform.parent);

                var myTeam = GetComponent<TeamMember>();
                var corpseTeam = corpse.GetComponent<TeamMember>();
                if (myTeam != null && corpseTeam != null) corpseTeam.StartTeam = myTeam.Team;

                var corpseRBs = corpse.GetComponentsInChildren<Rigidbody>();
                for (int i = 0; i < corpseRBs.Length; i++)
                {
                    corpseRBs[i].AddForceAtPosition(impactForce, impactPoint);
                }

                Destroy(gameObject);
            }
            else if (rb != null)
            {
                rb.AddForceAtPosition(impactForce, impactPoint);
            }
        }

        public void Damage(float amount)
        {
            Impact(amount, Vector3.zero, Vector3.zero);
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            HP = MaxHP;
        }
    }
}