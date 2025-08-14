using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NN
{

    public class ScenePools : MonoBehaviour
    {
        public class OnRestoreDefaultsEventArgs
        {
            public GameObject GameObject;
        }
        public UnityEvent<OnRestoreDefaultsEventArgs> OnRestoreDefaults = new();

        private static ScenePools _instance;

        private Dictionary<GameObject, Stack<GameObject>> _pools = new();
        private Dictionary<GameObject, GameObject> _toPrefabMap = new();

        public static ScenePools Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindAnyObjectByType<ScenePools>();
                    if (!_instance)
                    {
                        _instance = new GameObject().AddComponent<ScenePools>();
                        _instance.name = "ScenePools_DefaultInstance";
                    }
                }
                return _instance;
            }
        }

        private Stack<GameObject> GetPool(GameObject prefab)
        {
            Stack<GameObject> pool;
            if (!_pools.TryGetValue(prefab, out pool))
            {
                pool = new();
                _pools.Add(prefab, pool);
            }
            return pool;
        }

        private GameObject GetObjectFromPool(Stack<GameObject> pool, GameObject prefab)
        {
            if (pool.Count > 0)
            {
                var go = pool.Pop();
                go.SetActive(true);
                go.transform.parent = null;
                return go;
            }
            else
            {
                var go = GameObject.Instantiate(prefab);
                _toPrefabMap.Add(go, prefab);
                go.name = prefab.name;
                return go;
            }
        }

        public GameObject Get(GameObject prefab)
        {
            var pool = GetPool(prefab);
            return GetObjectFromPool(pool, prefab);
        }

        public void Remove(GameObject go)
        {
            if (_toPrefabMap.TryGetValue(go, out var prefab))
            {
                var pool = GetPool(prefab);
                pool.Push(go);
                go.transform.parent = transform;
                RestoreToDefaults(go);
                go.SetActive(false);
            }
            else
            {
                Debug.LogError($"Missing prefab for object {go.name}({go.GetInstanceID()}) ");
                Destroy(go);
            }
        }

        private void RestoreToDefaults(GameObject go)
        {
            OnRestoreDefaults.Invoke(new() { GameObject = go });

            foreach (var joint in go.GetComponents<Joint>())
            {
                Destroy(joint);
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var ps = go.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Clear();
                ps.Play();
            }
        }
    }
}