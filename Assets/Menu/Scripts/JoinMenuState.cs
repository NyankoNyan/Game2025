using NN;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Menu
{
    public class JoinMenuState : MenuState
    {
        [SerializeField] private Button _joinBtn;
        [SerializeField] private Button _backBtn;
        [SerializeField] private TMP_InputField _portFld;
        [SerializeField] private TMP_InputField _IPFld;

        private void Awake()
        {
            Assert.IsNotNull( _joinBtn );
            Assert.IsNotNull( _backBtn );
            Assert.IsNotNull( _portFld );
            Assert.IsNotNull( _IPFld );
        }

        private void Start()
        {
            _joinBtn.onClick.AddListener( () =>
            {
                var mainContext = menuContoller.Context as MainContext;
                try
                {
                    mainContext.SelfPort = _portFld.text;
                    mainContext.IP = _IPFld.text;
                } catch (System.Exception e)
                {
                    mainContext.ErrMsg = e.Message;
                    menuContoller.CallAction( "MsgBox" );
                    return;
                }

                menuContoller.CallAction( "JoinGame" );
            } );

            _backBtn.onClick.AddListener( () => menuContoller.ReleaseCurrentMenu() );
        }
    }
}