using System.Collections.Generic;
using UnityEngine;

namespace NN
{
    public class GPUInstancePools : MonoBehaviour
    {
        private static GPUInstancePools _instance;

        /// <summary>
        /// Структура ключа для хранения данных о меше, submesh и материале.
        /// </summary>
        private struct InstanceKey
        {
            public Mesh Mesh;
            public int SubMeshIndex;
            public Material Material;

            public InstanceKey(Mesh mesh, int subMeshIndex, Material material)
            {
                Mesh = mesh;
                SubMeshIndex = subMeshIndex;
                Material = material;
            }

            public override bool Equals(object obj)
            {
                if (obj is InstanceKey other)
                {
                    return Mesh == other.Mesh &&
                           SubMeshIndex == other.SubMeshIndex &&
                           Material == other.Material;
                }
                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = (hash << 5) - hash + Mesh.GetHashCode();
                    hash = (hash << 5) - hash + SubMeshIndex.GetHashCode();
                    hash = (hash << 5) - hash + Material.GetHashCode();
                    return hash;
                }
            }
        }

        private Dictionary<InstanceKey, List<Matrix4x4>> _instanceData = new();
        private Dictionary<GameObject, (Mesh mesh, Material[] materials, Matrix4x4 baseMatrix)> _prefabCache = new();

        public static GPUInstancePools Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindAnyObjectByType<GPUInstancePools>();
                    if (!_instance)
                    {
                        _instance = new GameObject().AddComponent<GPUInstancePools>();
                        _instance.name = "GPUInstancePools_DefaultInstance";
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Добавить объект в пул для GPU Instancing.
        /// </summary>
        /// <param name="prefab">Префаб объекта.</param>
        /// <param name="matrix">Матрица трансформации объекта.</param>
        public void AddInstance(GameObject prefab, Matrix4x4 matrix)
        {
            if (!_prefabCache.TryGetValue( prefab, out var cachedData ))
            {
                // Извлекаем данные о меше и материалах из префаба
                var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
                var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                var meshTransform = meshFilter.GetComponent<Transform>();

                if (meshFilter == null || meshRenderer == null)
                {
                    Debug.LogError( $"Prefab {prefab.name} должен содержать MeshFilter и MeshRenderer." );
                    return;
                }

                cachedData = (meshFilter.sharedMesh, meshRenderer.sharedMaterials, meshTransform.localToWorldMatrix);
                _prefabCache[prefab] = cachedData;
            }

            var mesh = cachedData.mesh;
            var materials = cachedData.materials;
            var baseMatrix = cachedData.baseMatrix;

            for (int subMeshIndex = 0; subMeshIndex < materials.Length; subMeshIndex++)
            {
                var material = materials[subMeshIndex];
                var key = new InstanceKey( mesh, subMeshIndex, material );

                if (!_instanceData.TryGetValue( key, out var matrices ))
                {
                    matrices = new List<Matrix4x4>();
                    _instanceData[key] = matrices;
                }

                matrices.Add( matrix * baseMatrix );
            }
        }

        public void AddInstance(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS( position, rotation, scale );
            AddInstance( prefab, matrix );
        }

        public void AddInstance(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            AddInstance( prefab, position, rotation, Vector3.one );
        }

        /// <summary>
        /// Удалить объект из пула.
        /// </summary>
        /// <param name="prefab">Префаб объекта.</param>
        /// <param name="matrix">Матрица трансформации объекта.</param>
        public void RemoveInstance(GameObject prefab, Matrix4x4 matrix)
        {
            if (!_prefabCache.TryGetValue( prefab, out var cachedData ))
                return;

            var mesh = cachedData.mesh;
            var materials = cachedData.materials;
            var baseMatrix = cachedData.baseMatrix;

            for (int subMeshIndex = 0; subMeshIndex < materials.Length; subMeshIndex++)
            {
                var material = materials[subMeshIndex];
                var key = new InstanceKey( mesh, subMeshIndex, material );

                if (_instanceData.TryGetValue( key, out var matrices ))
                {
                    matrices.Remove( baseMatrix * matrix );

                    if (matrices.Count == 0)
                    {
                        _instanceData.Remove( key );
                    }
                }
            }
        }

        private void Update()
        {
            foreach (var entry in _instanceData)
            {
                var key = entry.Key;
                var matrices = entry.Value;

                // Разбиваем на батчи, так как Graphics.DrawMeshInstanced поддерживает максимум 1023 экземпляра за раз
                for (int i = 0; i < matrices.Count; i += 1023)
                {
                    int count = Mathf.Min( 1023, matrices.Count - i );
                    Graphics.DrawMeshInstanced( key.Mesh, key.SubMeshIndex, key.Material, matrices.GetRange( i, count ) );
                }
            }
        }
    }
}