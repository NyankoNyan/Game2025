using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildingGen.Tools;
using NN;
using UnityEngine;
using UnityEngine.AI;

namespace BuildingGen.Components
{
    /// <summary>
    /// Генератор зданий на основе конфигурационного файла и пула объектов.
    /// </summary>
    public class BuildingGenerator : MonoBehaviour
    {
        public delegate GameObject OnBlockInstantiateDelegate(GameObject prefab);
        public delegate void OnBlockSetupDelegate(GameObject go, int health);
        public OnBlockInstantiateDelegate OnBlockInstantiate { get; set; }
        public OnBlockSetupDelegate OnBlockSetup { get; set; }

        private const int ColliderArraySize = 5; // Константа вместо волшебного числа

        private Dictionary<string, ConfigFile> _configs = new();

        /// <summary>
        /// Загружает конфигурационный файл.
        /// </summary>
        /// <param name="config">Конфигурационный файл.</param>
        public void LoadConfig(string fileName, ConfigFile config)
        {
            if (_configs.ContainsKey(fileName))
            {
                _configs[fileName] = config;
            }
            else
            {
                _configs.Add(fileName, config);
            }
        }

        /// <summary>
        /// Получает конфиг здания по его идентификатору.
        /// </summary>
        /// <param name="buildingId">Идентификатор здания.</param>
        /// <returns>Объект здания.</returns>
        /// <exception cref="Exception">Если здание не найдено.</exception>
        private (Building, ConfigFile) GetBuilding(string buildingId)
        {
            foreach (var config in _configs.Values)
            {
                foreach (var building in config.Buildings)
                {
                    if (building.Id == buildingId)
                    {
                        return (building, config);
                    }
                }
            }

            throw new Exception($"Не найдено здание с ID: {buildingId}");
        }

        /// <summary>
        /// Генерирует здание по указанному идентификатору.
        /// </summary>
        /// <param name="buildingId">Идентификатор здания.</param>
        public Transform GenerateBuilding(string buildingId)
        {
            var (building, config) = GetBuilding(buildingId);
            return GenerateBuilding(building, config);
        }

        /// <summary>
        /// Генерирует одно здание.
        /// </summary>
        /// <param name="building">Объект здания.</param>
        private Transform GenerateBuilding(Building building, ConfigFile config)
        {
            var context = new BuildingGenerationContext();
            context.Root = new GameObject($"[{building.Id}] {building.Name}").transform;
            context.EvaluationContext = new(null, config.Parameters);

            foreach (var section in building.Sections)
            {
                GenerateSection(section, building, context);
            }

            LinkSections(context);

            return context.Root.transform;
        }

