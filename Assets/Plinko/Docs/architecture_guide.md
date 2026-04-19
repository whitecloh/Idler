# Architecture Guide

## Current Superseding Update

This section overrides older battle-related wording below if there is any conflict.

- Supported authored `LevelType` values are now:
  - `Purchase`
  - `Retraining`
  - `FieldUpgrade`
  - `StandardBattle`
  - `DefenceBattle`
  - `PowerLineBattle`
- Battle is not an implicit second half of other levels.
- `BattleMode` is not used as an architectural concept anymore.
- Each battle-style level is a separate authored level type with:
  - separate runtime contract
  - separate save/load behavior
  - separate UI payload
  - separate primary window
  - separate ECS systems where the gameplay rules materially differ

### Current battle types

- `StandardBattle`
  - classic base-vs-base battle
  - turn-based deployment flow
  - enemy base HP exists
- `DefenceBattle`
  - grid/lane defence flow
  - player survives configured waves
  - enemy base does not exist
- `PowerLineBattle`
  - real-time lane escort/combat level
  - player goal is to connect 4 plugs to enemy base sockets
  - no turns, no start-battle button, no mid-level save
  - mana regenerates continuously by ticks
  - reroll costs mana
  - hand size is constant and a single card is drawn after each successful deploy

### PowerLineBattle runtime contract

- This mode is a separate `LevelType.PowerLineBattle`.
- Save policy:
  - save at level start
  - save at level completion
  - no mid-level save
  - `Continue` restarts this level from the beginning
- Lanes:
  - 4 named lanes: `Red`, `Yellow`, `Blue`, `Green`
  - units and enemies move only on their lane
  - multiple allies and multiple enemies may coexist on the same lane
  - allies do not block allies, enemies do not block enemies
- Plug state per lane:
  - `AtSpawn`
  - `Carried`
  - `Dropped`
  - `Connected`
- When a carrier dies, the plug drops on that lane.
- When another allied unit reaches the dropped plug, it picks it up.
- When a plug reaches the enemy base:
  - the lane becomes connected
  - all units on that lane are removed
  - future enemy spawns on that lane are cancelled
  - player can no longer spawn units on that lane
- Victory:
  - all 4 lanes connected
- Defeat:
  - player base HP <= 0

### Shared unit/enemy stats

Unit/enemy authored data is shared across all battle level types. Shared combat fields now include:

- `Attack`
- `Health`
- `MoveSpeed`
- `AttackRange`
- `AttackSpeed`

Pins and training are allowed to affect these shared stats.

## 1. Назначение документа

Этот документ фиксирует актуальную архитектуру `Assets/Plinko` и все ключевые продуктовые решения, которые уже приняты.

Его цель:
- быть единым source of truth для архитектуры и runtime-контрактов;
- синхронизировать человека и любую другую модель, которая будет писать код в проект;
- не допускать возврата к старым решениям, которые уже были отброшены в ходе разработки.

---

## 2. Технологический baseline

Проект разрабатывается как:
- Unity-проект;
- 2D игра;
- PC-first;
- gameplay runtime на `LeoECSLite`;
- UI на `uGUI + Canvas`;
- анимации на `DOTween`.

Главный приоритет разработки:
1. runtime-логика;
2. save/load и meta progression;
3. UI sync;
4. production UI;
5. визуализация и polish.

---

## 3. Источник истины

### 3.1 Что является authoritative state

Единственный authoritative state для игры:
- `ECS runtime state` для активного run;
- `ScriptableObject` для authored data;
- `Save DTO` только для persistence;
- `Meta save` только для мета-прогресса между сессиями.

UI не является authoritative state.

### 3.2 Что UI делать не должен

UI не должен:
- хранить gameplay-состояние;
- вычислять цену, статы, победу/поражение, progression;
- дублировать runtime-пайплайны;
- становиться источником истины вместо ECS.

UI может:
- показывать уже подготовленный view data;
- отправлять requests через bridge;
- проигрывать анимации и presentation-only feedback.

---

## 4. Архитектурные слои

Проект делится на следующие слои:

1. `Data`
- ScriptableObject-конфиги;
- authored контент;
- локации, уровни, юниты, пины, враги, поля, unlock-условия.

2. `Models`
- runtime model;
- save DTO;
- battle timeline/result;
- view data.

