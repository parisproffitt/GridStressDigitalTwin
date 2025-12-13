# Quantum-Inspired Grid Stress Digital Twin for Extreme Heat Events
A QUBO-Inspired Framework Integrated with an Interactive Unity Visualization for Grid Resilience Analysis_

## Overview
This project is a small-scope digital twin–style prototype that simulates heatwave-driven stress on a synthetic set of transformer nodes and visualizes risk over time in Unity. It is designed to be interpretable, modular, and extensible for future lab use cases.

## Problem
Extreme heat events can push distribution transformers closer to thermal limits. Traditional monitoring often relies on threshold alarms and provides limited forecasting or intuitive visualization of evolving stress patterns.

## What this prototype includes
- **Synthetic grid generator** (10–20 transformer nodes, coordinates, ratings, baseline loads)
- **Heatwave simulation** (ambient temperature increases over time; loads scale accordingly)
- **Rule-based risk model** (risk_score 0–1 and risk_level G/Y/R using interpretable heuristics)
- **Explanation engine** (plain-language reasons for risk level)
- **Unity visualization** (color-coded nodes, click-to-explain, time slider)

> Note: “Quantum-inspired” refers to QUBO-inspired framing and compatibility with future quantum optimization integration. No quantum computations are performed in this prototype.

## How it can be useful to the lab
- Provides a reusable framework for “digital twin–style” grid stress demos
- Demonstrates a clean data pipeline from simulation output → Unity visualization
- Offers an extensible structure for future integration with real datasets, ML models, or quantum solvers

## Quick Start (Python)
1. Create a virtual environment and install dependencies:
   - `pip install -r requirements.txt`
2. Run the pipeline:
   - `python -m src.pipeline`
3. Outputs are written to:
   - `python/data/generated/`

## Quick Start (Unity)
1. Open the Unity project in `/unity/`
2. Place the exported JSON/CSV into the Unity data folder
3. Press Play to visualize risk states and click nodes for explanations

## Data Format
See:
- `docs/DATA_SCHEMA.md`

## Risk Model Notes
See:
- `docs/RISK_MODEL_JUSTIFICATION.md`
