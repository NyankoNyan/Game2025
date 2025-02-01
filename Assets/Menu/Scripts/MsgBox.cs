using NN;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Menu
{
    public class MsgBox : MenuState
    {
        [SerializeField] private Button _okBtn;
        [SerializeField] private TextMeshProUGUI _text;

        private void Awake()
        {
            Assert.IsNotNull( _okBtn );
            Assert.IsNotNull( _text );
        }

        private void Start()
        {
            _okBtn.onClick.AddListener( () => menuContoller.ReleaseCurrentMenu() );
        }

        public override void Show(IMenuController menuContoller)
        {
            base.Show( menuContoller );

            _text.text = ((MainContext)menuContoller.Context).ErrMsg;
        }
    }
}