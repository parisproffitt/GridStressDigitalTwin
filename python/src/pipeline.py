from __future__ import annotations
from pathlib import Path

from .config import GridConfig
from .generate_grid import generate_nodes
from .export import export_sim_output

def main() -> None:
    cfg = GridConfig(num_nodes=20)  # change to any number 10–20
    nodes = generate_nodes(cfg)

    # Dec 13: nodes only; timesteps added Dec 14–18
    timesteps = []

    out_path = Path(__file__).resolve().parents[1] / "data" / "generated" / "sim_output.json"
    export_sim_output(nodes, timesteps, out_path)

    print(f"[OK] Generated {len(nodes)} nodes")
    print(f"[OK] Wrote: {out_path}")

if __name__ == "__main__":
    main()