using PlayerSystems.Collectables;
using PlayerSystems.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerSystems
{
    public class InteractionSystem : System<InteractionSystem>
    {
        [Range(-1, 1)]
        [SerializeField] private float dotThreshold = 0.3f;

        private List<IInteractables> interactables;

        private void Start()
        {
            inputManager.OnInteractTriggered += PerformInteract;
        }

        private void OnDestroy()
        {
            inputManager.OnInteractTriggered -= PerformInteract;
        }

        public override void OnStatusChange(bool currentStatus)
        {
            if (currentStatus)
            {
                interactables = new List<IInteractables>();
                EnvironmentChecker.Instance.OnInteractablesChanged += CollectInteractables;
            }
            else
            {
                interactables.Clear();
                EnvironmentChecker.Instance.OnInteractablesChanged -= CollectInteractables;
            }
        }

        private void CollectInteractables(List<IInteractables> list)
        {
            interactables = list;
        }

        public void PerformInteract()
        {
            if (Status)
            {
                //var item = GetClosestItem(interactables);

                //if (item == null) return;

                //Vector3 itemPos = item.GetPosition();

                //Vector3 directionToTarget = (itemPos - transform.position).normalized;

                //float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

                //Debug.Log(dotProduct);

                //if (dotProduct > dotThreshold)
                //{
                //    item.Interact();
                //}

                RaycastHit[] raycastHits = new RaycastHit[5];

                float interactRadius = EnvironmentChecker.Instance.GetInteractRadius();
                int interactLayers = EnvironmentChecker.Instance.GetInteractLayers();

                int colliderCounts = Physics.RaycastNonAlloc(transform.position + Vector3.up * 1.2f, transform.forward, raycastHits, interactRadius, interactLayers);
                Debug.DrawRay(transform.position + Vector3.up * 1.2f, transform.forward /** interactRadius*/,Color.green);

                Debug.Log(colliderCounts);

                if(colliderCounts > 0)
                {
                    foreach (RaycastHit hit in raycastHits)
                    {
                        if(hit.collider != null)
                        {
                            Debug.Log($"Collider {hit.collider.gameObject.name}");

                            if(hit.collider.gameObject.TryGetComponent(out IInteractables interactables))
                            {
                                interactables.Interact();
                            }
                            else
                            {
                                Debug.LogWarning("Iinteractabele Not exist");
                            }
                        }
                       
                    }
                }
            }
        }

    }
}