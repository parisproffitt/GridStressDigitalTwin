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

    [Header("Layout (Visual Polish)")]
    [Tooltip("How wide the whole neighborhood should be (Unity units)")]
    public float worldWidth = 8.5f;

    [Tooltip("How tall the whole neighborhood should be (Unity units)")]
    public float worldHeight = 6.0f;

    [Tooltip("Small organic offset per node (Unity units). Keep subtle (0.2–0.6).")]
    public float jitter = 0.35f;

    [Tooltip("Keep nodes away from the edges a bit (Unity units).")]
    public float padding = 0.6f;

    [Tooltip("Seed so layout is stable every run (good for demos).")]
    public int layoutSeed = 2026;

    // Internal data
    private SimOutput sim;

    private readonly Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, NodeState> stateLookup = new Dictionary<string, NodeState>();

    private int currentTimestep = 0;

    // Cached bounds for coordinate normalization
    private float minX, maxX, minY, maxY;

    void Start()
    {
        LoadSimulationData();
        CacheNodeBounds();
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

    void CacheNodeBounds()
    {
        // Find min/max of JSON coordinates so we can normalize to a nicer layout
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;

        if (sim == null || sim.nodes == null || sim.nodes.Count == 0) return;

        foreach (var n in sim.nodes)
        {
            if (n.x < minX) minX = n.x;
            if (n.x > maxX) maxX = n.x;
            if (n.y < minY) minY = n.y;
            if (n.y > maxY) maxY = n.y;
        }

        // If all coords are same somehow, avoid divide-by-zero normalization
        if (Mathf.Abs(maxX - minX) < 0.0001f) { minX -= 1f; maxX += 1f; }
        if (Mathf.Abs(maxY - minY) < 0.0001f) { minY -= 1f; maxY += 1f; }
    }

    void SpawnNodes()
    {
        if (sim == null || sim.nodes == null) return;

        foreach (var n in sim.nodes)
        {
            Vector3 pos = ComputePolishedPosition(n.id, n.x, n.y);

            GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity);
            go.name = n.id;

            // Ensure NodeView exists for clicking
            NodeView view = go.GetComponent<NodeView>();
            if (view == null) view = go.AddComponent<NodeView>();
            view.NodeId = n.id;

            nodeObjects[n.id] = go;
        }
    }

    Vector3 ComputePolishedPosition(string nodeId, float x, float y)
    {
        // Normalize JSON x/y into [0,1]
        float nx = Mathf.InverseLerp(minX, maxX, x);
        float ny = Mathf.InverseLerp(minY, maxY, y);

        // Map to centered world space, using XZ plane
        float usableW = Mathf.Max(0.1f, worldWidth - padding);
        float usableH = Mathf.Max(0.1f, worldHeight - padding);

        float px = (nx - 0.5f) * usableW;
        float pz = (ny - 0.5f) * usableH;

        // Deterministic jitter per node (stable every run, no Random.Range dependency)
        float jx = (HashTo01(nodeId, layoutSeed) - 0.5f) * 2f * jitter;
        float jz = (HashTo01(nodeId, layoutSeed ^ 0x9e3779b9) - 0.5f) * 2f * jitter;

        // Optional: tiny neighborhood “skew” so it’s less perfectly aligned
        // (Very subtle; keeps it looking like streets/blocks.)
        float skew = 0.15f;
        px += (ny - 0.5f) * skew;
        pz += (nx - 0.5f) * skew;

        return new Vector3(px + jx, 0f, pz + jz);
    }

    // Stable hash -> [0,1)
    float HashTo01(string s, int seed)
    {
        unchecked
        {
            int h = seed;
            for (int i = 0; i < s.Length; i++)
                h = (h * 31) + s[i];

            // Mix bits
            uint x = (uint)h;
            x ^= x >> 17;
            x *= 0xed5ad4bb;
            x ^= x >> 11;
            x *= 0xac4c1b51;
            x ^= x >> 15;

            return (x & 0xFFFFFF) / (float)0x1000000; // 24-bit fraction
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