3. `Services`
- конфиг-сервисы;
- save/meta сервисы;
- runtime helper-сервисы;
- общие deterministic utility-алгоритмы.

4. `ECS`
- components;
- requests/events;
- systems;
- indexes;
- composition root.

5. `View`
- bridges;
- screen/panel controllers;
- item views;
- animation helpers.

---

## 5. Правила по authored data

### 5.1 Visual data в assets

Визуальные данные в authored assets хранятся как sprite-based данные, а не как готовые gameplay-префабы.

Принятый подход:
- в data лежат `Sprite`, `Sprite[]`, animation sets и другие простые визуальные ссылки;
- UI/view layer сам использует scene-authored/view-authored префабы и инициализирует их данными.

Это уже закреплено для:
- `PinTypeData`;
- `BasketTypeData`;
- `UnitTypeData`;
- `EnemyUnitSpawnData`;
- `LevelData`;
- `LocationData`.

### 5.2 Runtime entity != type config

Нужно жёстко различать:
- `UnitTypeData` как authored template;
- `owned unit` как конкретную runtime-сущность игрока;
- `pin type` как authored template;
- `installed board pin` как конкретный runtime pin на поле.

Особенно важно это для:
- retraining;
- hand generation;
- save/load;
- battle deploy.

---

## 6. Архитектура run и progression

### 6.1 Меню и локации

Игра стартует в меню.

Игрок может:
- начать новый run через выбор локации;
- продолжить незавершённый run;
- выйти из игры.

Выбор локации:
- является popup внутри меню;
- использует meta progression;
- позволяет выбрать только открытую или уже пройденную локацию;
- по умолчанию выбирает последнюю доступную открытую локацию.

### 6.2 Meta progression

`Meta save` хранит:
- завершённые локации;
- прогресс анлоков;
- состояние, нужное для открытия следующих локаций.

`Run save` хранит:
- только текущую незавершённую игровую сессию.

### 6.3 Уровни в локации

Локация состоит из последовательности authored уровней.

Каждый уровень имеет свой `LevelType`:
- `Purchase`;
- `Retraining`;
- `FieldUpgrade`;
- `StandardBattle`.
- `DefenceBattle`.

Battle больше не является implicit-частью других уровней. Это отдельный authored тип уровня.

---

## 7. Battle Level Types

Боевые уровни больше не используют вложенный `BattleMode`.

Поддерживаемые authored level types:
- `LevelType.StandardBattle`;
- `LevelType.DefenceBattle`.

Каждый боевой тип уровня:
- задаётся напрямую в `LevelData`;
- имеет свой runtime contract;
- имеет свой save/load contract;
- имеет свой UI payload;
- имеет свой resolver и свои victory/defeat conditions.

Не допускается попытка впихнуть оба боевых типа в один oversized resolver или один oversized battle screen с большим числом случайных `if`-веток. Разделение по type-level является частью архитектурного контракта проекта.

---

## 8. Phase model

Текущие gameplay-фазы:
- `MainMenu`;
- `PurchasePhase`;
- `RetrainingPhase`;
- `FieldUpgradePhase`;
- `BattlePreparation`;
- `StandardBattle`;
- `DefenceBattle`;
- `BattlePlayback`;
- `Result`.

Каждая ECS-система должна быть phase-aware и работать только внутри корректной фазы.

---

## 9. Окна и popups

### 9.1 Primary windows

В игре одновременно должно быть открыто только одно primary window:
- `MainMenu`;
- `Purchase`;
- `Retraining`;
- `FieldUpgrade`;
- `StandardBattle`;
- `DefenceBattle`;
- `BattleResult`.

Этим управляет `UiWindowManager`.

### 9.2 Popups

Popup не являются primary windows.

На текущий момент:
- `LocationSelection` является popup поверх меню;
- popup может быть открыт поверх своего родительского окна;
- popup не должен сам ломать контракт `one primary window at a time`.

---

## 10. Scene-authored UI policy

### 10.1 Что делать с UI

UI собирается вручную в сценах и префабах.

Код со стороны модели/агента должен:
- писать только scripts;
- работать через `SerializeField`;
- предполагать, что layout собирается руками;
- не создавать иерархию UI на лету.

### 10.2 Что запрещено

