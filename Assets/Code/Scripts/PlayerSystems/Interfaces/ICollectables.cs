using PlayerSystems.Interactables;
using UnityEngine;

namespace PlayerSystems.Collectables
{
    public interface ICollectables : IGetPosition
    {
        public void Collect();
    }
}