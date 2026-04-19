# Codex Rules

## Current Superseding Update

If battle rules changed, do not reintroduce a shared oversized battle controller or resolver.

Current expectation:

- `StandardBattle`, `DefenceBattle`, and `PowerLineBattle` are separate `LevelType` values.
- If a new battle-like mode has materially different win conditions, mana flow, save/load behavior, board model, or UI contract, implement it as a new authored level type instead of another switch branch inside an existing battle screen/resolver.

## 1. Роль

Ты работаешь в `Assets/Plinko` внутри Unity-проекта.

Проект архитектурно чувствительный. Здесь нельзя писать код как для быстрого прототипа. Любое изменение должно сохранять:
- runtime consistency;
- save/load consistency;
- scene-authored UI contract;
- принятые продуктовые правила.

---

## 2. Главный приоритет

Всегда приоритетен следующий порядок:
1. корректный runtime flow;
2. корректный save/load;
3. соответствие документам;
4. UI sync;
5. visuals / animation polish.

Не делай visual-first решения, если они ломают или подменяют runtime.

---

## 3. Обязательные документы

Перед изменениями ориентируйся на:
- `Assets/Plinko/Docs/architecture_guide.md`
- `Assets/Plinko/Docs/session_game_roadmap.md`

Если реализация больше не соответствует документам, сначала обнови документы, потом код.

---

## 4. Что является source of truth

Source of truth:
- `ECS runtime` для активной игры;
- `ScriptableObject` для authored контента;
- `Save DTO` только для persistence;
- `Meta save` только для мета-прогресса.

UI не является source of truth.

---

## 5. ECS-правила

### 5.1 Gameplay logic только в ECS

Gameplay logic должна жить в systems, services и runtime models, а не в MonoBehaviour view-классе.

### 5.2 Requests и Events

Используй последовательно:
- `Request` для команды выполнить действие;
- `Event` для сообщения, что действие уже произошло.

### 5.3 Phase guards

Если система работает только в конкретной фазе, она обязана проверять фазу до мутации состояния.

### 5.4 Wiring обязателен

Новая система не считается реализованной, если она не подключена в `EcsCompositionRoot`.

---

## 6. UI-правила

### 6.1 Scene-authored only

UI создаётся в сценах и префабах вручную.

Код должен:
- писать только scripts;
- работать через `SerializeField`;
- предполагать, что пользователь сам собирает layout и префабы;
- не создавать иерархию UI runtime-ом.

### 6.2 Не использовать GetComponent во View

Во view-слое не использовать `GetComponent`.

Если ссылка нужна, она должна быть прокинута через `SerializeField`.

### 6.3 Не писать validator-layer для UI

Не нужно добавлять кастомные валидаторы ради проверки всех UI-ссылок.

Принятый подход:
- код пишется чисто и без лишних defensive checks;
- если обязательная ссылка не прокинута, это обычная Unity-ошибка сборки сцены/префаба.

### 6.4 Не создавать UI, если пользователь не просил

Не добавляй новые UI-элементы, окна или поля самовольно.

Если layout-решение не было согласовано, сначала уточни его.

### 6.5 Primary windows vs popups

У проекта есть правило:
- одновременно открыт только один primary window;
- popup живёт отдельно и не ломает это правило.

Следуй `UiWindowManager` и не дублируй отдельную систему показа окон без необходимости.

---

## 7. Анимации

### 7.1 Через общий manager

Для UI-анимаций используй общий `UiAnimationManager`.

Не разбрасывай локальные ad-hoc DOTween-цепочки по item/view-классам без причины.

### 7.2 Presentation не должен становиться gameplay

Анимации не должны:
- хранить игровой state;
- решать gameplay outcome;
- блокировать runtime flow неявным образом.

---

## 8. Данные и визуалы

### 8.1 Sprite-based visuals

В authored data хранятся sprite/animation-set ссылки, а не готовые gameplay-prefabs.

### 8.2 Runtime entity != config asset

Не путай:
- тип юнита;
- owned unit;
- тип пина;
- установленный pin на поле;
- authored enemy spawn;
- runtime enemy unit.

Это особенно важно для:
- retraining;
- battle deploy;
- save/load.

---

## 9. Save / Load

Текущая политика:
- checkpoint на старте уровня;
- checkpoint на старте хода игрока в battle;
- broken save => безопасный перезапуск текущей локации.

Не вводи новый save contract локально внутри UI или presentation.

---

## 10. Текущий продуктовый фокус

Текущий активный боевой фокус проекта — `LevelType.DefenceBattle`.

`LevelType.StandardBattle` существует, но не является главным направлением, если пользователь не попросил вернуться к нему явно.

Значит:
- новые battle-решения в первую очередь должны учитывать `BaseDefense`;
- не нужно углублять `Standard`, если задача реально относится к `BaseDefense`.

---

## 11. Правила по изменениям

### 11.1 Не делать широкие рефакторинги без причины

Если задача не требует большого рефакторинга, не делай его.

### 11.2 Не дублировать пайплайны

Если pipeline уже существует, переиспользуй его.

Пример:
- purchase и retraining используют общий plinko training pipeline.

### 11.3 Не придумывать продуктовые решения молча

Если отсутствует продуктовая спецификация, а решение повлияет на runtime contract, сначала зафиксируй assumption или спроси пользователя.

---

## 12. Как писать код в этом проекте

Предпочтительно:
- небольшие целевые изменения;
- понятные имена;
- один класс — одна чёткая ответственность;
- минимальная магия;
- прозрачный runtime flow.

Нежелательно:
- размазанные helper-абстракции;
- скрытые побочные эффекты;
- defensive noise вместо ясной логики;
- несогласованные временные костыли.

---

## 13. Что писать в итоговом сообщении

После выполнения задачи кратко сообщай:
- что изменилось;
- что теперь работает по игровому флоу;
- какие ограничения или хвосты остались.

Если задача касалась UI:
- отдельно укажи, что нужно прокинуть в инспекторе.
