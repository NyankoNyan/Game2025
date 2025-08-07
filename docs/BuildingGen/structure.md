# Структура JSON для параметрической генерации зданий

## Общая структура
```json
{
  "version": "1.0",
  "parameters": {
    "parameterName1": { "value": 10 },
    "parameterName2": {
      "operationTree": {
        "operation": "+",
        "operands": [
          { "value": 5 },
          {
            "operation": "*",
            "operands": [
              { "value": 2 },
              { "value": 3 }
            ]
          }
        ]
      }
    }
  },
  "blockGroups": [
    {
      "id": "g1",
      "blocks": [
        { "id": "blk1", "pointType": "Inside" }
      ]
    }
  ],
  "buildings": [
    {
      "id": "b1",
      "name": "MainBuilding",
      "description": "",
      "sections": [
        {
          "id": "s1",
          "description": "",
          "position": {
            "x": { "value": 0 },
            "y": { "value": 0 },
            "z": { "value": 0 }
          },
          "rotation": {
            "x": { "value": 0 },
            "y": { "value": 0 },
            "z": { "value": 0 }
          },
          "generationAlgorithm": "Grid",
          "blockGroupId": "g1"
        }
      ]
    }
  ]
}
```

---

## Основные компоненты

### `ConfigFile` (корневой объект)
| Поле | Тип | Описание |
|------|-----|----------|
| `version` | `string` | Версия конфигурационного файла |
| `parameters` | `Dictionary<string, Parameter>` | Словарь параметров генерации |
| `buildings` | `List<Building>` | Список описаний зданий |

---

### `Parameter`
Абстрактный класс с поддержкой обобщённых типов:
- **`Parameter<TValue>`** содержит поле `value` типа `TValue` (int/float).
- Поддерживает дерево операций (`operationTree`) для вычисления значения.

Пример с конкретным значением:
```json
{
  "height": { "value": 10 },
  "angle": { "value": 45.5 }
}
```

Пример с деревом операций:
```json
{
  "speed": {
    "operationTree": {
      "operation": "*",
      "operands": [
        { "value": 2 },
        { "value": 3.5 }
      ]
    }
  }
}
```

---

### `OperationNode` (узел дерева операций)
Класс для представления арифметических выражений:
- **`operation`** — строка с операцией (`+`, `-`, `*`, `/` и т.д.).
- **`operands`** — список дочерних узлов (операндов).

Пример дерева:
```json
{
  "operationTree": {
    "operation": "+",
    "operands": [
      { "value": 5 },
      {
        "operation": "*",
        "operands": [
          { "value": 2 },
          { "value": 3 }
        ]
      }
    ]
  }
}
```

---

### `Building`
| Поле | Тип | Описание |
|------|-----|----------|
| `id` | `string` | Уникальный идентификатор здания |
| `name` | `string` | Имя здания |
| `description` | `string` | Описание здания |
| `sections` | `List<Section>` | Список секций |

---

### `Section`
| Поле | Тип | Описание |
|------|-----|----------|
| `id` | `string` | Уникальный идентификатор секции |
| `description` | `string` | Описание секции |
| `position` | `Vec3` | Позиция в пространстве |
| `rotation` | `Vec3` | Вращение |
| `generationAlgorithm` | `string` | Название алгоритма генерации |
| `generationSettings` | `object` | Дополнительные настройки (опционально) |
| `blockGroupId` | `string` | Идентификатор группы блоков (опционально) |

---

### `Vec3` (вектор в 3D)
Каждая координата (x/y/z) — это `Parameter<float>`:
```json
{
  "position": {
    "x": { "value": 0 },
    "y": { "value": 0 },
    "z": { "value": 0 }
  }
}
```

---

### `BlockGroup`
| Поле | Тип | Описание |
|------|-----|----------|
| `id` | `string` | Уникальный идентификатор группы |
| `blocks` | `List<Block>` | Список блоков |

---

### `Block`
| Поле | Тип | Описание |
|------|-----|----------|
| `id` | `string` | Уникальный идентификатор блока |
| `pointType` | `PointType` | Тип точки (Inside/Boundary/Corner) |

---

## Особенности сериализации
1. **Параметры (`Parameter<TValue>`)**:
   - Для `int`: `"value": 10`
   - Для `float`: `"value": 5.5`
   - Поддержка дерева операций через `"operationTree"`:
     ```json
     {
       "operationTree": {
         "operation": "+",
         "operands": [
           { "value": 5 },
           { "value": 3 }
         ]
       }
     }
     ```

2. **Опциональные поля**:
   - `generationSettings` и `blockGroupId` могут отсутствовать.

3. **Кастомные конвертеры**:
   - `ParameterConverter` обрабатывает типизированные параметры.
   - `ParameterDictionaryConverter` используется для словаря `parameters`.

---

## Пример использования
```json
{
  "version": "1.0",
  "parameters": {
    "width": { "value": 8 },
    "height": { 
      "operationTree": {
        "operation": "-",
        "operands": [
          { "value": 20 },
          { "value": 5 }
        ]
      }
    }
  },
  "blockGroups": [
    {
      "id": "g1",
      "blocks": [
        { "id": "blk1", "pointType": "Inside" }
      ]
    }
  ],
  "buildings": [
    {
      "id": "b1",
      "name": "Residential",
      "sections": [
        {
          "id": "s1",
          "position": {
            "x": { "value": 0 },
            "y": { "value": 0 },
            "z": { "value": 0 }
          },
          "blockGroupId": "g1"
        }
      ]
    }
  ]
}
```

---

## Возможные расширения
- Добавление новых типов `PointType`.
- Расширение `generationSettings` для конкретных алгоритмов.
- Поддержка сложных деревьев операций в `Parameter`.
