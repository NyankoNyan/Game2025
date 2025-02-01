using NN;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Menu
{
    public class SplashScreen : MenuState
    {
        [SerializeField] private TextMeshProUGUI _text;
        private void Awake()
        {
            Assert.IsNotNull( _text );
        }
        public override void Show(IMenuController menuContoller)
        {
            base.Show( menuContoller );
            _text.text = ((MainContext)menuContoller.Context).SplashText;
        }
    }
}