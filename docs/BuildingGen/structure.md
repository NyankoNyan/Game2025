# Структура JSON для параметрической генерации зданий

## Общая структура
```json
{
  "version": "1.0",
  "parameters": {
    // Прямое значение (без дерева операций)
    "width": { "value": 8 },
    
    // Значение через дерево операций (20 - 5)
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
          "id": "sc1",
          "position": {
            // Вектор позиции с параметрами
            "x": { "value": 0 },
            "y": { "value": 0 },
            "z": { "value": 0 }
          },
          "rotation": {
            // Вектор вращения с параметрами
            "x": { "value": 0 },
            "y": { "value": 0 },
            "z": { "value": 0 }
          },
          "generationAlgorithm": "Grid",
          "generationSettingsGrid": {
              // Настройки для алгоритма Grid:
              // - size: размер сетки по осям X/Y/Z
              // - spacing: расстояние между элементами в сетке
              "size": {
                "x": { "value": 5 },
                "y": { "value": 5 },
                "z": { "value": 5 }
              },
              "spacing": {
                "x": { "value": 1 },
                "y": { "value": 1 },
                "z": { "value": 1 }
              }
          },
          "blockGroupId": "g1",
          "blockMass": { "value": 100 }
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
| `description` | `string` | Описание секции (что-то вроде комментария в конфиге) |
| `position` | `Vec3` | Позиция в пространстве |
| `rotation` | `Vec3` | Вращение |
| `generationAlgorithm` | `string` | Название алгоритма генерации |
| `generationSettingsGrid` | `GenerationSettingsGrid` | Дополнительные настройки для алгоритма Grid |
| `blockGroupId` | `string` | Идентификатор группы блоков |
| `blockMass` | `float` | Масса каждого блока секции |

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

### `GenerationSettingsGrid` (настройки для алгоритма Grid)
| Поле | Тип | Описание |
|------|-----|----------|
| `size` | `Size3` | Размер сетки по осям X/Y/Z (количество элементов) |
| `spacing` | `Vec3` | Расстояние между соседними элементами в сетке (шаг) |

> **Примечание:** Эти настройки применяются только если `generationAlgorithm` = `"Grid"`.

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

2. **Кастомные конвертеры**:
   - `ParameterConverter` обрабатывает типизированные параметры.
   - `ParameterDictionaryConverter` используется для словаря `parameters`.

---

## Возможные расширения
- Добавление новых типов `PointType`.
- Больше `generationAlgorithm` для новых алгоритмов.
- Поддержка сложных деревьев операций в `Parameter` с большим числом операций.
