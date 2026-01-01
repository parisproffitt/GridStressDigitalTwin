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
    [Tooltip("Overall horizontal spread of the neighborhood")]
    public float worldWidth = 8.5f;

    [Tooltip("Overall vertical spread of the neighborhood")]
    public float worldHeight = 6.0f;

    [Tooltip("Small organic offset per node (keep subtle)")]
    public float jitter = 0.35f;

    [Tooltip("Padding from edges to keep nodes on screen")]
    public float padding = 0.6f;

    [Tooltip("Seed so layout is stable every run")]
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
            Debug.LogError("Failed to parse sim_output.json");
            return;
        }

        Debug.Log($"Loaded nodes={sim.nodes.Count}, timesteps={sim.timesteps.Count}");
    }

    void CacheNodeBounds()
    {
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;

        foreach (var n in sim.nodes)
        {
            minX = Mathf.Min(minX, n.x);
            maxX = Mathf.Max(maxX, n.x);
            minY = Mathf.Min(minY, n.y);
            maxY = Mathf.Max(maxY, n.y);
        }

        if (Mathf.Abs(maxX - minX) < 0.001f) { minX -= 1f; maxX += 1f; }
        if (Mathf.Abs(maxY - minY) < 0.001f) { minY -= 1f; maxY += 1f; }
    }

    void SpawnNodes()
    {
        foreach (var n in sim.nodes)
        {
            Vector3 pos = ComputePolishedPosition(n.id, n.x, n.y);

            GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity);
            go.name = n.id;

            NodeView view = go.GetComponent<NodeView>();
            if (view == null) view = go.AddComponent<NodeView>();
            view.NodeId = n.id;

            nodeObjects[n.id] = go;
        }
    }

    Vector3 ComputePolishedPosition(string nodeId, float x, float y)
    {
        float nx = Mathf.InverseLerp(minX, maxX, x);
        float ny = Mathf.InverseLerp(minY, maxY, y);

        float usableW = Mathf.Max(0.1f, worldWidth - padding);
        float usableH = Mathf.Max(0.1f, worldHeight - padding);

        float px = (nx - 0.5f) * usableW;
        float pz = (ny - 0.5f) * usableH;

        float jx = (Hash01(nodeId, layoutSeed) - 0.5f) * 2f * jitter;
        float jz = (Hash01(nodeId, layoutSeed + 7919) - 0.5f) * 2f * jitter;

        // subtle skew so it feels like streets/neighborhoods
        float skew = 0.15f;
        px += (ny - 0.5f) * skew;
        pz += (nx - 0.5f) * skew;

        return new Vector3(px + jx, 0f, pz + jz);
    }

    float Hash01(string s, int seed)
    {
        unchecked
        {
            int h = seed;
            for (int i = 0; i < s.Length; i++)
                h = (h * 31) + s[i];

            uint x = (uint)h;
            x ^= x >> 17;
            x *= 0xed5ad4bb;
            x ^= x >> 11;
            x *= 0xac4c1b51;
            x ^= x >> 15;

            return (x & 0xFFFFFF) / (float)0x1000000;
        }
    }

    void AdvanceTimestep()
    {
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
        idx = Mathf.Clamp(idx, 0, sim.timesteps.Count - 1);
        var ts = sim.timesteps[idx];

        stateLookup.Clear();
        foreach (var s in ts.states)
            stateLookup[s.id] = s;

        foreach (var kvp in nodeObjects)
        {
            if (!stateLookup.ContainsKey(kvp.Key)) continue;

            Renderer rend = kvp.Value.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = ColorForRisk(stateLookup[kvp.Key].risk_level);
        }

        if (infoText != null)
            infoText.text = $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}F\nClick a node to view details.";
    }

    public void SelectNode(string id)
    {
        int idx = Mathf.Clamp(currentTimestep - 1, 0, sim.timesteps.Count - 1);
        var ts = sim.timesteps[idx];

        if (!stateLookup.ContainsKey(id)) return;
        var st = stateLookup[id];

        infoText.text =
            $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}F\n\n" +
            $"{id}\n" +
            $"Load: {(int)(st.load_pct * 100)}%   Risk: {st.risk_level} ({st.risk_score:0.00})\n\n" +
            st.explanation;
    }

    Color ColorForRisk(string level)
    {
        if (level == "R") return new Color(0.85f, 0.25f, 0.25f); // red
        if (level == "Y") return new Color(0.95f, 0.80f, 0.20f); // yellow
        return new Color(0.20f, 0.75f, 0.65f);                  // teal-green
    }
}

