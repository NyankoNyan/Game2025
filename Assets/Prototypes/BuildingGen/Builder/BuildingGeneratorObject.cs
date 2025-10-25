using UnityEngine;
using static BuildingGen.Components.BuildingGenerator;

namespace BuildingGen.Components
{
    /// <summary>
    /// Отдельный монобех для генератора зданий, который доспавнивает Joints на последующих кадрах
    /// </summary>
    public class BuildingGeneratorObject : MonoBehaviour
    {
        /// <summary>
        /// Позволяет перехватить инстанцирование блока. Используется для реализации пулов объектов или расширении компонент блоков.
        /// </summary>
        public OnBlockInstantiateDelegate OnBlockInstantiate
        {
            get => _buildingGenerator.OnBlockInstantiate;
            set => _buildingGenerator.OnBlockInstantiate = value;
        }

        /// <summary>
        /// Позволяет настроить блок после его создания. Передаёт также контекст настроек из которого можно извлечь кастомные параметры.
        /// </summary>
        public OnBlockSetupDelegate OnBlockSetup
        {
            get => _buildingGenerator.OnBlockSetup;
            set => _buildingGenerator.OnBlockSetup = value;
        }

        /// <summary>
        /// Событие настройки параметров по умолчанию перед генерацией здания.
        /// </summary>
        public OnDefaultContextSetupDelegate OnDefaultContextSetup
        {
            get => _buildingGenerator.OnDefaultContextSetup;
            set => _buildingGenerator.OnDefaultContextSetup = value;
        }

        private BuildingGenerator _buildingGenerator;

        /// <summary>
        /// Загружает конфигурационный файл.
        /// </summary>
        /// <param name="config">Конфигурационный файл.</param>
        public void LoadConfig(string fileName, ConfigFile config)
            => _buildingGenerator.LoadConfig(fileName, config);

        /// <summary>
        /// Генерирует здание по указанному идентификатору.
        /// </summary>
        /// <param name="buildingId">Идентификатор здания.</param>
        public Transform GenerateBuilding(string buildingId)
            => _buildingGenerator.GenerateBuilding(buildingId);

        private void Awake()
        {
            _buildingGenerator = new BuildingGenerator();
        }

        private void FixedUpdate()
        {
            _buildingGenerator.FlushJoints();
        }
    }
}