using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    public class PlayerSystemsManager : MonoBehaviour
    {
        public bool toCheckInteractables = true;
        public bool toCheckCollectables = true;
        public InputManager inputManager;

        private void Awake()
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
        public void CheckInteractables(bool status)
        {
            if (!gameObject.TryGetComponent(out InteractionSystem _))
            {
                gameObject.AddComponent<InteractionSystem>();
            }

            InteractionSystem.Instance.Status = status;
            InteractionSystem.Instance.inputManager = inputManager;
        }

        [ContextMenu("Toggle Collectables")]
        public void ToggleCollect()
        {
            toCheckCollectables = !toCheckCollectables;
            CheckCollectables(toCheckCollectables);
        }

        public void CheckCollectables(bool status)
        {
            if (!gameObject.TryGetComponent(out CollectableSystem _))
            {
                gameObject.AddComponent<CollectableSystem>();
            }

            CollectableSystem.Instance.Status = status;
            CollectableSystem.Instance.inputManager = inputManager;

        }


    }
}