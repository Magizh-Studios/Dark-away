using UnityEngine;
namespace PlayerSystems.Interactables
{
    public interface IInteractables : IGetPosition
    {
        public void Interact();
    }
}