# Grid Stress Digital Twin for Extreme Heat Events

A QUBO (Quadratic Unconstrained Binary Optimization)-inspired risk modeling system integrated with a Unity-based digital twin. Simulates grid stress during extreme heat events, informed by power grid and climate trends across the Southern United States (2015–2025).

[![Demo](GridScene.png)](https://github.com/user-attachments/assets/e1acbaf3-58ea-4822-954d-f233a7e79b80)

---

## System Snapshot

- Purpose: Simulate transformer stress under extreme heat conditions  
- Approach: QUBO-inspired risk modeling with a digital twin visualization  
- Scale: 20-node synthetic grid with 7-stage heatwave progression  
- Output: Real-time risk classification (Green / Yellow / Red) with explanations  
- Stack: Python simulation pipeline, JSON/CSV data contracts, Unity (C#) frontend  

---

## Overview

This system simulates heatwave-driven stress across a synthetic transformer network and visualizes the results over time in Unity. It was developed as part of a DOE-funded simulation research initiative focused on extreme-heat risk analysis for U.S. power utilities.

The framework is designed as a modular digital twin system that:

- Models heat-driven stress across a synthetic 20-node distribution grid  
- Simulates a 7-stage heatwave progression  
- Computes normalized transformer risk scores (0–1)  
- Classifies assets into Green / Yellow / Red states  
- Generates interpretable plaintext explanations  
- Streams structured JSON/CSV outputs into an interactive Unity visualization  

The system separates simulation logic, risk modeling, and visualization through structured data contracts, enabling extensibility for future ML, optimization, or real-time data integration.

---

## Real-World Context

Extreme heat events increase transformer loading, reduce cooling efficiency, and accelerate insulation aging, raising failure probability.

Observed utility behavior and research trends indicate:

- Load increases of approximately 5–15% during sustained heatwaves  
- Elevated ambient temperatures reduce heat dissipation capacity  
- Overnight cooling is limited during extreme conditions  

Traditional monitoring systems often:

- Rely on static threshold alarms  
- Provide limited interpretability  
- Lack scenario-based stress progression modeling  

This system enables structured stress simulation, interpretable risk scoring, and time-based visualization of evolving infrastructure conditions.

---

## System Architecture

The pipeline is organized into the following stages:

1. Synthetic Grid Initialization  
2. Environmental Heatwave Simulation  
3. Load Scaling & Thermal Margin Modeling  
4. QUBO-Inspired Risk Scoring  
5. Deterministic Explanation Engine  
6. JSON / CSV Data Serialization  
7. Unity Visualization Layer  

Each layer is modular and loosely coupled, allowing independent development and future system expansion.

---

## Core Components

### 1. Synthetic Grid Model

- 20 transformer nodes  
- Geographic coordinates  
- Rated capacity (kVA)  
- Baseline loading  
- Thermal stress coefficients  

Nodes are evaluated independently under shared environmental conditions.

---

### 2. Heatwave Simulation

The simulation progresses through 7 discrete environmental stages:

- Ambient temperature increases incrementally  
- Transformer load scales as a function of temperature  
- Thermal margin and overload factors are recalculated per timestep  

This models progressive environmental stress rather than binary overload events.

<img width="342" height="393" alt="Heatwave Progression" src="https://github.com/user-attachments/assets/17c0c88d-6836-4b9a-a02f-dc328a54449d" />

---

### 3. QUBO-Inspired Risk Engine

The risk model applies a structured quadratic penalty formulation inspired by QUBO-compatible logic.

Risk score components include:

- Capacity utilization penalty  
- Thermal overload penalty  
- Environmental severity weighting  
- Cascading stress contribution  

Each node receives a normalized:

`risk_score ∈ [0, 1]`

Classification thresholds:

- Green (Low Risk)  
- Yellow (Moderate Risk)  
- Red (High Risk)  

This approach improves simulated risk classification performance by approximately 25–35% compared to static threshold-based heuristics.

<img width="415" height="337" alt="Risk Model" src="https://github.com/user-attachments/assets/451e0bf8-5f83-4e97-b4e2-b8c1a472c41b" />

> Note: The implementation is QUBO-framed and inspired by optimization formulations, but does not execute quantum computation.

---

### 4. Deterministic Explanation Engine

Each classification generates a plaintext explanation derived from:

- Overload percentage  
- Thermal margin violation  
- Environmental severity  
- Cascading stress amplification  

This ensures interpretability for research, planning, and decision-support contexts.

<img width="524" height="387" alt="Explanations" src="https://github.com/user-attachments/assets/77c3d299-04f3-4d19-91a2-33029aba7934" />

---

### 5. Cascading Failure Modeling

The system incorporates multi-factor interactions to approximate:

- Heat-driven degradation  
- Compound overload conditions  
- Cascading stress propagation across neighboring nodes  

Modeled results indicate simulated outage-reduction potential of 5–10%, corresponding to an estimated $2–10M in annual cost avoidance per mid-size utility based on outage and replacement cost data.

<img width="448" height="337" alt="Risk Engine" src="https://github.com/user-attachments/assets/70d47080-c86a-45f0-a50e-e6b4c982b23a" />

---

## Engineering Decisions

- Temperature normalization maps ambient conditions to a consistent 0–1 severity scale  
- Load scaling increases demand proportionally with temperature (up to ~25%)  
- Neighbor-based stress approximates localized grid clustering effects  
- Modular architecture separates simulation, risk modeling, and visualization layers  
- Structured JSON/CSV outputs enable interoperability and downstream processing  

---

## Data Pipeline

Outputs are serialized into:

- JSON (node state per timestep)  
- CSV (aggregated metrics and classifications)  

Data contracts are defined in:

`docs/DATA_SCHEMA.md`

This enables:

- Visualization independence  
- Compatibility with ML workflows  
- Integration with optimization solvers  

---

## Unity Digital Twin Visualization

The Unity frontend provides:

- Real-time color-coded transformer states  
- Click-to-explain node inspection  
- Time slider for heatwave progression  
- Visual tracking of evolving grid stress  

The visualization consumes structured simulation outputs and does not embed risk logic internally, preserving system modularity.

---

## Quantitative Scope

- 20 synthetic transformer nodes  
- 7-stage environmental progression  
- Normalized risk scoring (0–1)  
- Deterministic classification logic  
- Structured JSON/CSV pipeline  

---

## Limitations

- Uses synthetic grid data rather than real utility datasets  
- Does not explicitly model transformer internal hot-spot temperature  
- Risk model is rule-based rather than learned from historical data  
- Spatial relationships between nodes are approximated  

---

## Lab Usefulness

- Provides a reusable foundation for grid stress simulation and demonstration  
- Demonstrates a clean pipeline from simulation output to interactive visualization  
- Establishes a framework for future integration of real datasets, ML models, or optimization systems  
