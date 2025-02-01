using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu
{
    public abstract class MenuState : MonoBehaviour
    {
        protected IMenuController menuContoller;

        [SerializeField] private float _showTime = .2f;
        [SerializeField] private float _hideTime = .2f;

        public UnityAction hide;
        public UnityAction show;

        public virtual void Show(IMenuController menuContoller)
        {
            this.menuContoller = menuContoller;
            gameObject.SetActive( true );

            StartCoroutine( ShowCoroutine() );
        }

        public virtual void Hide()
        {
            StartCoroutine( HideCoroutine() );
        }

        private IEnumerator ShowCoroutine()
        {
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster)
                raycaster.enabled = false;

            float timer = 0;
            Transform scaleTarget = transform.GetChild( 0 );
            do
            {
                timer += Time.deltaTime;
                scaleTarget.localScale = new Vector3( 1, Mathf.SmoothStep( 0, 1, timer / _showTime ), 1 );
                yield return null;
            } while (timer < _showTime);
            if (raycaster)
                raycaster.enabled = true;
            show?.Invoke();
        }

        private IEnumerator HideCoroutine()
        {
            float timer = 0;
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster)
                raycaster.enabled = false;
            Transform scaleTarget = transform.GetChild( 0 );
            do
            {
                timer += Time.deltaTime;
                scaleTarget.localScale = new Vector3( 1, Mathf.SmoothStep( 1, 0, timer / _hideTime ), 1 );
                yield return null;
            } while (timer < _hideTime);
            if (raycaster)
                raycaster.enabled = true;
            gameObject.SetActive( false );
            hide?.Invoke();
        }
    }
}