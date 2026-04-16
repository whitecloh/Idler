# ARCHITECTURE_GUIDE.md

## 1. Назначение документа

Этот документ фиксирует текущую архитектурную базу проекта Session Game.

Цель документа:
- быть единым source of truth для архитектурных решений;
- не допускать смешивания старой и новой логики;
- помогать человеку и агентам писать код в одном стиле;
- задавать границы между gameplay, runtime, save, UI и visual layers.

---

## 2. Технологический baseline

Проект разрабатывается как:
- **Unity project**
- **LeoECSLite** как основной gameplay runtime layer
- **Gameplay-first pipeline**
- **UI later / visuals later**

Главное правило:
- сначала реализуется и стабилизируется системная логика;
- потом runtime/UI sync;
- потом визуализация и анимации.

---

## 3. Основной архитектурный принцип

### 3.1 Источник правды
Основной источник правды для игрового состояния — **ECS runtime state**.

Это означает:
- актуальное состояние run хранится в ECS-компонентах;
- игровые фазы хранятся в ECS;
- owned units, shop offers, installed pins, phase state, battle state — всё это runtime ECS data;
- UI не хранит authoritative-state.

### 3.2 Слои проекта
Проект делится на следующие слои:

1. **Data layer**
   - ScriptableObject-конфиги
   - авторские данные
   - pools, weights, unlocks, level content, plinko field config

2. **Runtime model layer**
   - DTO и runtime models
   - save data
   - plinko results
   - battle timeline

3. **Services layer**
   - config services
   - save services
   - runtime helper services
   - deterministic/shared logic around config/runtime composition

4. **ECS runtime layer**
   - components
   - requests/events
   - indexes
   - systems

5. **UI layer**
   - bridges
   - controllers
   - screen refresh
   - только чтение runtime state и отправка requests

6. **Visual layer**
   - анимации
   - plinko playback visualization
   - battle playback visualization
   - полировка

---

## 4. Правила по ECS

### 4.1 ECS — основной слой gameplay logic
Вся gameplay-логика должна жить в ECS systems.

Сюда относится:
- run start / continue
- level routing
- shop generation
- buying
- training
- retraining
- hand generation
- deployment
- battle resolution
- battle outcome routing
- save requests / save writes

### 4.2 Systems должны быть phase-aware
Каждая фаза должна иметь свою зону ответственности:
- `PurchasePhase`
- `RetrainingPhase`
- `FieldUpgradePhase`
- `BattlePreparation`
- `Battle`
- `BattlePlayback`
- `Result`

Система не должна выполнять логику вне своей валидной фазы.

### 4.3 Runtime state не должен уходить в UI
Нельзя:
- хранить gameplay state в MonoBehaviour view;
- считать ману, статы, офферы, результаты battle внутри UI;
- использовать UI как источник truth.

UI только:
- читает уже рассчитанный state;
- отображает его;
- отправляет requests через bridge.

Технические debug tools допустимы как вспомогательный слой проверки, если:
- они не становятся authoritative-state;
- они не содержат gameplay logic;
- они только читают runtime state и/или отправляют requests для тестового запуска flow.

### 4.4 Requests и Events
Используем чёткое разделение:

- **Request** — сигнал “нужно выполнить действие”
- **Event** — сигнал “действие уже произошло”

Примеры:
- `BuyUnitRequest` — запрос на покупку
- `UnitPurchasedEvent` — покупка уже выполнена

### 4.5 Одноразовые события
Все одноразовые events должны очищаться отдельным cleanup-system.

То есть:
- event не должен жить дольше одного прохода, если он одноразовый;
- нельзя полагаться на lingering events.

---

## 5. Data model rules

### 5.1 ScriptableObject data — авторские данные
Data assets описывают:
- unit types
- pin types
- baskets
- levels
- locations
- unlock conditions
- game settings
- names pools

### 5.2 Runtime-сущность != type config
Важно различать:

- **Unit type** — шаблон из data
- **Owned unit** — уже обученная runtime-сущность игрока

То же относится к:
- pin types vs installed board pins
- authored field layout vs runtime board state

### 5.3 Save DTO отдельно от ECS runtime
Сохранение и загрузка не должны смешиваться с runtime ECS state напрямую.

Правильно:
- ECS state -> Save DTO
- Save DTO -> ECS restore flow