Запрещено:
- runtime UI factory как основной подход;
- автоматическое создание недостающих окон или контроллеров;
- `GetComponent` в view-слое;
- кастомный validation-layer для UI ради “мягких” ошибок;
- defensive UI-код, который тихо скрывает отсутствие критичных ссылок.

Принятая модель:
- если обязательная ссылка не прокинута, это ошибка сборки сцены/префаба;
- Unity сам покажет проблему стандартной ошибкой;
- кастомный validator для этого не нужен.

### 10.3 Разделение view-слоя

View-слой организуется так:
- `ScreenController` управляет окном;
- `PanelController` управляет частью окна;
- `ItemView` висит на prefab-элементе;
- layout и префабы собираются вручную в редакторе.

---

## 11. Animation policy

### 11.1 Централизация

Все UI-анимации должны идти через общий animation manager.

Текущий approved путь:
- `UiAnimationManager` является общим местом запуска UI-анимаций;
- отдельные view/item-классы не должны самостоятельно создавать произвольные DOTween-пайплайны, если это можно делегировать в manager.

### 11.2 Безопасность

Анимационный слой должен учитывать:
- объекты могут удаляться в середине анимации;
- анимация не должна ломать runtime flow;
- presentation не должен становиться gameplay logic.

---

## 12. Purchase phase

### 12.1 Что делает phase

`PurchasePhase` отвечает за:
- генерацию магазина юнитов;
- reroll магазина;
- покупку юнита;
- старт training для купленного юнита;
- блокировку перехода дальше, пока training активен.

### 12.2 Training contract

Покупка юнита запускает общий plinko training pipeline:
1. рассчитывается результат;
2. создаётся playback runtime;
3. playback проигрывается;
4. результат финализируется;
5. юнит попадает в owned pool.

---

## 13. Retraining phase

### 13.1 Принятая модель

Retraining больше не работает как ручной выбор юнитов.

Текущая модель:
- при входе в retraining phase строится batch-shop;
- batch состоит из `M` owned units;
- `M` берётся из level override или game settings;
- если eligible units меньше `M`, показывается только доступное количество;
- batch строится из owned entities, а не из unit types;
- цена batch = сумма `UnitTypeData.ShopPrice` всех юнитов batch;
- reroll выбирает новый batch из eligible pool;
- reroll не гарантирует новый состав;
- если текущий batch уже показывает все eligible units, reroll недоступен;
- юниты, уже upgraded на этом уровне, больше не участвуют в генерации batch на этом уровне;
- переход на следующий уровень доступен, если нет активного training.

### 13.2 Training contract

Retraining использует тот же plinko training pipeline, что и purchase.

Дублировать отдельный retraining pipeline запрещено.

---

## 14. FieldUpgrade phase

`FieldUpgradePhase` отвечает за:
- генерацию магазина пинов;
- reroll;
- покупку пина;
- создание pending pin;
- выбор board slot;
- замену установленного пина;
- сохранение board state.

Логика выбора и замены пина живёт в runtime, а UI только показывает:
- доступные пины;
- выбранный пин;
- pending pin;
- состояние блокировки интерфейса на время выбора.

---

## 15. Shared plinko training pipeline

Purchase и Retraining обязаны использовать один и тот же pipeline.

Запрещено:
- иметь два независимых куска логики для расчёта training;
- иметь разные правила прохождения пинов для purchase и retraining.

---

## 16. Save / Load policy

### 16.1 Checkpoints

Текущая политика сохранений:
- автосейв на старте уровня;
- автосейв на старте хода игрока в battle после подготовки состояния хода;
- `meta save` хранится отдельно от `run save`.

### 16.2 Повреждённый save

Если `run save` повреждён или неполон:
- игра не пытается продолжать run в неконсистентном виде;
- текущая локация перезапускается заново;
- создаётся новый корректный run save.

### 16.3 Что не нужно делать

Не нужно:
- использовать UI как промежуточный источник восстановления состояния;
- допускать “почти рабочий” broken resume;
- сохранять mid-animation presentation state как authoritative gameplay state.

---

## 17. Standard Battle

### 17.1 Назначение

`LevelType.StandardBattle` — классический режим “база игрока против базы врага”.

### 17.2 Базовые правила

Принятые правила:
- у обеих сторон есть база;
- рука генерируется в начале хода;
- мана восстанавливается в начале хода;
- игрок deploy-ит юнитов в порядке, который влияет на стартовую линию;
- бой стартует только по кнопке;
- после боя либо начинается следующий ход, либо показывается `Result`.