        /// <summary>
        /// Генерирует секцию здания.
        /// </summary>
        /// <param name="section">Секция здания.</param>
        /// <param name="building">Объект здания.</param>
        /// <param name="context">Контекст генерации.</param>
        private void GenerateSection(Section section, Building building, BuildingGenerationContext context)
        {
            // Определяем позицию и поворот секции
            Vector3 sectionPosition = GetSectionPosition(section, context.EvaluationContext);
            Quaternion sectionRotation = GetSectionRotation(section, context.EvaluationContext);
            float hitboxPadding = section.HitboxPadding.ToFloat(context.EvaluationContext);

            // Получаем группу блоков
            BlockGroup blockGroup = GetBlockGroup(section.BlockGroupId);
            if (blockGroup == null)
            {
                Debug.LogError($"Группа блоков с ID '{section.BlockGroupId}' не найдена.");
                return;
            }

            // Получаем алгоритм генерации
            IGenerationAlgorithm algorithm = GetGenerationAlgorithm(section.GenerationAlgorithm);

            // Генерируем позиции блоков
            var (blocksInfo, links, potentialLinks) = algorithm.GeneratePositions(section, context.EvaluationContext);
            if (blocksInfo.Count == 0)
            {
                Debug.LogWarning($"Нет позиций для генерации в секции '{section.Id}'.");
                return;
            }

            GameObject[] blockGOs = new GameObject[blocksInfo.Count];
            for (int i = 0; i < blocksInfo.Count; i++)
            {
                BlockPointInfo info = blocksInfo[i];
                //TODO: Обработка исключений
                Block block = blockGroup.Blocks.Single(b => b.PointType == info.PointType);

                // Получаем блок из пула
                GameObject blockPrefab = BlockPrefabManager.Instance.GetPrefab(block.Id);
                GameObject blockInstance;
                if (OnBlockInstantiate != null)
                {
                    blockInstance = OnBlockInstantiate(blockPrefab);
                }
                else
                {
                    blockInstance = GameObject.Instantiate(blockPrefab);
                }
                blockInstance.transform.SetPositionAndRotation(sectionPosition + sectionRotation * info.Offset, sectionRotation * info.Rotation);
                blockInstance.transform.parent = context.Root;

                var collider = blockInstance.GetComponent<Collider>();
                if (!collider)
                {
                    var bxcol = blockInstance.AddComponent<BoxCollider>();
                    // bxcol.center = new Vector3(
                    //     (bxcol.size.x > Mathf.Epsilon) ? (bxcol.center.x - bxcol.center.x / bxcol.size.x * hitboxPadding) : 0,
                    //     (bxcol.size.y > Mathf.Epsilon) ? (bxcol.center.y - bxcol.center.y / bxcol.size.y * hitboxPadding) : 0,
                    //     (bxcol.size.z > Mathf.Epsilon) ? (bxcol.center.z - bxcol.center.z / bxcol.size.z * hitboxPadding) : 0
                    //     );
                    bxcol.size = new Vector3(
                        Mathf.Max(Mathf.Epsilon, bxcol.size.x - 2 * hitboxPadding),
                        Mathf.Max(Mathf.Epsilon, bxcol.size.y - 2 * hitboxPadding),
                        Mathf.Max(Mathf.Epsilon, bxcol.size.z - 2 * hitboxPadding)
                    );
                }

                var rb = blockInstance.GetComponent<Rigidbody>();
                if (!rb)
                {
                    // Добавляем Rigidbody
                    rb = blockInstance.AddComponent<Rigidbody>();
                }
                rb.mass = section.BlockMass.ToFloat(context.EvaluationContext);
                rb.isKinematic = section.IsStatic;

                // var gizmos = blockInstance.GetComponent<JointDebugDrawer>();
                // if (!gizmos)
                // {
                //     gizmos = blockInstance.AddComponent<JointDebugDrawer>();
                // }

                blockGOs[i] = blockInstance;
                context.AddBlock(blockInstance, context.SectionsCount, i);

                if (OnBlockSetup != null)
                {
                    OnBlockSetup(blockInstance, section.BlockHealth.ToInteger(context.EvaluationContext));
                }
            }

            // Подключаем блоки физическими соединениями
            foreach (var link in links)
            {
                ConnectBlocks(blockGOs[link.Id1], blockGOs[link.Id2], section, context.EvaluationContext);
            }

            context.GeneratedSections.Add(new GeneratedSectionInfo(
                id: context.SectionsCount,
                section: section,
                position: sectionPosition,
                rotation: sectionRotation,
                potentialLinks: potentialLinks,
                blocksGO: blockGOs.ToList()));
            context.SectionsCount++;
        }

