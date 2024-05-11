using PlayerSystems.Collectables;
using PlayerSystems.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems
{
    public class CollectableSystem : System<CollectableSystem>
    {
        private void Start()
        {
            inputManager.OnCollectTriggered += PerformCollect;
        }
        private void OnDestroy()
        {
            inputManager.OnCollectTriggered -= PerformCollect;
        }
        public override void OnStatusChange(bool currentStatus)
        {
            if (currentStatus)
            {
                EnvironmentChecker.Instance.OnCollectablesChanged += CollectCollectables;
            }
            else
            {
                collectables.Clear();
                EnvironmentChecker.Instance.OnCollectablesChanged -= CollectCollectables;
            }
        }

        private void CollectCollectables(List<ICollectables> list)
        {
            collectables = list;
        }

        public void PerformCollect()
        {
            if (Status)
            {
                var item = GetClosestItem<ICollectables>(collectables);
                item?.Collect();
                EnvironmentChecker.Instance.RemoveCollectableFromList(item);

                //Add Item to Inventory Code Logics

            }
        }

        [SerializeField] private List<ICollectables> collectables = new List<ICollectables>();
        
    }
}