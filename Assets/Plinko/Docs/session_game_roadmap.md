# Session Game Roadmap

## Current Superseding Update

If older sections below conflict with this block, this block is authoritative.

Current battle stack:

- `StandardBattle` exists and remains supported, but is not the active product focus.
- `DefenceBattle` exists and remains supported, but is no longer the sole active battle focus.
- `PowerLineBattle` is now the active implementation focus.

Current priority order:

1. Finish `PowerLineBattle` runtime + UI + result flow.
2. Finish `PowerLineBattle` authored content and scene hookup.
3. Convert `BattleResult` from temporary IMGUI to scene-authored UI.
4. Only after that, return to `DefenceBattle` and `StandardBattle` polish if product still needs it.

What is already implemented for `PowerLineBattle`:

- separate `LevelType.PowerLineBattle`
- separate runtime state/service models
- separate ECS requests for init / draw / reroll / deploy
- separate real-time tick resolver
- separate UI payload
- separate primary window and scene-authored controller layer

What still needs to be verified/finalized for `PowerLineBattle`:

- scene hookup in Unity
- authored level data/assets
- UX polish for realtime hand/board feedback
- result popup presentation
- full gameplay verification on real content

## 1. Назначение документа

Этот документ фиксирует не “идеальный план с нуля”, а актуальный roadmap проекта с учётом уже реализованного кода и всех решений, принятых в ходе работы.

Документ нужен для ответа на три вопроса:
- что уже сделано;
- что сделано частично;
- какой следующий приоритет.

---

## 2. Что уже реализовано

### 2.1 Базовая архитектура и инфраструктура

Реализовано:
- `GameBootstrapper`;
- `GameServicesInstaller`;
- `EcsCompositionRoot`;
- data/runtime/view data models;
- requests/events/indexes;
- meta progression;
- menu/location shell;
- primary window manager;
- общий UI animation manager.

### 2.2 Runtime core loop

Реализовано:
- `StartNewRun`;
- `ContinueRun`;
- meta unlocks и location progression;
- `LoadLevel`;
- `AdvanceToNextLevel`;
- `ReturnToMenu`;
- `Result flow`;
- отдельные `StandardBattle` и `DefenceBattle` level types;
- `LevelType.StandardBattle`;
- authored/runtime/saveload/UI контракты для `LevelType.DefenceBattle`.

### 2.3 Purchase

Реализовано:
- unit shop generation;
- reroll;
- buy;
- start purchased training;
- shared plinko playback;
- complete purchased training;
- save/load для фазы;
- scene-authored окно Purchase.

### 2.4 Retraining

Реализовано:
- новая batch-shop модель вместо ручного выбора;
- batch generation из owned units;
- batch reroll;
- batch buy по сумме `ShopPrice`;
- retraining через общий plinko pipeline;
- исключение уже upgraded units из текущего retraining level;
- save/load для runtime;
- scene-authored окно Retraining.

### 2.5 Field Upgrade

Реализовано:
- pin shop generation;
- reroll;
- buy pin;
- pending pin;
- выбор board pin;
- replace board pin;
- save/load board state;
- scene-authored окно Field Upgrade.

### 2.6 Save / Load

Реализовано:
- checkpoint на старте уровня;
- checkpoint на старте хода игрока в battle;
- отдельный `meta save`;
- восстановление run;
- fallback при повреждённом save: перезапуск текущей локации.

### 2.7 UI

Реализовано:
- scene-authored menu;
- popup выбора локации;
- scene-authored окна:
  - `Purchase`;
  - `Retraining`;
  - `FieldUpgrade`;
- `StandardBattle` shell;
- `DefenceBattle` shell;
  - `BaseDefense` battle shell.

Частично реализовано:
- `BattleResult` всё ещё остаётся временным IMGUI-окном.

---

## 3. Что реализовано частично

### 3.1 Standard Battle

`LevelType.StandardBattle` существует и работает как runtime, но сейчас не является активным продуктовым фокусом.

