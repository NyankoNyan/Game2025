using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Menu
{
    public class MainMenuState : MenuState
    {
        [SerializeField] private Button _startBtn;
        [SerializeField] private Button _joinBtn;
        [SerializeField] private Button _exitBtn;

        private void Awake()
        {
            Assert.IsNotNull( _startBtn );
            Assert.IsNotNull( _joinBtn );
            Assert.IsNotNull( _exitBtn );
        }

        private void Start()
        {
            _startBtn.onClick.AddListener( () => menuContoller.CallAction( "StartMenu" ) );
            _joinBtn.onClick.AddListener( () => menuContoller.CallAction( "JoinMenu" ) );
            _exitBtn.onClick.AddListener( () => menuContoller.CallAction( "Exit" ) );
        }
    }
}