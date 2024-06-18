using UnityEngine;

public class PathNode : MonoBehaviour
{
   public NodeType NodeType;
   public bool InspectedArea = false;
}
public enum NodeType {
    OutSide,
    Inside,
    RoofTop
}