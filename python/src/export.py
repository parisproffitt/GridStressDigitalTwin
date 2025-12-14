from __future__ import annotations
import json
from pathlib import Path
from typing import Any, Dict, List

from .generate_grid import Node

def export_sim_output(nodes: List[Node], timesteps: List[Dict[str, Any]], out_path: Path) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    payload = {
        "nodes": [n.to_dict() for n in nodes],
        "timesteps": timesteps,
    }
    out_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")