# CODEX_RULES.md

## 1. Role

You are working on **Session Game**, a Unity project built around **LeoECSLite**.

Your role is to implement the project as a **gameplay-first ECS architecture**, not as a UI-first prototype.

---

## 2. Core rules

### 2.1 Gameplay first
Always prioritize:
1. ECS runtime logic
2. save / restore consistency
3. phase-specific gameplay flow
4. UI sync
5. visuals / animation

Do **not** implement visuals or UI-driven work unless explicitly asked and unless the current roadmap step requires it.

### 2.2 Follow the roadmap strictly
Always follow [session_game_roadmap.md](/U:/UNITY%20PROJECTS/Idler/Assets/Plinko/Docs/session_game_roadmap.md).

Do not jump ahead.
Do not mix multiple roadmap steps in one task unless explicitly asked.

### 2.3 Follow the architecture guide strictly
Always follow [architecture_guide.md](/U:/UNITY%20PROJECTS/Idler/Assets/Plinko/Docs/architecture_guide.md).

If a requested change conflicts with it, explain the conflict and propose the architecture-safe solution.

---

## 3. Hard constraints

You must not:
- move gameplay logic into UI classes;
- invent a second runtime pipeline if one already exists;
- mix old and new architecture versions;
- rename critical runtime contracts without reason;
- perform broad refactors outside the current task;
- add animation/visual layers while gameplay systems are still incomplete;
- use MonoBehaviour state as the gameplay source of truth;
- introduce singleton-heavy gameplay architecture for core runtime state.

---

## 4. Source of truth

The authoritative state is:
- **ECS runtime state** for gameplay
- **Save DTOs** only for persistence
- **ScriptableObject data** only for authored content

UI is never authoritative.

---

## 5. ECS rules

### 5.1 Systems own the gameplay
Gameplay logic must live in ECS systems.

### 5.2 Requests vs Events
- Request = ask to do something
- Event = something already happened

Use them consistently.

### 5.3 Phase guards are required
A system must verify that it is operating inside the correct phase before mutating state.

### 5.4 One-shot events must be cleaned
If an event is intended to be transient, it must be handled and then cleared by cleanup flow.

### 5.5 New systems must be wired
Any newly implemented system must be connected in `EcsCompositionRoot`.

A system that exists but is not wired is considered incomplete.

---

## 6. Shared pipelines

If a shared gameplay pipeline already exists, reuse it.

Example:
- purchase training and retraining must use the same Plinko training pipeline.

Do not duplicate logic unless explicitly requested.

---

## 7. Scope discipline

For each task:
- implement only the systems required by that roadmap step;
- avoid unrelated file changes;
- avoid “cleanup refactors” outside the requested scope;
- keep changes targeted and easy to review.

---

## 8. Style of implementation

Prefer:
- clear class names;
- single-responsibility systems;
- compile-oriented code;
- explicit runtime flow;
- minimal but correct changes.

Avoid:
- vague helper abstractions;
- dead temporary code;
- duplicate parallel flows;
- hidden side effects.

---

## 9. Expected task format

When given a task, interpret it like this:

1. identify the roadmap step;
2. identify the exact systems to implement;
3. respect current runtime contracts;
4. avoid touching UI/visuals unless asked;
5. return concise implementation notes.

---

## 10. Required answer format

After making changes, always summarize:

1. what was implemented;
2. which files were changed;
3. how the runtime flow works now;
4. what edge cases were handled;
5. what remains unfinished inside the current roadmap step.

---

## 11. Validation checklist

Before considering the task done, verify:
- the logic is in ECS, not in UI;
- the correct phase guards exist;
- events/requests are used correctly;
- composition root wiring is present;
- no duplicate gameplay pipeline was introduced;
- save/runtime contracts were not broken.

---

## 12. Preferred workflow for this repository

Use this working order:

1. understand the current roadmap step;
2. inspect related components, requests, events, services and systems;
3. implement the smallest correct vertical slice;
4. wire systems into composition root;
5. summarize the resulting flow.

---

## 13. What to do when uncertain

If uncertain:
- prefer the safer architecture-preserving option;
- do not invent missing product decisions silently;
- keep the implementation minimal and aligned with the existing baseline;
- explicitly note assumptions in the summary.

---

## 14. Project-specific implementation order

Current target order:
1. Purchase phase
2. Retraining phase
3. Field upgrade phase
4. Shared Plinko pipeline stabilization
5. Hand generation and deployment
6. Enemy wave selection
7. Battle resolution
8. Battle outcome routing
9. Save/load stabilization
10. UI sync
11. Visuals / animations

Do not reorder this unless explicitly instructed.

---

## 15. Final instruction

Treat this repository as an **architecture-sensitive gameplay project**, not as a rapid prototype.

Correct flow, runtime consistency and roadmap discipline are more important than speed.
