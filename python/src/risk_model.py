from __future__ import annotations
from typing import Dict, List, Any
from .generate_grid import Node


def clamp(x: float, lo: float = 0.0, hi: float = 1.0) -> float:
    return max(lo, min(hi, x))


def _piecewise_linear(x: float, x0: float, x1: float) -> float:
    """
    Map x into [0,1] linearly between x0 and x1, clamped outside.
    """
    if x <= x0:
        return 0.0
    if x >= x1:
        return 1.0
    return (x - x0) / (x1 - x0)


def compute_risk_score(load_pct: float, ambient_temp_f: float, neighbor_stress_frac: float) -> float:
    """
    Planning-level, interpretable risk model.
    Weighted combination of three components:
      - Load stress (primary)
      - Temperature stress
      - Neighbor stress (amplifier)
    """

    # 1) Load component: starts rising meaningfully above ~0.75, near max by ~0.95
    load_component = _piecewise_linear(load_pct, 0.75, 0.95)

    # 2) Temperature component: stress begins around 90F, near max by 100F
    temp_component = _piecewise_linear(ambient_temp_f, 90.0, 100.0)

    # 3) Neighbor component: fraction of neighbors above 0.85 load (0..1)
    neighbor_component = clamp(neighbor_stress_frac)

    # Weights (sum to 1.0) — explainable and easy to justify
    risk = (
        0.50 * load_component +
        0.30 * temp_component +
        0.20 * neighbor_component
    )
    return clamp(risk)


def risk_level_from_score(score: float) -> str:
    """
    Map risk_score to a simple discrete state:
      G: low
      Y: elevated
      R: high/critical
    """
    if score < 0.40:
        return "G"
    if score < 0.70:
        return "Y"
    return "R"


def apply_risk_model(nodes: List[Node], timesteps: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """
    Mutates timestep states in-place by filling:
      - risk_score
      - risk_level
    Returns timesteps for convenience.
    """
    # Quick lookup: node_id -> neighbors
    neighbor_map: Dict[str, List[str]] = {n.id: n.neighbors for n in nodes}

    for ts in timesteps:
        ambient = float(ts["ambient_temp_f"])
        states: List[Dict[str, Any]] = ts["states"]

        # Build load lookup for this timestep
        load_by_id: Dict[str, float] = {s["id"]: float(s["load_pct"]) for s in states}

        for s in states:
            node_id = s["id"]
            load = float(s["load_pct"])

            neighbors = neighbor_map.get(node_id, [])
            if neighbors:
                stressed = sum(1 for nid in neighbors if load_by_id.get(nid, 0.0) > 0.85)
                neighbor_stress_frac = stressed / len(neighbors)
            else:
                neighbor_stress_frac = 0.0

            score = compute_risk_score(load, ambient, neighbor_stress_frac)
            level = risk_level_from_score(score)

            s["risk_score"] = round(score, 4)
            s["risk_level"] = level

    return timesteps
