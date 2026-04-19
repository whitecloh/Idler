# Project Handoff Context

## Current Superseding Update

This block is the fastest way to put another model into the current project context.

### Current level types

- `Purchase`
- `Retraining`
- `FieldUpgrade`
- `StandardBattle`
- `DefenceBattle`
- `PowerLineBattle`

### Current architecture rule for battles

- There is no shared `BattleMode` architecture anymore.
- Each battle gameplay family is a separate `LevelType`.
- Each battle level type owns:
  - its own runtime contract
  - its own UI payload
  - its own primary window
  - its own ECS systems where rules differ

### PowerLineBattle short spec

- Separate `LevelType.PowerLineBattle`
- Real-time tick-driven level
- 4 named lanes: `Red`, `Yellow`, `Blue`, `Green`
- Player base has HP
- Enemy base has 4 sockets, one per lane
- Goal: carry plugs from player side to enemy side and connect all 4 lanes
- After a lane is connected:
  - all units on that lane are removed
  - future enemy spawns on that lane are cancelled
  - the lane can no longer be used for player spawns
- Defeat: player base HP <= 0
- Save/load:
  - save at level start
  - save at level completion
  - no mid-level save
  - `Continue` restarts this level from the start
- Hand flow:
  - constant hand size from `GameSettingsData`
  - duplicates allowed
  - after deploy, draw exactly one new card
  - reroll clears current hand and draws a full new hand
  - reroll costs mana
- Mana flow:
  - `startingMana`
  - `maxMana`
  - `manaPerTick`
  - `manaTickInterval`
- Shared combat stats used by authored unit/enemy data:
  - `Attack`
  - `Health`
  - `MoveSpeed`
  - `AttackRange`
  - `AttackSpeed`

## 1. Что это за проект

Это Unity-проект `Assets/Plinko`.

Жанрово это session-based strategy/card game с несколькими типами уровней:
- `Purchase`;
- `Retraining`;
- `FieldUpgrade`;
- `StandardBattle`.
- `DefenceBattle`.

Проект уже ушёл далеко от исходного прототипа. Здесь нельзя писать код “по наитию”. Нужно опираться на текущие runtime-контракты, документы и уже принятые решения.

---

## 2. Технологический стек и общие правила

Стек:
- Unity;
- 2D;
- PC-first;
- `LeoECSLite` для gameplay runtime;
- `uGUI + Canvas` для UI;
- `DOTween` для UI/presentation анимаций.

Главные правила:
- runtime authoritative, UI passive;
- UI собирается вручную в сценах и префабах;
- код пишет scripts, но не создаёт UI-иерархию на лету;
- во `View` не использовать `GetComponent`;
- не писать кастомный validator layer для UI;
- обязательные UI-ссылки считаются обязанностью сборки сцены/префаба;
- для UI-анимаций используется общий `UiAnimationManager`.

---

## 3. Архитектура

### 3.1 Источник истины

Source of truth:
- `ECS runtime state` для игрового процесса;
- `ScriptableObject` для authored контента;
- `Run save DTO` только для текущей сессии;
- `Meta save DTO` только для мета-прогресса.

UI не является source of truth.

### 3.2 Основные слои

1. `Data`
- authored контент, конфиги, визуальные данные.

2. `Models`
- runtime models;
- save DTO;
- battle timeline/result;
- view data.

3. `Services`
- config/save/meta/runtime helpers.

4. `ECS`
- components;
- requests/events;
- systems;
- composition root.

5. `View`
- bridges;
- screen/panel controllers;
- item views;
- animation/presentation helpers.

---

## 4. Текущий product flow

### 4.1 Menu flow

Игрок стартует в меню.

Меню:
- `Play` открывает popup выбора локации;
- `Continue` продолжает незавершённый run, если он есть;
- `Exit` закрывает игру.

Popup выбора локации:
- является popup внутри меню;
- показывает открытые/закрытые локации;
- по умолчанию выбирает последнюю открытую.

### 4.2 Location / progression

Локация содержит последовательность authored уровней.

Порядок уровней полностью задаётся `LocationData`.

Локации открываются через meta progression и authored unlock conditions.

### 4.3 Save / Continue

`Continue` ведёт в незавершённый run.

Если save повреждён:
- игра не пытается продолжать broken state;
- текущая локация перезапускается с новым run save.

---

## 5. Level types

Поддерживаемые типы уровней:
- `Purchase`;
- `Retraining`;
- `FieldUpgrade`;
- `StandardBattle`.
- `DefenceBattle`.

Battle — отдельный authored уровень. Он больше не является implicit частью других фаз.

---

## 6. Purchase

