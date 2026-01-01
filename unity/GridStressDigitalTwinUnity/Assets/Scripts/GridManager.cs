using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    public GameObject nodePrefab;   // Drag your Node prefab here
    public Text infoText;           // Drag your InfoText (Legacy Text) here

    [Header("Playback")]
    public float timestepInterval = 2f; // seconds between heat stages

    // Internal data
    private SimOutput sim;

    private readonly Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, NodeState> stateLookup = new Dictionary<string, NodeState>();

    private int currentTimestep = 0;

    void Start()
    {
        LoadSimulationData();
        SpawnNodes();

        // Start heatwave playback
        InvokeRepeating(nameof(AdvanceTimestep), 0f, timestepInterval);

        if (infoText != null)
            infoText.text = "Click a node to view details.";
    }

    void LoadSimulationData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "sim_output.json");

        if (!File.Exists(path))
        {
            Debug.LogError("sim_output.json not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        sim = JsonUtility.FromJson<SimOutput>(json);

        if (sim == null || sim.nodes == null || sim.timesteps == null)
        {
            Debug.LogError("Failed to parse sim_output.json (sim/nodes/timesteps null)");
            return;
        }

        Debug.Log($"Loaded nodes={sim.nodes.Count}, timesteps={sim.timesteps.Count}");
    }

    void SpawnNodes()
    {
        if (sim == null || sim.nodes == null) return;

        foreach (var n in sim.nodes)
        {
            // JSON x/y -> Unity x/z (flat plane)
            Vector3 pos = new Vector3(n.x, 0f, n.y);

            GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity);
            go.name = n.id;

            // Ensure NodeView exists for clicking
            NodeView view = go.GetComponent<NodeView>();
            if (view == null) view = go.AddComponent<NodeView>();
            view.NodeId = n.id;

            nodeObjects[n.id] = go;
        }
    }

    void AdvanceTimestep()
    {
        if (sim == null || sim.timesteps == null || sim.timesteps.Count == 0)
            return;

        if (currentTimestep >= sim.timesteps.Count)
        {
            CancelInvoke(nameof(AdvanceTimestep));
            Debug.Log("Finished all timesteps.");
            return;
        }

        ApplyTimestep(currentTimestep);
        currentTimestep++;
    }

    void ApplyTimestep(int idx)
    {
        if (sim == null || sim.timesteps == null || sim.timesteps.Count == 0)
            return;

        idx = Mathf.Clamp(idx, 0, sim.timesteps.Count - 1);
        var ts = sim.timesteps[idx];

        // Build lookup for node states at this timestep
        stateLookup.Clear();
        foreach (var s in ts.states)
            stateLookup[s.id] = s;

        // Update node colors
        foreach (var kvp in nodeObjects)
        {
            string id = kvp.Key;
            GameObject go = kvp.Value;

            if (!stateLookup.ContainsKey(id)) continue;

            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = ColorForRisk(stateLookup[id].risk_level);
        }

        Debug.Log($"Applied timestep {ts.t} @ {ts.ambient_temp_f}F");

        // Update UI header text
        if (infoText != null)
            infoText.text = $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}F\nClick a node to view details.";
    }

    // Called by NodeView when clicking a node
    public void SelectNode(string id)
    {
        if (infoText == null || sim == null || sim.timesteps == null || sim.timesteps.Count == 0)
            return;

        int idx = Mathf.Clamp(currentTimestep - 1, 0, sim.timesteps.Count - 1);
        var ts = sim.timesteps[idx];

        if (!stateLookup.ContainsKey(id))
            return;

        var st = stateLookup[id];

        infoText.text =
            $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}F\n\n" +
            $"{id}\n" +
            $"Load: {(int)(st.load_pct * 100)}%   Risk: {st.risk_level} ({st.risk_score:0.00})\n\n" +
            st.explanation;
    }

    Color ColorForRisk(string level)
    {
        // Match your colorway nicely
        if (level == "R") return new Color(0.85f, 0.25f, 0.25f); // red
        if (level == "Y") return new Color(0.95f, 0.80f, 0.20f); // yellow
        return new Color(0.20f, 0.75f, 0.65f);                  // teal-green
    }
}
