using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
public class PlayerIk : MonoBehaviour
{
    public static PlayerIk Instance { get; private set; }

    private RigBuilder rigBuilder; // 0th Layer Is right Hand Ik
    [SerializeField] private Torch torch;

    private IHoldable currentHoldingObject;
    private void Awake()
    {
        Instance = this;
        rigBuilder = GetComponent<RigBuilder>();
        DisableIks();
    }

    private void UpdateRigVisual()
    {
        if (currentHoldingObject != null)
        {
            switch (currentHoldingObject)
            {
                case Lamp:
                    rigBuilder.layers[0].rig.weight = 1;
                    rigBuilder.layers[1].rig.weight = 0;
                    break;
                case Torch:
                    rigBuilder.layers[0].rig.weight = 0;
                    rigBuilder.layers[1].rig.weight = 1;
                    break;
            }
        }
        else
        {
            DisableIks();
        }
    }

    public void SetHoldingObject(IHoldable holdable)
    {
        currentHoldingObject = holdable;

        //Debug.Log($"Current Held Object {currentHoldingObject}");

        UpdateRigVisual();
    }

    private void DisableIks()
    {
        foreach (RigLayer rig in rigBuilder.layers)
        {
            rig.rig.weight = 0;
        }
    }
}