### 17.3 Текущий статус

Runtime и часть UI для Standard уже существуют, но это больше не главный приоритет разработки.

Текущий основной боевой фокус проекта — `BaseDefense`.

---

## 18. BaseDefense Battle

### 18.1 Назначение

`LevelType.DefenceBattle` — отдельный режим уровня, ближе по принципу к `Plants vs Zombies`.

### 18.2 Победа и поражение

Условия:
- у врага нет базы;
- у игрока есть база с HP и progress meter;
- победа = пережить все authored волны;
- поражение = HP базы игрока `<= 0`;
- заполнение progress meter равно числу пережитых волн/ходов;
- required turns равны количеству configured waves.

### 18.3 Поле

Поле:
- клеточное;
- по умолчанию `4 x 6`;
- разделено на сторону игрока и сторону врага;
- дополнительно есть preview-клетки для следующей волны.

Допустимая occupancy:
- `player + player` в одной клетке нельзя;
- `enemy + enemy` можно;
- `enemy + player` можно.

### 18.4 Игрок

Игрок:
- получает руку как обычно;
- тратит mana на deploy;
- ставит юнитов в клетки своей половины поля;
- после размещения юниты игрока стоят на месте;
- в начале каждого хода mana растёт от `StartingMana` до `MaxMana`.

### 18.5 Враги

Враги:
- спавнятся authored волнами по ходам;
- прикреплены к своим линиям, если у них нет разрешения менять линию;
- идут к базе игрока;
- если достигли клетки у базы и живы, бьют базу каждый ход;
- если игрок поставил юнита в клетку с таким врагом, враг на следующей проверке таргетинга может сменить цель.

### 18.6 Боевая логика

Принятые правила:
- player units статичны;
- enemy units ищут ближайшую валидную цель;
- если цель в range, атакуют;
- если цель вне range, двигаются к ней;
- `attackRange = 1` означает соседнюю клетку;
- `attackRange = 0` означает ту же клетку;
- cross-line attack использует Manhattan distance;
- `CanAttackOtherLines` и `CanMoveBetweenLines` — разные флаги;
- если юнит может атаковать другую линию, но не может менять линию, он атакует cross-line цель только если она уже в range из текущей линии;
- если враг может менять линию, то за ход всё равно двигается только на одну клетку.

### 18.7 Текущий статус

Для BaseDefense уже реализованы:
- authored data contract;
- runtime state;
- отдельный resolver;
- save/load;
- battle HUD payload;
- scene-authored UI shell для экрана, панели хода и панели поля.

Ещё не завершены:
- production-quality playback;
- полноценное presentation окна результата;
- финальный polish анимаций.

---

## 19. Правила по ECS

### 19.1 Gameplay logic только в ECS

Gameplay logic живёт в ECS systems.

Сюда входят:
- run start / continue;
- level routing;
- shops;
- purchases;
- training;
- field upgrade;
- battle turn loop;
- battle resolution;
- save/load.

### 19.2 Requests и Events

Использовать строго:
- `Request` = нужно выполнить действие;
- `Event` = действие уже произошло.

### 19.3 Composition root обязателен

Любая новая система считается незавершённой, если она не подключена в `EcsCompositionRoot`.

---

## 20. Что запрещено

Запрещено:
- переносить gameplay logic в UI;
- делать runtime UI generation как основной способ сборки интерфейса;
- использовать `GetComponent` во view-слое;
- вводить кастомный validator layer ради UI;
- silently hide broken dependencies через defensive UI code;
- плодить дублирующие пайплайны;
- делать большие несогласованные рефакторинги вне текущей задачи;
- добавлять presentation-полировку до согласования gameplay-контракта.

---

## 21. Практическое правило перед новой реализацией

Перед добавлением новой функциональности нужно ответить на вопросы:
1. В каком слое должна жить логика?
2. Кто authoritative state?
3. Это новый runtime contract или расширение существующего?
4. Уже есть общий pipeline, который надо переиспользовать?
5. Не нарушает ли изменение scene-authored UI policy?
6. Не ломает ли изменение save/load?

Если на любой вопрос нет чёткого ответа, сначала нужно зафиксировать решение в документах, и только потом писать код.