Логика:
- генерируется магазин юнитов;
- игрок покупает юнит;
- покупка сразу запускает training через plinko;
- после завершения training новый owned unit попадает в пул игрока.

UI:
- окно уже собрано как scene-authored production-style shell;
- есть отдельные панели:
  - plinko field;
  - level track;
  - shop;
  - next level;
  - trained units.

---

## 7. Retraining

### 7.1 Текущая модель

Retraining больше не работает через ручной выбор юнитов.

Текущий flow:
- при входе в уровень генерируется batch-shop из `M` owned units;
- если eligible units меньше `M`, показывается меньше;
- batch строится из owned entities, а не из типов;
- кнопка покупки одна на весь batch;
- цена batch = сумма `UnitTypeData.ShopPrice` всех юнитов batch;
- reroll выбирает новый batch из eligible units;
- reroll не гарантирует новый состав;
- юниты, уже upgraded на этом уровне, исключаются из дальнейшей генерации batch на этом уровне;
- следующий уровень доступен, если нет активного training.

### 7.2 Важное правило

Retraining использует тот же plinko training pipeline, что и purchase.

---

## 8. Field Upgrade

Логика:
- генерируется магазин пинов;
- игрок покупает pin;
- pin становится pending replacement;
- поле затемняется overlay, кроме plinko area;
- игрок выбирает pin на поле для замены;
- подтверждает или отменяет;
- board state сохраняется.

Важно:
- логика замены живёт в runtime;
- UI только показывает pending/selected state.

---

## 9. Shared plinko pipeline

Purchase и Retraining используют один и тот же training pipeline.

Общий контракт:
1. сначала рассчитывается training result;
2. потом создаётся playback runtime;
3. потом идёт playback;
4. потом финализируется результат.

Нельзя вводить отдельный retraining-specific pipeline.

---

## 10. Battle modes

Боевые уровни больше не используют `BattleMode`.

Вместо этого используются отдельные `LevelType`:
- `StandardBattle`
- `DefenceBattle`

Поддерживаемые режимы:
- `Standard`;
- `BaseDefense`.

Важно:
- battle mode влияет на runtime, save/load, UI payload и resolver;
- не нужно пытаться впихнуть оба режима в один большой resolver с хаотическими условными ветками.

---

## 11. Standard Battle

Это классический base-vs-base режим.

Что есть:
- runtime turn loop;
- hand generation;
- deploy;
- wave selection по HP enemy base;
- battle resolution;
- result routing;
- battle HUD payload.

Статус:
- режим реализован как runtime;
- не является текущим главным приоритетом проекта;
- product focus переключён на `BaseDefense`.

---

## 12. BaseDefense

### 12.1 Суть режима

Это отдельный battle mode, ближе по логике к `Plants vs Zombies`.

Ключевые правила:
- у врага нет базы;
- у игрока есть база с HP и progress meter;
- победа = пережить все волны;
- поражение = HP базы игрока `<= 0`.

### 12.2 Поле

Поле клеточное.

Текущий контракт:
- `4 линии x 6 клеток`;
- половина клеток — сторона врага;
- половина клеток — сторона игрока;
- дополнительно есть preview slots для следующей волны.

Occupancy:
- `player + player` в одной клетке нельзя;
- `enemy + enemy` можно;
- `enemy + player` можно.

### 12.3 Мана

Мана в BaseDefense:
- имеет `StartingMana`;
- имеет `MaxMana`;
- растёт каждый ход до капа;
- тратится на deploy карт.

### 12.4 Юниты игрока

Правила:
- игрок получает руку как обычно;
- по клику/выбору ставит карту на доступную клетку своей стороны;
- после размещения юнит остаётся статичным;
- `player units` не двигаются.

### 12.5 Враги

Правила:
- враги приходят authored волнами по ходам;
- у них есть линия и клетка спавна;
- враги ищут ближайшую валидную цель;
- если цель в range — атакуют;
- если нет — двигаются к ней;
- враг у клетки базы может бить базу каждый ход;
- если игрок поставил юнита в клетку такого врага, враг на следующей проверке может сменить target.

### 12.6 Range и cross-line rules

Принятые правила:
- `attackRange = 1` означает соседнюю клетку;
- `attackRange = 0` означает ту же клетку;
- cross-line attack считает Manhattan distance;
- `CanAttackOtherLines` и `CanMoveBetweenLines` — разные флаги;
- враг/юнит может уметь атаковать другую линию, но не уметь в неё переходить;
- при переходе между линиями за ход всё равно происходит только смещение на одну клетку.

### 12.7 Что уже реализовано

