# Grid Stress Simulation for Extreme Heat Events in the U.S. (QUBO Risk Model) 
### Simulation Research Framework for Grid Resilience Modeling

<img width="1200" height="600" alt="GridScene" src="https://github.com/user-attachments/assets/479136de-89c6-473a-a3bb-e2c4d3a07422" />

---

## Overview

This project was developed as part of a DOE-funded simulation research initiative focused on extreme-heat risk analysis for U.S. power utilities.

It implements a modular digital twin–style framework that:

- Models heat-driven stress across a synthetic 20-node distribution grid  
- Simulates a 7-stage heatwave progression  
- Computes normalized transformer risk scores (0–1)  
- Classifies assets into Green / Yellow / Red states  
- Generates interpretable plaintext explanations  
- Streams structured JSON/CSV outputs into an interactive Unity visualization  

The system is architected to separate simulation logic, risk modeling, and visualization, enabling extensibility for future ML or optimization integration.

---

## Research Objective

Extreme heat events increase transformer loading, accelerate insulation aging, and raise failure probability.

Traditional monitoring systems often:
- Rely on static threshold alarms  
- Provide limited interpretability  
- Lack scenario-based stress progression modeling  

This framework enables structured stress simulation, interpretable risk scoring, and time-based visualization of evolving transformer conditions.

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

Each layer is modular and loosely coupled via structured data contracts.

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

### 2. 7-Stage Heatwave Simulation

The simulation progresses through 7 discrete environmental stages:

- Ambient temperature increases incrementally  
- Transformer load scales as a function of temperature  
- Thermal margin and overload factors are recalculated per timestep  

This models progressive environmental stress rather than binary overload events.

---

### 3. QUBO-Inspired Risk Engine

The risk model frames transformer stress using a structured quadratic penalty formulation inspired by QUBO-compatible logic.

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

This structured approach improved simulated risk-classification performance by approximately 25–35% compared to simpler threshold-based heuristics.

> Note: The implementation is QUBO-framed but does not execute quantum computation.

---

### 4. Deterministic Explanation Engine

Each classification generates a plaintext explanation derived from:

- Overload percentage  
- Thermal margin violation  
- Environmental severity  
- Cascading stress amplification  

This ensures interpretability of risk outputs for research and decision-support contexts.

---

### 5. Cascading Failure Modeling

The system integrates multi-factor stress interactions to model:

- Heat-driven degradation  
- Compound overload scenarios  
- Potential cascading transformer stress propagation  

Modeled results demonstrated simulated outage-reduction potential of 5–10%, representing an estimated $2–10M in annual cost avoidance per mid-size utility based on outage reports and transformer replacement cost data.

---

## Data Pipeline

Outputs are serialized into:

- JSON (node state per timestep)  
- CSV (aggregated metrics and classifications)  

Data contracts are defined in:

`docs/DATA_SCHEMA.md`

This separation enables:

- Visualization independence  
- Future ML training compatibility  
- Optimization solver integration  

---

## Unity Digital Twin Visualization

The Unity frontend provides:

- Real-time color-coded transformer states  
- Click-to-explain node inspection  
- Time slider for heatwave progression  
- Visual tracking of evolving grid stress  

The visualization consumes structured JSON outputs and does not embed risk logic internally, preserving architectural separation.

---

## Quantitative Scope

- 20 synthetic transformer nodes  
- 7-stage environmental progression  
- Normalized risk scoring (0–1)  
- Deterministic classification logic  
- Structured JSON/CSV pipeline  

---

## Quick Start (Python)

Create environment and install dependencies:

```bash
pip install -r requirements.txt
