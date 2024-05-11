using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    public class PlayerSystemsManager : MonoBehaviour
    {
        public void CheckInteractables(bool status)
        {
            gameObject.AddComponent<InteractionSystem>();
            var interactionSystem = InteractionSystem.Instance;
            interactionSystem.Status = status;
            interactionSystem.inputManager = inputManager;
            interactionSystem.inputManager = inputManager;
        }

        public void CheckCollectables(bool status)
        {
            gameObject.AddComponent<CollectableSystem>();

            var collectionSystem = CollectableSystem.Instance;
            collectionSystem.Status = status;
            collectionSystem.inputManager = inputManager;
        }

        private void Start()
        {
            CheckCollectables(toCheckCollectables);
            CheckInteractables(toCheckInteractables);
        }
        public bool toCheckInteractables = true;
        private bool toCheckCollectables = true;
        public InputManager inputManager;
    }
}