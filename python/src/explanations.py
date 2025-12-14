from __future__ import annotations
from typing import Dict, List, Any

from .generate_grid import Node


def _pct(x: float) -> str:
    return f"{round(x * 100)}%"


def apply_explanations(nodes: List[Node], timesteps: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """
    Fills the 'explanation' field for each node state using simple,
    human-interpretable rules consistent with the design document.
    """
    neighbor_map: Dict[str, List[str]] = {n.id: n.neighbors for n in nodes}

    for ts in timesteps:
        ambient = float(ts["ambient_temp_f"])
        states: List[Dict[str, Any]] = ts["states"]

        # Build lookup to assess neighbor stress at this timestep
        load_by_id: Dict[str, float] = {s["id"]: float(s["load_pct"]) for s in states}

        for s in states:
            node_id = s["id"]
            load = float(s["load_pct"])
            risk_level = s.get("risk_level", "G")

            neighbors = neighbor_map.get(node_id, [])
            stressed_neighbors = 0
            if neighbors:
                stressed_neighbors = sum(1 for nid in neighbors if load_by_id.get(nid, 0.0) > 0.85)

            reasons = []

            # Temperature reasons (aligned with your 90–100 stress window)
            if ambient >= 100:
                reasons.append("extreme ambient heat")
            elif ambient >= 95:
                reasons.append("heatwave-level temperature")
            elif ambient >= 90:
                reasons.append("elevated summer temperature")

            # Load reasons (aligned with planning thresholds)
            if load >= 0.95:
                reasons.append(f"near-capacity load ({_pct(load)})")
            elif load >= 0.90:
                reasons.append(f"high load ({_pct(load)})")
            elif load >= 0.85:
                reasons.append(f"elevated load ({_pct(load)})")

            # Neighbor reasons (amplifier)
            if stressed_neighbors >= 2:
                reasons.append("multiple stressed neighboring transformers")
            elif stressed_neighbors == 1:
                reasons.append("a stressed neighboring transformer")

            # Build final explanation with a consistent tone
            if not reasons:
                explanation = "Low risk under moderate load and temperature."
            else:
                # Keep it short and readable
                base = "Risk driven by " + ", ".join(reasons) + "."
                if risk_level == "R":
                    explanation = base + " Prioritize review."
                elif risk_level == "Y":
                    explanation = base + " Monitor conditions."
                else:
                    explanation = base

            s["explanation"] = explanation

    return timesteps
