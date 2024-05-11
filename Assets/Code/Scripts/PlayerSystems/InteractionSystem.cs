using PlayerSystems.Collectables;
using PlayerSystems.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    public class InteractionSystem : System<InteractionSystem>
    {
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
                var item = GetClosestItem<IInteractables>(interactables);
                item?.Interact();

            }
        }

        private List<IInteractables> interactables;
    }
}