Уже реализованы:
- data structures;
- runtime state;
- отдельный resolver;
- save/load;
- battle HUD payload;
- scene-authored UI shell:
  - экран;
  - панель хода;
  - панель поля.

Что ещё не завершено:
- production-quality playback;
- production result popup;
- полный visual polish.

---

## 13. Save / Load contract

### 13.1 Политика чекпоинтов

Текущая политика:
- save на старте уровня;
- save на старте хода игрока в battle;
- `meta save` отдельно от `run save`.

### 13.2 Broken save policy

Если save частично сломан:
- run не продолжается в неконсистентном виде;
- текущая локация перезапускается заново;
- создаётся свежий корректный run save.

---

## 14. UI architecture

### 14.1 Scene-authored only

Сцены и префабы собирает человек.

Код делает только:
- screen controllers;
- panel controllers;
- item views;
- bridges;
- view data consumption.

### 14.2 Primary window rule

Одновременно открыто только одно primary window:
- menu;
- purchase;
- retraining;
- field upgrade;
- battle;
- battle result.

Popup живут отдельно.

`LocationSelection` — popup внутри меню.

### 14.3 UI layout conventions

Принятый паттерн:
- `ScreenController` управляет окном;
- `PanelController` управляет областью окна;
- `ItemView` висит на prefab item;
- вся wiring делается через `SerializeField`.

### 14.4 Анимации

Для UI-анимаций используется общий `UiAnimationManager`.

Не нужно:
- раскидывать независимые локальные tween-системы по view;
- использовать UI-анимацию как часть gameplay state.

---

## 15. Что уже сделано по UI

Собраны scene-authored окна:
- меню;
- popup выбора локации;
- purchase;
- retraining;
- field upgrade;
- battle shell;
- base defense shell.

Текущий временный хвост:
- `BattleResultScreenController` всё ещё IMGUI-based и должен быть переведён в production-style scene-authored popup.

---

## 16. Файлы, которые стоит смотреть в первую очередь

Если новая модель должна быстро войти в контекст, сначала нужно посмотреть:

### Архитектура и документы
- `Assets/Plinko/Docs/architecture_guide.md`
- `Assets/Plinko/Docs/session_game_roadmap.md`
- `Assets/Plinko/Docs/codex_rules.md`

### Основные runtime-контракты
- `Assets/Plinko/Scripts/Data/Common/Enums.cs`
- `Assets/Plinko/Scripts/Data/Levels/LevelData.cs`
- `Assets/Plinko/Scripts/Data/Battle/BaseDefenseBattleData.cs`
- `Assets/Plinko/Scripts/Services/BattleRuntimeService.cs`
- `Assets/Plinko/Scripts/ECS/Installers/EcsCompositionRoot.cs`

### Core systems
- `Assets/Plinko/Scripts/ECS/Systems/BeginBattleTurnSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/DeployCardSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/SelectEnemyWaveSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/ResolveBattleSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/ResolveBaseDefenseBattleSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/WriteRunSaveSystem.cs`
- `Assets/Plinko/Scripts/ECS/Systems/ContinueRunSystem.cs`

### UI shell
- `Assets/Plinko/Scripts/View/UiCompositionRoot.cs`
- `Assets/Plinko/Scripts/View/UiWindowManager.cs`
- `Assets/Plinko/Scripts/View/Animations/UiAnimationManager.cs`
- `Assets/Plinko/Scripts/View/Controllers/BattleScreenController.cs`
- `Assets/Plinko/Scripts/View/Controllers/BaseDefenseScreenController.cs`

---

## 17. Текущий приоритет для следующей модели

Если новая модель продолжает проект без дополнительных указаний, правильный фокус такой:
1. не углублять `Standard Battle`, если это не требуется явно;
2. продолжать доводить `BaseDefense`;
3. перевести `BattleResult` на scene-authored production UI;
4. завершить battle presentation и feedback для `BaseDefense`;
5. после этого возвращаться к polish и хвостам других режимов.

---

## 18. Критичные правила, которые нельзя нарушать

Нельзя:
- переносить gameplay logic в UI;
- создавать runtime UI вместо scene-authored подхода;
- использовать `GetComponent` в View;
- плодить дублирующие gameplay pipeline;
- вводить кастомную validator-систему для UI;
- silently hide broken dependencies;
- делать крупный рефакторинг без необходимости;
- изменять product contract, не обновив документы.

---

## 19. Краткий вывод

Проект уже не в стадии “скелета”. Здесь есть реальный runtime, real save/load, menu/location flow, scene-authored UI окна и два battle mode-контракта.

Новая модель должна работать не как исследователь с нуля, а как инженер, который продолжает конкретную уже сложившуюся архитектуру.
