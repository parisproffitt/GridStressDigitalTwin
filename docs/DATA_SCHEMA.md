# Data Schema (Unity-Ready)

This project exports a single JSON file that Unity can load.

## File: python/data/generated/sim_output.json

### Top-Level Keys
- `nodes`: static node metadata
- `timesteps`: time-series states for each node

### nodes[]
Each node:
- `id` (string): e.g., "T01"
- `x` (number): x position in Unity
- `y` (number): y position in Unity
- `rating_kva` (int)
- `neighbors` (string[]): list of node IDs

### timesteps[]
Each timestep:
- `t` (int): timestep index
- `ambient_temp_f` (number)
- `states` (array)

### states[]
Each node state at time `t`:
- `id` (string)
- `load_pct` (number, 0–1)
- `risk_score` (number, 0–1)
- `risk_level` (string): "G", "Y", or "R"
- `explanation` (string)