# Research Justification: Transformer Stress Modeling Assumptions

## Purpose
This document outlines the research-informed assumptions used in the
Quantum-Inspired Grid Stress Digital Twin prototype. The goal is to
create a **plausible, interpretable, and defensible approximation** of
transformer stress behavior during extreme heat events in the Southern
United States.

This is a **conceptual digital twin prototype**, not a validated
engineering or failure-prediction model.

---

## Regional Climate Context (Southern U.S.)
Southern states such as Florida experience frequent summer heatwaves
with sustained high ambient temperatures.

Based on NOAA climate data:
- Typical summer daytime temperatures range from **85–90°F**
- Heatwave conditions commonly reach **95–100°F**
- Extreme events can exceed **100°F** for sustained periods

These ranges are used to define ambient temperature inputs for the
simulation.

**Source:** NOAA National Centers for Environmental Information

---

## Transformer Thermal Stress Fundamentals
Transformer thermal stress increases due to two primary factors:

### 1. Electrical Load
- Higher load increases internal winding and oil temperatures
- Operating near or above rated capacity accelerates aging
- Sustained loads above ~85–90% are commonly treated as warning or
  critical conditions in utility operations

### 2. Ambient Temperature
- Transformers rely on ambient air for cooling
- Higher air temperature reduces cooling effectiveness
- High nighttime temperatures during heatwaves prevent cooldown

IEEE literature commonly references **110°C hot-spot temperature** as a
critical aging threshold. This prototype does not explicitly simulate
hot-spot temperature but approximates risk using ambient temperature
and load interactions.

---

## Synthetic Load Behavior During Heatwaves
Utility studies and grid research show that:
- Electrical demand increases during heatwaves due to air conditioning
- Load increases of **5–15%** are typical during extreme heat events
- Sustained peak load often persists overnight

In this prototype:
- Baseline transformer load is initialized between **30–60%**
- Load increases proportionally with ambient temperature
- Load is capped at **100%** to represent rated capacity

This approach provides realistic demand behavior without requiring
detailed demand forecasting models.

---

## Neighbor Stress Approximation
In real distribution systems:
- Transformers and feeders share localized demand patterns
- Stress often clusters geographically
- Overloads are rarely isolated to a single asset

To approximate this behavior:
- Each transformer node tracks a simple neighbor stress indicator
- Neighbor stress increases when nearby nodes exceed load thresholds

This provides a lightweight proxy for localized congestion without
full power-flow modeling.

---

## Risk Scoring Approach (Prototype)
The prototype computes a normalized risk score using a weighted sum:

risk_score =
  w_load * load_pct +
  w_temp * normalized_temperature +
  w_neighbor * neighbor_stress

Default weights:
- **Load (0.5)** — primary contributor to thermal stress
- **Temperature (0.3)** — strong secondary contributor
- **Neighbor stress (0.2)** — captures clustering effects

Weights are intentionally simple and tunable.

---

## Risk Thresholds
The following thresholds are used to classify risk levels:

| Condition | Threshold |
|---------|----------|
| Load warning | > 85% |
| Load critical | > 90% |
| Temperature warning | > 95°F |
| Temperature critical | > 98°F |

These thresholds are consistent with utility planning practices and
grid resilience studies and are used solely for visualization and
early-warning demonstration.

---

## Limitations
- No power-flow simulation
- No oil or winding hot-spot modeling
- No failure prediction
- No real utility data ingestion

These limitations are intentional to maintain interpretability and
scope suitability for a three-week prototype.

---

## Future Validation & Extension
This framework is designed to support future enhancement with:
- IEEE-based thermal equations
- Machine learning forecasting
- Real sensor or SCADA data
- Quantum optimization solvers (QUBO-based)