Что есть:
- runtime turn loop;
- wave selection;
- battle resolution;
- result routing;
- battle HUD payload.

Что не является текущим приоритетом:
- полное доведение Standard battle presentation;
- production polish именно этого режима.

### 3.2 BaseDefense

Для `LevelType.DefenceBattle` уже есть:
- authored data structures;
- runtime state;
- отдельный resolver;
- save/load;
- view payload;
- scene-authored battle shell.

Но ещё не завершены:
- production-level playback;
- полный result presentation;
- полный UI polish;
- дополнительный контент для authored уровней и волн.

---

## 4. Текущий активный фокус

Текущий основной фокус проекта:
1. `BaseDefense`;
2. production battle UI и presentation для него;
3. замыкание полного flow уровня `BaseDefense`;
4. затем возврат к хвостам `Standard Battle` только если это действительно нужно продукту.

Если нет прямого указания, не нужно продолжать углублять `Standard Battle` вместо `BaseDefense`.

---

## 5. Актуальный порядок следующих работ

### Шаг A. Довести BaseDefense до полноценного игрового режима

Нужно:
- довести scene-authored окно `BaseDefense` по фактической сборке сцены;
- подключить полноценный gameplay feedback;
- довести deploy flow по клеткам;
- довести блокировку UI во время autoplay;
- довести возврат карт в колоду между ходами;
- довести popup колоды;
- довести floating texts и mana feedback.

### Шаг B. Довести BaseDefense result flow

Нужно:
- перевести `BattleResult` из временного IMGUI в scene-authored production popup;
- корректно показывать victory/defeat для `BaseDefense`;
- связать result popup с progression и переходом на следующий уровень/в меню.

### Шаг C. Довести BaseDefense visuals/playback

Нужно:
- визуализировать timeline/автобой;
- анимировать спавн врагов волны;
- анимировать действия юнитов;
- анимировать урон, смерть и события хода;
- довести presentation до читаемого состояния.

### Шаг D. Расширить authored content

Нужно:
- завести реальные authored `BaseDefense` уровни;
- заполнить battle data, волны, grid content, visuals;
- проверить progression локаций на реальном контенте.

### Шаг E. Вернуться к хвостам Standard Battle только при необходимости

Если продукт всё ещё требует обычный base-vs-base battle:
- доделать standard battle presentation;
- синхронизировать его с текущими UI-правилами;
- довести его result window и polish.

Если продукт уходит полностью в `BaseDefense`, этот шаг можно отложить.

---

## 6. Технический backlog

### Высокий приоритет

- перевести `BattleResultScreenController` на scene-authored UI;
- убрать оставшиеся временные battle presentation-элементы;
- довести `BaseDefense` hand/board feedback;
- проверить save/load на длинных сериях ходов `BaseDefense`.

### Средний приоритет

- довести `BaseDefense` authored content;
- вычистить неиспользуемые временные battle-ветки;
- проверить согласованность `UiWindowManager` и popup flow.

### Низкий приоритет

- polishing стандартного battle режима;
- расширение визуальных эффектов вне текущего gameplay-приоритета;
- косметическая чистка, не влияющая на flow.

---

## 7. Что считается завершением текущего этапа

Текущий этап считается завершённым, когда выполнено всё ниже:
- игрок может пройти локацию через menu/location flow;
- `Purchase`, `Retraining`, `FieldUpgrade` работают через production-style scene-authored окна;
- `BaseDefense` playable от входа на уровень до result popup;
- save/load корректно работает на checkpoint-модели;
- результат уровня корректно ведёт дальше по progression;
- UI и runtime соответствуют текущим документам.

---

## 8. Правило изменения roadmap

Roadmap можно менять только если изменилось одно из двух:
- продуктовая модель;
- архитектурный контракт.

Если решение влияет на phase model, battle mode, save/load или UI contract, сначала нужно обновить документы, и только потом писать код.
