from __future__ import annotations
from typing import Dict, List, Any

from .config import GridConfig, get_rng
from .generate_grid import Node


def clamp(x: float, lo: float = 0.0, hi: float = 1.0) -> float:
    return max(lo, min(hi, x))


def temperature_profile_f() -> List[float]:
    """
    Realistic ramp (Florida summer) showing:
    normal -> elevated stress -> heatwave -> peak heat.
    """
    return [88, 90, 92, 95, 97, 99, 100]


def temp_norm(ambient_f: float, t0: float = 85.0, t1: float = 100.0) -> float:
    """
    Normalize ambient temp to [0,1]:
    85F -> 0, 100F -> 1
    """
    return clamp((ambient_f - t0) / (t1 - t0))


def simulate_timesteps(cfg: GridConfig, nodes: List[Node]) -> List[Dict[str, Any]]:
    rng = get_rng(cfg.seed)

    # City-wide variation: each node gets baseline demand + a local demand factor
    baseline_load: Dict[str, float] = {
        n.id: rng.uniform(cfg.baseline_load_min, cfg.baseline_load_max) for n in nodes
    }
    demand_factor: Dict[str, float] = {
        # Some neighborhoods consistently pull higher/lower load
        n.id: rng.uniform(0.90, 1.15) for n in nodes
    }

    temps = temperature_profile_f()
    timesteps: List[Dict[str, Any]] = []

    for t, ambient in enumerate(temps):
        tn = temp_norm(ambient)

        states = []
        for n in nodes:
            base = baseline_load[n.id]
            factor = demand_factor[n.id]

            # Heat-driven uplift: up to +25% as we approach 100F
            uplift = tn * 0.25

            # Small variability so curves aren’t perfectly smooth (still deterministic via seed)
            noise = rng.uniform(-0.02, 0.02)

            load_pct = clamp((base + uplift + noise) * factor, 0.0, 1.0)

            states.append({
                "id": n.id,
                "load_pct": round(load_pct, 4),
                # placeholders (we will fill these next step)
                "risk_score": 0.0,
                "risk_level": "G",
                "explanation": "Placeholder (risk/explanation added next step)."
            })

        timesteps.append({
            "t": t,
            "ambient_temp_f": ambient,
            "states": states
        })

    return timesteps
