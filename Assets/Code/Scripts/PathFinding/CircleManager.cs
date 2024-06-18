using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircleManager : MonoBehaviour {

    private List<PathNode> radiusPathNodes;

    private void Awake() {
        radiusPathNodes = GetComponentsInChildren<PathNode>().ToList();
    }

    public PathNode GetFarthestPathNodeNotInspected(Vector3 fromPosition) {
        return radiusPathNodes
            .Where(pathNode => !pathNode.InspectedArea)
            .OrderByDescending(pathNode => Vector3.Distance(pathNode.transform.position, fromPosition))
            .FirstOrDefault();
    }

    public List<PathNode> GetAllPathNodes() => radiusPathNodes;

}
