using System.Collections.Generic;

namespace BuildingGen.Components
{
    /// <summary>
    /// Интерфейс для алгоритмов генерации блоков.
    /// </summary>
    public interface IGenerationAlgorithm
    {
        (List<BlockPointInfo>, List<BlockLink>, List<PotentialLink>) GeneratePositions(Section section, EvaluationContext evalCtx);
    }
}
