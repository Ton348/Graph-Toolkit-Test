README — Прототип GTA-like business game на Unity
Цель проекта
Создать прототип игры в стиле GTA + бизнес-симулятор, где игрок:
перемещается по открытому городу
покупает и арендует здания
открывает бизнесы (кафе, рестораны, автомойки и т.д.)
получает задания и бизнес-проблемы
принимает решения с разными последствиями
взаимодействует с NPC, зданиями и точками на карте
Главная цель прототипа — проверить core loop, архитектуру и пригодность Graph Toolkit для настройки логики через графы.
Главная идея
Это не обычная квестовая система, а система бизнес-кейсов.
Каждое дело в игре — это проблема или задача бизнеса, например:
купить здание
улучшить здание
оплатить налоги
решить вопрос с лицензией
организовать поставки
разобраться с рэкетирами
У каждого дела может быть:
несколько способов решения
цена
риск
шанс успеха
последствия
Основной игровой цикл
Игрок перемещается по городу.
Получает задание от NPC-менеджера.
Покупает или арендует здание.
Улучшает здание.
Открывает бизнес.
Получает новые бизнес-кейсы.
Едет в нужные точки на карте.
Взаимодействует с NPC или триггерами.
Получает результат.
Мир и бизнес реагируют на последствия.
Ключевые системы
1. Player System
Игрок имеет профиль со статами:
Money
Bargaining
Speech
Speed

Damage

Health

Игрок должен уметь:

перемещаться по городу

взаимодействовать с объектами по кнопке E

получать квесты

покупать здания

улучшать здания

2. Building System

Каждое здание имеет настраиваемые параметры:

purchaseCost

rentPerMonth

incomePerDay

expensesPerDay

netProfit

upgradeCost

upgradeIncomeBonus

Здание может:

быть доступным для покупки

быть доступным для аренды

быть улучшенным

иметь менеджера

генерировать задания

3. Quest / Case System

Задания и бизнес-кейсы — это основа игры.

Примеры:

Купи здание

Улучши здание

Принеси товар

Оплати налоги

Сходи к инспектору

У каждого задания есть:

id

title

description

reward

target

status

4. Interaction System

Игрок взаимодействует с миром по кнопке E.

Типы интерактивных объектов:

NPCManager

BuildingInteractable

QuestTarget

5. Runtime State

Во время игры должно храниться текущее состояние:

состояние игрока

список зданий

список активных заданий

деньги

прогресс

Это живые данные, которые меняются во время игры.

6. Event Bus

Системы не должны быть жестко связаны друг с другом.

Они общаются через события, например:

BuildingPurchased

BuildingUpgraded

QuestAccepted

QuestCompleted

PlayerMoneyChanged

ManagerInteracted

7. Graph System (Graph Toolkit)

Graph Toolkit используется как база для создания своего редактора графов.

Через графы в будущем будет настраиваться логика заданий и кейсов.

Пример графа:

Start→ Give Quest (Buy Building)→ Check Building Owned→ Give Quest (Upgrade Building)→ Check Building Upgraded→ Complete

Важно: Graph Toolkit отвечает за editor-инструмент. Runtime-исполнение логики делается отдельно.

Технологический стек

Unity 6

Graph Toolkit

ScriptableObject

C# runtime classes

CharacterController

Принципы разработки

Сначала прототип, потом расширение.

Простота важнее идеальной архитектуры.

Данные должны быть отделены от runtime состояния.

Логика должна быть data-driven.

Минимум лишних зависимостей.

Не делать сложные системы раньше времени.

Структура проекта

Assets/
  Scenes/
    Prototype_City.unity

  Scripts/
    Core/
      EventBus/
      Runtime/
    Gameplay/
      Player/
      Buildings/
      Quests/
      Interaction/
    UI/
    Tools/
      Graph/

  Data/
    Definitions/
      Player/
      Buildings/
      Quests/
    Graphs/

  Prefabs/
    Player/
    NPC/
    Buildings/

Архитектурные слои

1. Definitions (ScriptableObject)

Папка:

Data/Definitions/

Хранят шаблоны данных и не должны содержать живое состояние.

Примеры:

PlayerProfileDefinition

BuildingDefinition

QuestDefinition

BuildingUpgradeDefinition

2. Runtime State

Папка:

Scripts/Core/Runtime/

Хранят текущее состояние игры.

Примеры:

PlayerProfileState

BuildingState

QuestState

GameRuntimeState

3. Gameplay

Папка:

Scripts/Gameplay/

Содержит игровую логику:

player movement

interaction

buying buildings

quest flow

4. Core

Папка:

Scripts/Core/

Содержит базовые системы:

EventBus

RuntimeState

5. Tools / Graph

Папка:

Scripts/Tools/Graph/

Содержит редактор графов и runtime runner для графов.

Ключевые классы

Player System

Definition

PlayerProfileDefinition : ScriptableObject

Поля:

startMoney

baseBargaining

baseSpeech

baseSpeed

baseDamage

baseHealth

Runtime

PlayerProfileState

Поля:

Money

Bargaining

Speech

Speed

Damage

Health

Gameplay

PlayerMovement

PlayerInteractor

PlayerService

FollowCamera

Building System

Definition

BuildingDefinition : ScriptableObject

Поля:

purchaseCost

rentPerMonth

incomePerDay

expensesPerDay

netProfit

upgradeCost

upgradeIncomeBonus

Runtime

BuildingState

Поля:

isOwned

level

currentIncome

currentExpenses

Gameplay

BuildingInteractable

BuildingService

Quest / Case System

Definition

QuestDefinition : ScriptableObject

Поля:

