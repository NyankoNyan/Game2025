using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewWeaponSystem
{
    [Serializable]
    public class GunModelSetup
    {
        public GameObject Prefab;
        public string RotateBoneName;
        public string AngleBoneName;
    }

    public class LoadedGunModel
    {
        public GameObject GameObject;
        public GunModelSetup Setup;
    }

    public class Gun
    {
        public class GunEffectStatus
        {
            public float ShowPart = 1;
            public Coroutine CurrentShowCorroutine;
        }

        public LoadedGunModel GunModel;
        public GunShootSetup ShootSetup;
        public GunEffectStatus EffectStatus = new();
    }

    public class GunChanger : MonoBehaviour
    {
        [Serializable]
        public struct GunChangeEffectSetup
        {
            public float ChangeTime;
        }

        [SerializeField]
        private Transform _mountPoint;

        [SerializeField]
        private List<GunModelSetup> _gunModelSetups = new();

        [SerializeField]
        private GunChangeEffectSetup _gunChangeEffectSetup = new()
        {
            ChangeTime = .5f,
        };

        private List<Gun> _guns = new();
        private Gun _currentGun;

        public void AddGun(IGunSetup gunSetup)
        {
            GunSetup gunSetupLocal = gunSetup.Get();
            LoadedGunModel loadedGun = LoadGunModel(gunSetupLocal.ModelPrefab);
            Gun gun = new()
            {
                GunModel = loadedGun,
                ShootSetup = gunSetupLocal.ShootSetup.Get()
            };
            _guns.Add(gun);
            SwapGuns(_currentGun, gun);
        }

        private void SwapGuns(Gun oldGun, Gun newGun)
        {
            if (oldGun != null)
            {
                if (oldGun.EffectStatus.CurrentShowCorroutine != null)
                {
                    StopCoroutine(oldGun.EffectStatus.CurrentShowCorroutine);
                }
                oldGun.EffectStatus.CurrentShowCorroutine = StartCoroutine(GunHideCoroutine(oldGun));
            }
            if (newGun != null)
            {
                if (newGun.EffectStatus.CurrentShowCorroutine != null)
                {
                    StopCoroutine(newGun.EffectStatus.CurrentShowCorroutine);
                }
                newGun.EffectStatus.CurrentShowCorroutine = StartCoroutine(GunShowCoroutine(newGun));
            }
            _currentGun = newGun;
        }

        public void SetNextGun()
        {
            int current = _guns.IndexOf(_currentGun);
            if (current == -1)
            {
                if (_guns.Count > 0)
                {
                    SwapGuns(_currentGun, _guns[0]);
                }
            }
            else
            {
                if (_guns.Count > 1)
                {
                    int next = (current + 1) % _guns.Count;
                    SwapGuns(_currentGun, _guns[next]);
                }
            }
        }

        public void SetPrevGun()
        {
            int current = _guns.IndexOf(_currentGun);
            if (current == -1)
            {
                if (_guns.Count > 0)
                {
                    SwapGuns(_currentGun, _guns[^1]);
                }
            }
            else
            {
                if (_guns.Count > 1)
                {
                    int prev = (current - 1 + _guns.Count) % _guns.Count;
                    SwapGuns(_currentGun, _guns[prev]);
                }
            }
        }

        public void RemoveCurrentGun()
        {
            int current = _guns.IndexOf(_currentGun);
            if (current != -1)
            {
                Gun oldGun = _currentGun;
                _guns.RemoveAt(current);
                if (_guns.Count > 0)
                {
                    int next = current % _guns.Count;
                    SwapGuns(oldGun, _guns[next]);
                }
                else
                {
                    SwapGuns(oldGun, null);
                }
            }
        }

        private IEnumerator GunHideCoroutine(Gun gun)
        {
            float time = 0f;

            GameObject go = gun.GunModel.GameObject;

            // Получаем все материалы объекта
            var renderers = go.GetComponentsInChildren<Renderer>();
            var materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                materials.AddRange(renderer.materials);
            }

            while (time < _gunChangeEffectSetup.ChangeTime)
            {
                time += Time.deltaTime;
                gun.EffectStatus.ShowPart = Mathf.Clamp01(1 - (time / _gunChangeEffectSetup.ChangeTime));

                // Устанавливаем параметр шейдера _ShowPart
                foreach (var material in materials)
                {
                    material.SetFloat("_ShowPart", gun.EffectStatus.ShowPart);
                }

                yield return null;
            }

            // Убедимся, что параметр _ShowPart установлен в 0 в конце
            gun.EffectStatus.ShowPart = 0f;
            foreach (var material in materials)
            {
                material.SetFloat("_ShowPart", 0f);
            }

            // Деактивируем объект после завершения анимации
            go.SetActive(false);

            gun.EffectStatus.CurrentShowCorroutine = null;
        }

        private IEnumerator GunShowCoroutine(Gun gun)
        {
            float time = _gunChangeEffectSetup.ChangeTime * gun.EffectStatus.ShowPart;

            GameObject go = gun.GunModel.GameObject;
            go.SetActive(true);

            // Получаем все материалы объекта
            var renderers = go.GetComponentsInChildren<Renderer>();
            var materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                materials.AddRange(renderer.materials);
            }

            while (time < _gunChangeEffectSetup.ChangeTime)
            {
                time += Time.deltaTime;
                gun.EffectStatus.ShowPart = Mathf.Clamp01(time / _gunChangeEffectSetup.ChangeTime);

                // Устанавливаем параметр шейдера _ShowPart
                foreach (var material in materials)
                {
                    material.SetFloat("_ShowPart", gun.EffectStatus.ShowPart);
                }

                yield return null;
            }

            // Убедимся, что параметр _ShowPart установлен в 1 в конце
            gun.EffectStatus.ShowPart = 1f;
            foreach (var material in materials)
            {
                material.SetFloat("_ShowPart", 1f);
            }

            gun.EffectStatus.CurrentShowCorroutine = null;
        }

        /// <summary>
        /// Загружает модель оружия
        /// </summary>
        private LoadedGunModel LoadGunModel(GameObject prefab)
        {
            GunModelSetup gunModelSetup = _gunModelSetups.Find(g => g.Prefab == prefab);
            if (gunModelSetup == null)
            {
                throw new ArgumentException($"Gun model with prefab {prefab.name} not found");
            }

            GameObject go = Instantiate(gunModelSetup.Prefab, _mountPoint);
            go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            LoadedGunModel loadedGun = new() { GameObject = go, Setup = gunModelSetup };

            return loadedGun;
        }
    }
}