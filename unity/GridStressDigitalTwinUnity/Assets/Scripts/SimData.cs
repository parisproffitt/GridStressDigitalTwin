using System;
using System.Collections.Generic;

[Serializable]
public class SimOutput {
    public List<NodeMeta> nodes;
    public List<Timestep> timesteps;
}

[Serializable]
public class NodeMeta {
    public string id;
    public float x;
    public float y;
    public int rating_kva;
    public List<string> neighbors;
}

[Serializable]
public class Timestep {
    public int t;
    public float ambient_temp_f;
    public List<NodeState> states;
}

[Serializable]
public class NodeState {
    public string id;
    public float load_pct;
    public float risk_score;
    public string risk_level;   // "G", "Y", "R"
    public string explanation;
}

