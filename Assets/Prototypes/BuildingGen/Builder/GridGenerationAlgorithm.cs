using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Алгоритм генерации в виде сетки.
    /// </summary>
    public class GridGenerationAlgorithm : IGenerationAlgorithm
    {
        public (List<BlockPointInfo>, List<BlockLink>, List<PotentialLink>) GeneratePositions(Section section, EvaluationContext evalCtx)
        {
            GenerationSettingsGrid settings = section.GenerationSettingsGrid;
            if (settings == null)
            {
                throw new InvalidOperationException("Отсутствуют настройки для алгоритма 'Grid'.");
            }

            List<BlockPointInfo> positions = new List<BlockPointInfo>();
            List<BlockLink> links = new();
            List<PotentialLink> potentialLinks = new();

            Vector3Int size = settings.Size.Evaluate(evalCtx);
            Vector3 spacing = settings.Spacing.Evaluate(evalCtx);
            Vector2 midSize = new((size.x - 1) / 2f, (size.y - 1) / 2f);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        // В настройках Z - верх, X - вперед, Y - вправо, поэтому свапаем оси
                        Vector3 offset = new Vector3((x - midSize.x) * spacing.x, (z + .5f) * spacing.z, (y - midSize.y) * spacing.y);
                        float angle;
                        int fromBorderDistanceX = Mathf.FloorToInt(midSize.x) - Mathf.FloorToInt(Mathf.Abs(x - midSize.x));
                        int fromBorderDistanceY = Mathf.FloorToInt(midSize.y) - Mathf.FloorToInt(Mathf.Abs(y - midSize.y));
                        int depth = Mathf.Min(fromBorderDistanceX, fromBorderDistanceY);
                        // Сравнение расстояния краёв. Если больше 0, то ближе к краю по оси Y
                        int xyDistance = fromBorderDistanceX - fromBorderDistanceY;
                        // Поворот блока зависит от того, насколько далеко он находится от каждой из сторон.
                        // Для угловых блоков поворот берётся от блока, который от них слева
                        if (x > midSize.x)
                        {
                            if (y > midSize.y)
                            {
                                if (xyDistance > 0)
                                    angle = 0;
                                else
                                    angle = 90;
                            }
                            else
                            {
                                if (xyDistance >= 0)
                                    angle = 180;
                                else
                                    angle = 90;
                            }
                        }
                        else
                        {
                            if (y > midSize.y)
                            {
                                if (xyDistance >= 0)
                                    angle = 0;
                                else
                                    angle = 270;
                            }
                            else
                            {
                                if (xyDistance > 0)
                                    angle = 180;
                                else
                                    angle = 270;
                            }
                        }
                        Quaternion rotation = Quaternion.Euler(0, angle, 0);

                        PointType pointType;
                        if (depth > 0)
                        {
                            pointType = PointType.Inside;
                        }
                        else
                        {
                            if (xyDistance == 0)
                            {
                                pointType = PointType.OuterCorner;
                            }
                            else
                            {
                                pointType = PointType.Boundary;
                            }
                        }

                        positions.Add(new BlockPointInfo()
                        {
                            Offset = offset,
                            Rotation = rotation,
                            PointType = pointType,
                            Depth = depth
                        });

                        int currentId = GetId(x, y, z, size);
                        // Добавляем информацию о связях внутри секции
                        if (x > 0)
                            links.Add(new() { Id1 = currentId, Id2 = GetId(x - 1, y, z, size) });
                        if (y > 0)
                            links.Add(new() { Id1 = currentId, Id2 = GetId(x, y - 1, z, size) });
                        if (z > 0)
                            links.Add(new() { Id1 = currentId, Id2 = GetId(x, y, z - 1, size) });

                        // Добавялем информацию о потенциальных связях на краях секции
                        if (x == 0)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset - Vector3.right * spacing.x });
                        if (x == size.x - 1)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset + Vector3.right * spacing.x });
                        if (y == 0)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset - Vector3.forward * spacing.y });
                        if (y == size.y - 1)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset + Vector3.forward * spacing.y });
                        if (z == 0)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset - Vector3.up * spacing.z });
                        if (z == size.z - 1)
                            potentialLinks.Add(new() { BlockId = currentId, LinkPosition = offset + Vector3.up * spacing.z });
                    }
                }
            }

            return (positions, links, potentialLinks);
        }

        private int GetId(int x, int y, int z, Vector3Int size)
        {
            return (x * size.y + y) * size.z + z;
        }
    }
}
