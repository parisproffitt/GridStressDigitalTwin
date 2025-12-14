from __future__ import annotations
from dataclasses import dataclass
from typing import List, Dict
import math

from .config import GridConfig, get_rng

@dataclass
class Node:
    id: str
    x: float
    y: float
    rating_kva: int
    neighbors: List[str]

    def to_dict(self) -> Dict:
        return {
            "id": self.id,
            "x": self.x,
            "y": self.y,
            "rating_kva": self.rating_kva,
            "neighbors": self.neighbors,
        }

def _make_ids(n: int) -> List[str]:
    width = len(str(n))
    return [f"T{str(i+1).zfill(width)}" for i in range(n)]

def _grid_dimensions(n: int) -> tuple[int, int]:
    cols = math.ceil(math.sqrt(n))
    rows = math.ceil(n / cols)
    return cols, rows

def _adjacent_neighbors(index: int, cols: int, rows: int) -> List[int]:
    r = index // cols
    c = index % cols
    out = []
    if r > 0: out.append((r - 1) * cols + c)
    if r < rows - 1: out.append((r + 1) * cols + c)
    if c > 0: out.append(r * cols + (c - 1))
    if c < cols - 1: out.append(r * cols + (c + 1))
    return out

def generate_nodes(cfg: GridConfig) -> List[Node]:
    rng = get_rng(cfg.seed)
    ids = _make_ids(cfg.num_nodes)
    cols, rows = _grid_dimensions(cfg.num_nodes)

    nodes: List[Node] = []
    for i, node_id in enumerate(ids):
        r = i // cols
        c = i % cols
        x = c * cfg.x_spacing
        y = -r * cfg.y_spacing
        rating = rng.choice(cfg.rating_kva_options)
        nodes.append(Node(id=node_id, x=x, y=y, rating_kva=rating, neighbors=[]))

    for i, node in enumerate(nodes):
        adj = [j for j in _adjacent_neighbors(i, cols, rows) if j < cfg.num_nodes]
        rng.shuffle(adj)

        k = rng.randint(cfg.min_neighbors, cfg.max_neighbors)
        chosen = adj[: min(k, len(adj))]

        while len(chosen) < cfg.min_neighbors:
            j = rng.randrange(cfg.num_nodes)
            if j != i and j not in chosen:
                chosen.append(j)

        node.neighbors = [nodes[j].id for j in chosen]

    return nodes