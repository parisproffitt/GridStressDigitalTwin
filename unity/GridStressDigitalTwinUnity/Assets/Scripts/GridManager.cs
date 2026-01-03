using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GridManager : MonoBehaviour
{
    [Header("References")]
    public GameObject nodePrefab;   // Sphere prefab
    public Text infoText;           // Legacy UI Text
    public Camera clickCamera;      // Drag your top-down camera here

    [Header("Playback")]
    public float timestepInterval = 2f;

    [Header("Layout (Neighborhood Feel)")]
    [Tooltip("Overall horizontal spread of the neighborhood")]
    public float worldWidth = 8.5f;

    [Tooltip("Overall vertical spread of the neighborhood")]
    public float worldHeight = 6.0f;

    [Tooltip("Small organic offset per node (stable each run)")]
    public float jitter = 0.35f;

    [Tooltip("Padding margin so nodes don't hug the edges")]
    public float padding = 0.6f;

    [Tooltip("Adds subtle diagonal distortion (street-like)")]
    public float skew = 0.15f;

    [Tooltip("Seed so layout stays identical every run")]
    public int layoutSeed = 2026;

    private SimOutput sim;
    private readonly Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();

    private int currentTimestep = 0;

    // Bounds for normalization
    private float minX, maxX, minY, maxY;

    // =========================
    // UNITY LIFECYCLE
    // =========================

    void Start()
    {
        LoadSimulationData();
        if (sim == null) return;

        CacheNodeBounds();
        SpawnNodes();

        InvokeRepeating(nameof(AdvanceTimestep), 0f, timestepInterval);

        if (infoText != null)
            infoText.text = "Heat Stage: (loading...) | Click a node to view details.";
    }

    void Update()
    {
        if (WasLeftClickThisFrame())
        {
            Camera cam = clickCamera != null ? clickCamera : Camera.main;
            if (cam == null) return;

            Vector2 mousePos = GetMouseScreenPosition();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                NodeView view = hit.collider.GetComponent<NodeView>();
                if (view != null)
                    SelectNode(view.NodeId);
            }
        }
    }

    // =========================
    // INPUT (OLD + NEW SYSTEM)
    // =========================

    bool WasLeftClickThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    Vector2 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    // =========================
    // DATA LOADING
    // =========================

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
            sim = null;
        }
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

        // Avoid divide-by-zero
        if (Mathf.Abs(maxX - minX) < 0.001f) { minX -= 1f; maxX += 1f; }
        if (Mathf.Abs(maxY - minY) < 0.001f) { minY -= 1f; maxY += 1f; }
    }

    // =========================
    // NODE SPAWNING
    // =========================

    void SpawnNodes()
    {
        foreach (var n in sim.nodes)
        {
            Vector3 pos = ComputePolishedPosition(n.id, n.x, n.y);

            GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity);
            go.name = n.id;

            // Collider required for raycast
            if (go.GetComponent<Collider>() == null)
            {
                SphereCollider sc = go.AddComponent<SphereCollider>();
                sc.radius = 0.5f;
            }

            // Make sure it's raycastable
            go.layer = 0; // Default

            NodeView view = go.GetComponent<NodeView>();
            if (view == null) view = go.AddComponent<NodeView>();
            view.NodeId = n.id;

            nodeObjects[n.id] = go;
        }
    }

    Vector3 ComputePolishedPosition(string id, float x, float y)
    {
        // Normalize to 0..1
        float nx = Mathf.InverseLerp(minX, maxX, x);
        float ny = Mathf.InverseLerp(minY, maxY, y);

        // Keep everything inside a padded rectangle
        float usableW = Mathf.Max(0.1f, worldWidth - padding);
        float usableH = Mathf.Max(0.1f, worldHeight - padding);

        float px = (nx - 0.5f) * usableW;
        float pz = (ny - 0.5f) * usableH;

        // Subtle skew so it doesn't look like a perfect grid
        px += (ny - 0.5f) * skew;
        pz += (nx - 0.5f) * skew;

        // Deterministic jitter per node id (stable each run)
        float jx = (Hash01(id, layoutSeed) - 0.5f) * 2f * jitter;
        float jz = (Hash01(id, layoutSeed + 7919) - 0.5f) * 2f * jitter;

        return new Vector3(px + jx, 0f, pz + jz);
    }

    float Hash01(string s, int seed)
    {
        unchecked
        {
            int h = seed;
            for (int i = 0; i < s.Length; i++)
                h = (h * 31) + s[i];

            // Mix bits for nicer distribution
            uint x = (uint)h;
            x ^= x >> 17; x *= 0xed5ad4bb;
            x ^= x >> 11; x *= 0xac4c1b51;
            x ^= x >> 15;

            return (x & 0xFFFFFF) / (float)0x1000000;
        }
    }

    // =========================
    // TIMESTEP PLAYBACK
    // =========================

    void AdvanceTimestep()
    {
        if (currentTimestep >= sim.timesteps.Count)
        {
            CancelInvoke(nameof(AdvanceTimestep));
            return;
        }

        ApplyTimestep(currentTimestep);
        currentTimestep++;
    }

    void ApplyTimestep(int idx)
    {
        var ts = sim.timesteps[idx];

        // Build lookup for this timestep
        Dictionary<string, NodeState> lookup = new Dictionary<string, NodeState>();
        foreach (var s in ts.states)
            lookup[s.id] = s;

        // Color nodes
        foreach (var kvp in nodeObjects)
        {
            if (!lookup.ContainsKey(kvp.Key)) continue;

            Renderer r = kvp.Value.GetComponent<Renderer>();
            if (r != null)
                r.material.color = ColorForRisk(lookup[kvp.Key].risk_level);
        }

        if (infoText != null)
            infoText.text =
                $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}°F | Click a node to view details.";
    }

    // =========================
    // NODE SELECTION
    // =========================

    public void SelectNode(string id)
    {
        int idx = Mathf.Clamp(currentTimestep - 1, 0, sim.timesteps.Count - 1);
        var ts = sim.timesteps[idx];

        NodeState st = null;
        foreach (var s in ts.states)
        {
            if (s.id == id) { st = s; break; }
        }

        if (st == null || infoText == null) return;

        infoText.text =
            $"Heat Stage: t={ts.t} | Ambient: {ts.ambient_temp_f}°F\n\n" +
            $"Node {id}\n" +
            $"Load: {(int)(st.load_pct * 100)}%\n" +
            $"Risk Level: {st.risk_level} ({st.risk_score:0.00})\n\n" +
            st.explanation;
    }

    // =========================
    // COLORS
    // =========================

    Color ColorForRisk(string level)
    {
        if (level == "R") return new Color(0.88f, 0.16f, 0.20f);
        if (level == "Y") return new Color(1.00f, 0.82f, 0.10f);
        return new Color(0.20f, 0.85f, 0.30f);
    }
}


