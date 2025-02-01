using NN;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Menu
{
    public class StartMenuState : MenuState
    {
        [SerializeField] private Button _startBtn;
        [SerializeField] private Button _backBtn;
        [SerializeField] private TMP_InputField _portFld;

        private void Awake()
        {
            Assert.IsNotNull( _startBtn );
            Assert.IsNotNull( _backBtn );
            Assert.IsNotNull( _portFld );
        }

        private void Start()
        {
            _startBtn.onClick.AddListener( () =>
            {
                var mainContext = menuContoller.Context as MainContext;
                try
                {
                    mainContext.SelfPort = _portFld.text;
                } catch (System.Exception e)
                {
                    mainContext.ErrMsg = e.Message;
                    menuContoller.CallAction( "MsgBox" );
                    return;
                }

                menuContoller.CallAction( "StartGame" );
            } );

            _backBtn.onClick.AddListener( () => menuContoller.ReleaseCurrentMenu() );
        }
    }
}