# План дальнейшей разработки Session Game

Этот документ фиксирует порядок заполнения скриптов и общий принцип разработки. Используем его как опорный roadmap и baseline последовательности работ.

## Общий принцип разработки

Текущая продуктовая модель уровней:
- каждый уровень имеет самостоятельный `LevelType`;
- `Battle` — отдельный тип уровня;
- `Purchase`, `Retraining` и `FieldUpgrade` больше не содержат implicit battle внутри себя;
- порядок уровней полностью определяется списком в `LocationData`.

Текущая модель сохранений:
- `run save` хранит незавершённый run;
- `meta save` хранит progression для анлоков и завершённых локаций.

Текущий приоритет:

1. Gameplay state и ECS flow
2. Сохранение / восстановление
3. Фазы уровня
4. Plinko / training
5. Battle logic
6. UI sync
7. Визуал / анимации

Правило работы:
- сначала добиваем корректную работу систем и флоу;
- UI, анимации и визуальное отображение подключаем в конце;
- не делаем временные UI-костыли до стабилизации gameplay runtime.
- технический debug/dev harness допустим как инструмент проверки, если он не хранит gameplay state и не подменяет ECS runtime.

---

## Что уже заполнено

### База
Уже есть рабочий каркас:
- `GameBootstrapper`
- `GameServicesInstaller`
- `EcsCompositionRoot`
- data/runtime/view models
- request/event contracts
- indexes

### Уже есть живая логика
Уже заполнены:
- `StartNewRunSystem`
- `ContinueRunSystem`
- `RestoreRunBoardSystem`
- `RestoreOwnedUnitsSystem`
- `LoadLevelSystem`
- `RouteLevelTypeToPhaseSystem`
- `InitializeLocationBoardSystem`
- `RegisterOwnedUnitSystem`
- `ReplaceOwnedUnitSystem`
- `WriteRunSaveSystem`
- `GenerateUnitShopOffersSystem`
- `RerollUnitShopSystem`
- `BuyUnitSystem`
- `BeginPurchasedTrainingSystem`
- `AdvancePlinkoTrainingPlaybackSystem`
- `CompletePurchasedTrainingSystem`
- `CleanupTransientEventsSystem`

---

## Чёткий план дальнейшего заполнения скриптов

## Шаг 1. Добить purchase phase до полностью корректного gameplay loop
### Что заполняем
- проверить и при необходимости доработать:
  - `GenerateUnitShopOffersSystem`
  - `RerollUnitShopSystem`
  - `BuyUnitSystem`
  - `BeginPurchasedTrainingSystem`
  - `AdvancePlinkoTrainingPlaybackSystem`
  - `CompletePurchasedTrainingSystem`

### Что должно получиться
- вход в purchase phase;
- магазин генерируется корректно;
- реролл работает;
- покупка списывает золото;
- купленный слот сразу заменяется новым оффером;
- training запускается сразу;
- после завершения training юнит попадает в owned pool;
- в бой нельзя перейти, пока есть активные training.

### Критерий готовности
Сценарий:
`start run -> open purchase -> buy several units -> дождаться training -> owned pool корректно пополнился`

---

## Шаг 2. Реализовать retraining phase
### Что заполняем
- `GenerateRetrainingShopBatchSystem`
- `RerollRetrainingShopSystem`
- `BuyRetrainingBatchSystem`
- `BeginRetrainingSystem`
- `CompleteRetrainingSystem`

### Что должно получиться
- при входе в retraining level генерируется batch из `M` owned units;
- `M` берётся из settings / level override;
- если eligible units меньше `M`, показывается только доступное количество;
- batch строится из owned unit pool, а не из unit types;
- reroll выбирает новый batch из eligible owned units и может показать тот же состав снова;
- reroll недоступен, если текущий batch уже показывает всех eligible units;
- покупка списывает сумму `ShopPrice` всех юнитов batch;
- после покупки весь batch отправляется на retraining;
- юниты, уже upgraded на этом уровне, исключаются из следующих batch generation;
- для них генерируются новые результаты через тот же plinko pipeline;
- после завершения старые версии юнитов заменяются новыми;
- owned pool остаётся консистентным;
- переход на следующий уровень доступен, если нет активного training.

### Критерий готовности
Сценарий:
`enter retraining level -> batch M owned units generated -> buy batch or reroll -> bought units go through playback -> stats/mana обновлены -> upgraded units больше не участвуют в batch generation на этом уровне`

---

## Шаг 3. Реализовать pin shop и field upgrade phase
### Что заполняем
- `GeneratePinShopOffersSystem`
- `RerollPinShopSystem`
- `BuyPinSystem`
- `SelectBoardSlotSystem`
- `ReplaceBoardPinSystem`

### Что должно получиться
- pin shop генерируется случайно по весам;
- дубликаты допустимы;
- реролл обновляет весь магазин;
- покупка пина создаёт pending candidate;
- игрок выбирает слот на поле;
- выбранный pin заменяется;
- board сохраняется в runtime и в save;
- следующий training использует уже обновлённое поле.

### Критерий готовности
Сценарий:
`enter field upgrade -> reroll pin shop -> buy pin -> choose board slot -> board state updated -> save/load keeps board`

---

