using BuildingGen.Components;
using Newtonsoft.Json;
using UnityEngine;

namespace BuildingGen.Tools
{
    [RequireComponent(typeof(BuildingGenerator))]
    public class BuildingGeneratorStarter : MonoBehaviour
    {
        [SerializeField]
        private string _configName = "";

        private BuildingGenerator _buildingGenerator;

        void Awake()
        {
            _buildingGenerator = GetComponent<BuildingGenerator>();
        }

        void Start()
        {
            RunGeneration();
        }

        void RunGeneration()
        {
            TextAsset configContent = Resources.Load<TextAsset>(_configName);
            ConfigFile configFile = JsonConvert.DeserializeObject<ConfigFile>(configContent.text);
            _buildingGenerator.LoadConfig(configFile);
            _buildingGenerator.GenerateBuilding("b1");
        }
    }
}
