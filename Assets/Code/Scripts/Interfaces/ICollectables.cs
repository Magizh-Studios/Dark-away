using PlayerSystems.Interactables;
using UnityEngine;

namespace PlayerSystems.Collectables
{
    public interface ICollectables : IGetPosition
    {
        public bool canCollect { get; set; }
        public void Collect();
    }
}