        /// <summary>
        /// Связывает секции через физические соединения.
        /// </summary>
        /// <param name="context">Контекст генерации.</param>
        private void LinkSections(BuildingGenerationContext context)
        {
            HashSet<LinkProtector> linkProtectors = new();
            Collider[] colliders = new Collider[ColliderArraySize]; // Использование константы

            for (int sectionId = 0; sectionId < context.GeneratedSections.Count; sectionId++)
            {
                var sectionInfo = context.GeneratedSections[sectionId];
                foreach (var link in sectionInfo.PotentialLinks)
                {
                    GameObject sourceGO = null;
                    Vector3 searchPosition = sectionInfo.Position + sectionInfo.Rotation * link.LinkPosition;
                    int count = Physics.OverlapSphereNonAlloc(searchPosition, sectionInfo.Section.LinkSearchRadius.ToFloat(context.EvaluationContext), colliders);
                    float minDistance = float.PositiveInfinity;
                    GameObject nearestTarget = null;

                    for (int i = 0; i < count; i++)
                    {
                        Collider collider = colliders[i];
                        GameObject targetGO = collider.gameObject;

                        if (context.Blocks.TryGetValue(targetGO, out var targetBlockInfo))
                        {
                            if (targetBlockInfo.SectionId != sectionInfo.Id)
                            {
                                if (!sourceGO)
                                    sourceGO = sectionInfo.BlocksGO[link.BlockId];
                                float distance = (sourceGO.transform.position - targetGO.transform.position).sqrMagnitude;
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    nearestTarget = targetGO;
                                }


                            }
                        }
                    }

                    if (nearestTarget)
                    {
                        var linkProtector = new LinkProtector { GO1 = sourceGO, GO2 = nearestTarget };
                        if (!linkProtectors.Contains(linkProtector))
                        {
                            ConnectBlocks(sourceGO, nearestTarget, sectionInfo.Section, context.EvaluationContext);
                            linkProtectors.Add(linkProtector);
                        }
                    }
                }
            }
        }

        private Vector3 GetSectionPosition(Section section, EvaluationContext context)
        {
            Vector3 position = section.Position.Evaluate(context);
            return new Vector3(position.x, position.z, position.y);
        }

        /// <summary>
        /// Получает поворот секции, учитывая параметры.
        /// </summary>
        /// <param name="section">Секция здания.</param>
        /// <returns>Ориентация секции.</returns>
        private Quaternion GetSectionRotation(Section section, EvaluationContext context)
        {
            Vector3 rotation = section.Rotation.Evaluate(context);
            return Quaternion.Euler(rotation.x, rotation.z, rotation.y);
        }

        /// <summary>
        /// Получает группу блоков по ID.
        /// </summary>
        /// <param name="id">Идентификатор группы блоков.</param>
        /// <returns>Группа блоков.</returns>
        /// <exception cref="ArgumentException">Если группа не найдена.</exception>
        private BlockGroup GetBlockGroup(string id)
        {
            foreach (var config in _configs.Values)
            {
                foreach (var group in config.BlockGroups)
                {
                    if (group.Id == id)
                    {
                        return group;
                    }
                }
            }
            throw new ArgumentException($"Группа блоков с ID '{id}' не найдена.");
        }

        /// <summary>
        /// Получает алгоритм генерации по имени.
        /// </summary>
        /// <param name="name">Название алгоритма.</param>
        /// <returns>Алгоритм генерации.</returns>
        /// <exception cref="ArgumentException">Если алгоритм не поддерживается.</exception>
        private IGenerationAlgorithm GetGenerationAlgorithm(string name)
        {
            switch (name)
            {
                case "Grid":
                    return new GridGenerationAlgorithm();
                default:
                    throw new ArgumentException($"Алгоритм генерации '{name}' не поддерживается.");
            }
        }

        /// <summary>
        /// Подключает два блока физическим шарниром.
        /// </summary>
        /// <param name="a">Первый блок.</param>
        /// <param name="b">Второй блок.</param>
        /// <param name="section">Секция, к которой принадлежат блоки.</param>
        private void ConnectBlocks(GameObject a, GameObject b, Section section, EvaluationContext evalCtx)
        {
            FixedJoint joint = a.AddComponent<FixedJoint>();
            joint.connectedBody = b.GetComponent<Rigidbody>();

            float breakForce = section.BreakForce.ToFloat(evalCtx);
            float breakTorque = section.BreakTorque.ToFloat(evalCtx);
            joint.breakForce = (breakForce >= 0) ? breakForce : float.PositiveInfinity;
            joint.breakTorque = (breakTorque >= 0) ? breakTorque : float.PositiveInfinity;
        }

        private struct LinkProtector
        {
            public GameObject GO1, GO2;
        }
    }
}
