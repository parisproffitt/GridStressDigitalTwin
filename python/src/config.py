from dataclasses import dataclass
import random

@dataclass(frozen=True)
class GridConfig:
    num_nodes: int = 12  # set anywhere 10–20
    x_spacing: float = 2.0
    y_spacing: float = 2.0
    rating_kva_options: tuple[int, ...] = (250, 500, 750, 1000)
    min_neighbors: int = 1
    max_neighbors: int = 3
    seed: int = 42

def get_rng(seed: int) -> random.Random:
    return random.Random(seed)