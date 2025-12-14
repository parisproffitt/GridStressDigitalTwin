from __future__ import annotations
from pathlib import Path

from .config import GridConfig
from .generate_grid import generate_nodes
from .simulate_heatwave import simulate_timesteps
from .risk_model import apply_risk_model
from .export import export_sim_output


def main() -> None:
    # Choose any number 10–20. Using 20 for demo.
    cfg = GridConfig(num_nodes=20)

    # 1) Generate the static transformer grid (nodes + neighbors + coordinates + ratings)
    nodes = generate_nodes(cfg)

    # 2) Simulate a heatwave progression and compute per-node load_pct across timesteps
    timesteps = simulate_timesteps(cfg, nodes)

    # 3) Apply rule-based, explainable risk scoring (risk_score + risk_level)
    timesteps = apply_risk_model(nodes, timesteps)

    # 4) Export Unity-ready JSON
    out_path = Path(__file__).resolve().parents[1] / "data" / "generated" / "sim_output.json"
    export_sim_output(nodes, timesteps, out_path)

    print(f"[OK] Generated {len(nodes)} nodes")
    print(f"[OK] Generated {len(timesteps)} timesteps")
    print(f"[OK] Wrote: {out_path}")


if __name__ == "__main__":
    main()
