using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NN
{
    public interface IPoolManager
    {
        GameObject GetInstance(GameObject prefab);

        void ReleaseInstance(GameObject instance);

        void ReleaseInstance(GameObject instance, float delay);
    }

    public class PoolManager : MonoBehaviour, IPoolManager
    {
        private static PoolManager _instance;

        public static IPoolManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindFirstObjectByType<PoolManager>();
                    if (!_instance)
                    {
                        Debug.Log( "Missing PoolManager in scene" );
                    }
                }
                return _instance;
            }
        }

        [Serializable]
        public struct PredefinePool
        {
            public GameObject prefab;
            public int poolSize;
        }

        private class Pool
        {
            public Stack<GameObject> Stack;
            public int Counter = 0;

            public Pool(int size)
            {
                Stack = new( size );
            }
        }

        [SerializeField] private int _defaultPoolSize = 5;

        [SerializeField]
        private List<PredefinePool> _predefinePools;

        private Dictionary<GameObject, Pool> _pools = new();
        private Dictionary<GameObject, GameObject> _originalPrefabs = new();

        public GameObject GetInstance(GameObject prefab)
        {
            Pool pool;
            if (!_pools.TryGetValue( prefab, out pool ))
            {
                pool = AddPool( prefab, _defaultPoolSize );
                Debug.LogWarning( $"Missing default pool for prefab {prefab.name}" );
            }
            GameObject instance;
            if (pool.Stack.Count == 0)
            {
                instance = CreateGO( prefab, pool.Counter++ );
                Debug.Log( $"Added new item in pool {prefab.name}" );
            } else
            {
                instance = pool.Stack.Pop();
            }

            instance.SetActive( true );

            return instance;
        }

        public void ReleaseInstance(GameObject instance)
        {
            GameObject prefab = _originalPrefabs[instance];
            instance.SetActive( false );
            Pool pool = _pools[prefab];
            pool.Stack.Push( instance );
        }

        public void ReleaseInstance(GameObject instance, float delay)
        {
            StartCoroutine( ReleaseInstanceCoroutine( instance, delay ) );
        }

        public IEnumerator ReleaseInstanceCoroutine(GameObject instance, float delay)
        {
            yield return new WaitForSeconds( delay );
            ReleaseInstance( instance );
        }

        private void Start()
        {
            InitPools();
        }

        private void InitPools()
        {
            foreach (var predefine in _predefinePools)
            {
                AddPool( predefine.prefab, predefine.poolSize );
            }
        }

        private Pool AddPool(GameObject prefab, int size)
        {
            Pool newPool = new( size );
            _pools.Add( prefab, newPool );
            for (int i = 0; i < size; i++)
            {
                GameObject newGO = CreateGO( prefab, newPool.Counter++ );
                newPool.Stack.Push( newGO );
            }
            return newPool;
        }

        private GameObject CreateGO(GameObject prefab, int index)
        {
            var newGO = GameObject.Instantiate( prefab );
            newGO.name = $"{prefab.name}_{index}";
            newGO.transform.parent = transform;
            newGO.SetActive( false );
            _originalPrefabs.Add( newGO, prefab );
            return newGO;
        }
    }
}