## Шаг 4. Добить Plinko как единый training pipeline для purchase и retraining
### Что заполняем / усиливаем
- `PlinkoPathFactory`
- `BeginPurchasedTrainingSystem`
- `BeginRetrainingSystem`
- `AdvancePlinkoTrainingPlaybackSystem`
- `CompletePurchasedTrainingSystem`
- `CompleteRetrainingSystem`

### Что нужно проверить и закрыть
- путь идёт только через две ближайшие точки;
- на каждой линии выбирается только одна точка;
- пины дают корректные модификаторы stat/mana;
- корзина выбирается из двух ближайших;
- mana cost формируется корректно;
- purchase и retraining используют один и тот же pipeline, а не две разные реализации.

### Критерий готовности
Любой training в игре работает одинаково:
`generate result first -> playback runtime -> finalize result`

---

## Шаг 5. Реализовать hand generation и deployment
### Что заполняем
- `GenerateHandSystem`
- `ClearHandSystem`
- `DeployCardSystem`

### Что должно получиться
- рука генерируется из `owned unit pool`, а не из типов;
- каждый слот генерируется независимо;
- возможны дубликаты одного и того же owned unit в руке;
- сыгранная карта исчезает только из руки;
- мана тратится корректно;
- в начале нового хода рука может быть пересобрана заново.

### Критерий готовности
Сценарий:
`owned pool exists -> generate hand -> deploy cards -> mana decreases -> hand updates correctly`

---

## Шаг 6. Реализовать enemy wave selection
### Что заполняем
- `SelectEnemyWaveSystem`

### Что должно получиться
- у уровня есть набор HP-threshold waves;
- система выбирает волну по текущему HP enemy base;
- если HP = 75%, берётся wave на 100%;
- если HP = 64%, берётся wave на 65%;
- выбранная волна кладётся в runtime state и используется battle system.

### Критерий готовности
При разных значениях HP enemy base система всегда выбирает правильную волну.

---

## Шаг 7. Реализовать battle resolution
### Что заполняем
- `ResolveBattleSystem`

### Что должно получиться
- battle идёт по тикам;
- юниты ищут ближайшую цель;
- двигаются;
- проверяют range;
- атакуют;
- при необходимости применяют ability;
- бой завершается корректно;
- выжившая сторона наносит урон базе;
- результат записывается в `BattleTimelineModel` и `BattleResultModel`.

### Критерий готовности
Сценарий:
`generate hand -> deploy units -> choose enemy wave -> resolve battle -> получить корректный result`

---

## Шаг 8. Реализовать route after battle
### Что заполняем
- `StartBattlePlaybackSystem`
- `RouteBattleOutcomeAfterPlaybackSystem`
- `AdvanceToNextLevelSystem`
- `ReturnToMenuSystem`

### Что должно получиться
- после battle result игра понимает:
  - победа;
  - поражение;
  - переход на следующий уровень;
  - завершение локации;
  - проигрыш run;
- unlock/meta прогресс может обновляться в нужный момент;
- run state сохраняется корректно.

### Критерий готовности
Полный цикл:
`level start -> prebattle phase -> battle -> result -> next level / fail / complete`

---

## Шаг 9. Добить save/load до финального состояния
### Что проверяем и дополняем
- `WriteRunSaveSystem`
- `ContinueRunSystem`
- `RestoreRunBoardSystem`
- `RestoreOwnedUnitsSystem`

### Что должно быть сохранено без дыр
- `location id`
- `level index`
- `level type`
- `phase type`
- `gold`
- `player base HP`
- `enemy base HP`
- `reroll counts`
- `owned units`
- `board state`

### Критерий готовности
После save/load игра продолжает run без потери логики и данных.

---

## Шаг 10. Только после этого подключаем UI sync
### Что заполняем
- `RefreshPurchasePhaseUiSystem`
- `RefreshRetrainingPhaseUiSystem`
- `RefreshFieldUpgradeUiSystem`
- `RefreshOwnedUnitsUiSystem`
- `RefreshBattleHudUiSystem`
- `RefreshBattleResultUiSystem`

### Что должно получиться
- UI только читает runtime state;
- UI не управляет логикой;
- никаких вычислений gameplay внутри view/controller.

---

## Шаг 11. И только потом визуализация / анимации
### Что добавляем в самом конце
- визуальный plinko playback;
- визуальный battle playback;
- анимации карт / юнитов / пинов;
- экранные переходы;
- polish.

---

## Самый правильный порядок работы прямо сейчас

### Блок A — Pre-battle gameplay
1. Добить `purchase phase`
2. Сделать `retraining phase`
3. Сделать `field upgrade phase`

### Блок B — Shared runtime
4. Унифицировать `plinko training pipeline`
5. Сделать `hand generation + deployment`
6. Сделать `enemy wave selection`

### Блок C — Core battle
7. Сделать `battle resolution`
8. Сделать `battle outcome routing`

### Блок D — Stabilization
9. Довести `save/load`
10. Подключить `UI sync`
11. Подключить `visuals/animations`

---

## Как движемся дальше

Следующий практический шаг:

### Сейчас заполняем `retraining phase`
Потому что:
- purchase pipeline уже есть;
- plinko runtime уже введён;
- retraining — следующий естественный слой поверх уже реализованного training flow.

Следующие целевые системы:
- `SelectUnitsForRetrainingSystem`
- `ConfirmRetrainingSelectionSystem`
- `BeginRetrainingSystem`
- `CompleteRetrainingSystem`
