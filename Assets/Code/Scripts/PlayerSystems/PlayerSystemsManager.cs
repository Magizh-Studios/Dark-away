using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayerSystems
{
    public class PlayerSystemsManager : MonoBehaviour
    {
        public bool toCheckInteractables = true;
        public bool toCheckCollectables = true;
        public InputManager inputManager;

        private void Start()
        {
            CheckCollectables(toCheckCollectables);
            CheckInteractables(toCheckInteractables);
        }

        [ContextMenu("Toggle Interactables")]
        public void ToggleInteract()
        {
            toCheckInteractables = !toCheckInteractables;
            CheckInteractables(toCheckInteractables);
        }

        [ContextMenu("Toggle Collectables")]
        public void ToggleCollect()
        {
            toCheckCollectables = !toCheckCollectables;
            CheckCollectables(toCheckCollectables);
        }

        public void CheckInteractables(bool status)
        {
            if (!gameObject.TryGetComponent(out InteractionSystem _))
            {
                gameObject.AddComponent<InteractionSystem>();
            }
            var interactionSystem = InteractionSystem.Instance;
            interactionSystem.Status = status;
            interactionSystem.SetInputManager(inputManager);
        }

        public void CheckCollectables(bool status)
        {
            if (!gameObject.TryGetComponent(out CollectableSystem _))
            {
                gameObject.AddComponent<CollectableSystem>();
            }

            var collectionSystem = CollectableSystem.Instance;
            collectionSystem.Status = status;
            collectionSystem.SetInputManager(inputManager);
        }
    }
}