using UnityEngine;

[DisallowMultipleComponent]
public class Player : MonoBehaviour,ILightAffectable
{
    public static Player Instance { get; private set; }

    [SerializeField] private bool isPlayerInSafeZone;
    
    [field: SerializeField] public bool IsAffectedByLight { get; set; }
    private void Awake()
    {
        Instance = this;
    }

    public void SetSafeZoneMode(bool isPlayerInSafeZone)
    {
       this.isPlayerInSafeZone = isPlayerInSafeZone;
    }

}