Неправильно:
- считать Save DTO основным runtime форматом
- использовать save classes как runtime storage

---

## 6. Gameplay-specific architecture rules

### 6.1 Purchase phase
Purchase phase отвечает только за:
- unit shop generation
- reroll
- buy
- immediate training start
- blocking battle start while training is active

### 6.2 Retraining phase
Retraining phase отвечает только за:
- selection from owned unit pool
- selection limit
- confirm selection
- sending selected owned units into retraining
- replacing owned units after retraining completes

### 6.3 Field upgrade phase
Field upgrade phase отвечает только за:
- pin shop generation
- reroll
- pin buy
- pending pin placement
- replacing selected board pin
- persisting board state

### 6.4 Архитектура уровней
Каждый уровень теперь является **самостоятельным level type**.

Допустимые типы:
- `Purchase`
- `Retraining`
- `FieldUpgrade`
- `Battle`

Это означает:
- battle больше не является автоматической второй половиной purchase/retraining/field upgrade уровня;
- последовательность уровней полностью задаётся data assets;
- допустимы любые комбинации, включая `Battle -> Battle` без промежуточных фаз;
- переход на следующий уровень выполняется отдельным route/progression flow.

### 6.4 Shared plinko training pipeline
Purchase и retraining обязаны использовать **один и тот же training pipeline**:

1. генерируется результат
2. создаётся playback runtime
3. playback завершается
4. результат финализируется

Нельзя делать:
- один pipeline для purchase;
- второй независимый pipeline для retraining.

### 6.5 Hand generation
Рука игрока генерируется:
- из **owned unit pool**;
- каждый слот генерируется независимо;
- дубликаты допустимы.

### 6.6 Battle
Battle должен быть:
- tick-based;
- автоматическим;
- определённым phase logic;
- сохранять результат в runtime models.

### 6.7 Meta progression
Meta progression хранится отдельно от активного run save.

Разделение такое:
- `run save` хранит только текущую незавершённую сессию;
- `meta save` хранит завершённые локации и прогресс анлоков;
- unit/pin/location unlocks читают именно meta progression, а не active run.

---

## 7. Naming и структура кода

### 7.1 Имена должны быть прямыми и однозначными
Используем понятные имена:
- `GenerateUnitShopOffersSystem`
- `CompletePurchasedTrainingSystem`
- `ReplaceBoardPinSystem`
- `ResolveBattleSystem`

Избегаем:
- расплывчатых имён;
- временных “Temp”, “Helper2”, “ManagerX”.

### 7.2 Один system — одна понятная ответственность
Если system делает слишком много — он должен быть разделён.

### 7.3 Composition root обязателен
Любая новая система должна быть подключена в `EcsCompositionRoot`.

Нельзя считать реализацию завершённой, если:
- system написан,
- но не подключён в композицию.

---

## 8. Запреты

Нельзя:
- смешивать старую архитектуру и новый baseline;
- возвращать старые phase names и старые flow;
- писать gameplay logic в UI;
- реализовывать визуал до завершения core systems;
- дублировать purchase/retraining pipeline;
- делать большой несогласованный рефакторинг вне текущего шага roadmap;
- менять runtime contracts без явной необходимости.

---

## 9. Порядок реализации

Строгий порядок реализации:

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

Если есть сомнение — ориентируемся на roadmap.

---

## 10. Что считать завершённой фичей

Фича считается завершённой, если:
- реализованы все системы её блока;
- учтены phase guards;
- корректно используются requests/events;
- нет дублирующего pipeline;
- код подключён в composition root;
- runtime flow можно объяснить по шагам;
- для неё есть понятный сценарий готовности.

---

## 11. Практическое правило для человека и агента

Перед любой новой реализацией нужно ответить на 5 вопросов:

1. К какой фазе относится логика?
2. Где должен лежать authoritative state?
3. Это request или event?
4. Нет ли уже общего pipeline, который нужно переиспользовать?
5. Подключён ли этот новый блок в composition root?

Если на эти вопросы нет ясного ответа — нельзя начинать кодить.

---

## 12. Итог

Архитектура Session Game строится как:
- ECS-centric
- gameplay-first
- phase-driven
- runtime-authoritative
- UI-passive
- visuals-last

Этот документ нужно использовать как жёсткую архитектурную опору при работе человека, Codex и любых других агентов.