id

title

description

rewardMoney

Runtime

QuestState

Поля:

definition

status

Gameplay

QuestService

NPCManager

Interaction System

Base

Interactable

Содержит:

interactionDistance

virtual Interact()

базовую проверку дистанции

Реализации

NPCManager

BuildingInteractable

Игрок

PlayerInteractor

Делает:

ищет ближайший Interactable

вызывает Interact() по E

Runtime State

GameRuntimeState

Содержит:

PlayerProfileState

List

List

Это центральная точка текущего состояния игры.

Event Bus

Папка:

Scripts/Core/EventBus/

Базовый класс:

EventBus

События:

BuildingPurchasedEvent

BuildingUpgradedEvent

QuestAcceptedEvent

QuestCompletedEvent

PlayerMoneyChangedEvent

ManagerInteractedEvent

Принцип:

системы не должны быть связаны напрямую

общение идет через события

Graph System

Папка:

Scripts/Tools/Graph/

Graph Asset

BusinessQuestGraph

Nodes

StartNode

GiveQuestNode

CheckBuildingOwnedNode

CheckBuildingUpgradedNode

CompleteNode

Runtime

GraphRunner

Функции:

читает graph asset

двигается по нодам

вызывает сервисы

использует RuntimeState и EventBus

Поток данных

Покупка здания

Игрок→ нажал E→ BuildingInteractable→ BuildingService→ GameRuntimeState обновился→ EventBus отправил событие BuildingPurchased

Получение квеста

Игрок→ подошел к NPCManager→ нажал E→ QuestService выдал задание→ QuestState добавлен в GameRuntimeState→ UI обновился

Правила проекта

1. Не смешивать данные и состояние

ScriptableObject = шаблоны

Runtime State = живое состояние

2. Не хранить игровую логику в MonoBehaviour

MonoBehaviour должен отвечать только за:

сцену

визуал

триггеры

взаимодействие

Логика должна быть в service-классах.

3. Не делать прямых зависимостей между системами

Использовать EventBus.

4. Graph Toolkit использовать только как основу editor-инструмента

Не пытаться засунуть в него весь runtime.

Этапы разработки

Этап 1 — База сцены

Сделать:

сцену с городом

игрока

CharacterController

камеру

интерактивные объекты

Минимальный набор скриптов

PlayerMovement.cs

FollowCamera.cs

Interactable.cs

NPCManager.cs

BuildingInteractable.cs

PlayerInteractor.cs

Этап 2 — Данные и профиль игрока

Сделать:

PlayerProfileDefinition

PlayerProfileState

BuildingDefinition

BuildingState

QuestDefinition

QuestState

GameRuntimeState

Этап 3 — Event Bus и сервисы

Сделать:

EventBus

PlayerService

BuildingService

QuestService

Этап 4 — Первый loop

Реализовать:

NPC выдает квест Купи здание

Игрок покупает здание

После покупки выдается квест Улучши здание

Игрок улучшает здание

Этап 5 — Graph Toolkit skeleton

Сделать:

BusinessQuestGraph

базовые node-классы

editor window

graph asset

Этап 6 — Runtime graph execution

Сделать:

GraphRunner

чтение графа

выполнение логики графа

привязку графа к NPCManager

MVP: обязательный минимум

Для первого играбельного прототипа достаточно:

управляемый игрок

камера

NPC менеджер

одно здание

покупка здания

улучшение здания

деньги игрока

одно-два задания

EventBus

RuntimeState

базовый graph skeleton

Что пока НЕ делаем

На первом прототипе не делаем:

полноценную боевку

сложную диалоговую систему

сложный UI блокнота

сохранения

генерацию случайных событий

много типов бизнесов

сложную экономику на недели и месяцы

Ближайший практический шаг

Сейчас ближайшая цель:

оживить игрока

сделать follow camera

добавить interaction по E

сделать NPCManager и BuildingInteractable

получить первый живой loop в сцене

После этого переходить к Definitions, RuntimeState и EventBus.

Промт для Cursor — первый этап

Сделай минимальный набор скриптов для Unity 6 прототипа.

ВАЖНО:
- НЕ используй новую Input System
- НЕ используй Rigidbody
- НЕ добавляй лишнюю архитектуру
- НЕ усложняй код
- Используй только CharacterController и Input.GetAxis / Input.GetKey

Нужно создать РОВНО 5 скриптов:

1. PlayerMovement.cs
Требования:
- движение на WASD через CharacterController
- бег при удержании LeftShift
- гравитация
- без прыжка
- скорость и runSpeed как публичные поля

2. FollowCamera.cs
Требования:
- камера следует за игроком
- использовать offset (Vector3)
- плавное движение через Lerp
- без вращения мышью

3. Interactable.cs
Требования:
- базовый класс
- поле interactionDistance
- метод Interact() виртуальный
- проверка дистанции до игрока

4. NPCManager.cs
Требования:
- наследуется от Interactable
- при Interact() → Debug.Log("Выдан квест: Купи здание")

5. BuildingInteractable.cs
Требования:
- наследуется от Interactable
- при Interact() → Debug.Log("Попытка купить здание")

Дополнительно:
- добавь отдельный простой PlayerInteractor.cs:
  - при нажатии E ищет ближайший Interactable
  - вызывает Interact()

В конце:
- объясни на какие объекты сцены повесить каждый скрипт
- не добавляй ничего лишнего

Итог

Этот проект строится по принципу:

Definitions + Runtime State + EventBus + Services + Graph Toolkit Editor

Сначала мы делаем рабочий прототип, потом переносим логику в графы и расширяем систему бизнес-кейсов.

