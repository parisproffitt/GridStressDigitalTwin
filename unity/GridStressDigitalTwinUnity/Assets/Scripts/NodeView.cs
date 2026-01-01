using UnityEngine;

public class NodeView : MonoBehaviour
{
    public string NodeId;

    private GridManager gridManager;

    void Start()
    {
        // Find the GridManager in the scene once
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
            Debug.LogError("GridManager not found in scene. Make sure you have a GridManager object with the GridManager script attached.");
    }

    void OnMouseDown()
    {
        if (gridManager != null)
            gridManager.SelectNode(NodeId);
    }
}
