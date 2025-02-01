using NN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Menu
{
    public interface IMenuController
    {
        void ReleaseCurrentMenu();

        void CallAction(string id);

        object Context { get; }
    }

    public class MainMenuController : MonoBehaviour, IMenuController
    {
        private Stack<MenuState> _statesStack = new();

        public object Context => NN.MainContext.Instance;

        public void CallAction(string id)
        {
            switch (id)
            {
                case "StartGame":
                {
                    var mainContext = Context as MainContext;
                    mainContext.SplashText = "Starting game...";
                    SwitchMenu( FindAnyObjectByType<SplashScreen>( FindObjectsInactive.Include ) );
                    mainContext.IsServer = true;
                    mainContext.RunFromMenu = true;
                    StartCoroutine( StartGame() );
                    break;
                }

                case "JoinGame":
                {
                    var mainContext = Context as MainContext;
                    mainContext.SplashText = "Joining game...";
                    SwitchMenu( FindAnyObjectByType<SplashScreen>( FindObjectsInactive.Include ) );
                    mainContext.IsServer = false;
                    mainContext.RunFromMenu = true;
                    StartCoroutine( StartGame() );
                    break;
                }

                case "Exit":
                    Exit();
                    break;

                case "StartMenu":
                    SwitchMenu( FindAnyObjectByType<StartMenuState>( FindObjectsInactive.Include ) );
                    break;

                case "JoinMenu":
                    SwitchMenu( FindAnyObjectByType<JoinMenuState>( FindObjectsInactive.Include ) );
                    break;

                case "MsgBox":
                    SwitchMenu( FindAnyObjectByType<MsgBox>( FindObjectsInactive.Include ) );
                    break;

                default:
                    Debug.LogError( $"Unknown action: {id}" );
                    break;
            }
        }

        public void ReleaseCurrentMenu()
        {
            var currentMenu = _statesStack.Pop();

            UnityAction onHide = null;

            onHide = () =>
            {
                if (_statesStack.TryPeek( out var prevMenu ))
                {
                    prevMenu.Show( this );
                } else
                {
                    throw new System.Exception( "No previous menu" );
                }

                currentMenu.hide -= onHide;
            };
            currentMenu.hide += onHide;

            currentMenu.Hide();
        }

        private void Start()
        {
            SwitchMenu( FindAnyObjectByType<MainMenuState>( FindObjectsInactive.Include ) );
        }

        private void Exit()
        {
            Application.Quit();
        }

        private void SwitchMenu(MenuState menuState)
        {
            if (_statesStack.TryPeek( out var currentMenu ))
            {
                UnityAction onHide = null;

                onHide = () =>
                {
                    _statesStack.Push( menuState );
                    menuState.Show( this );
                    currentMenu.hide -= onHide;
                };
                currentMenu.hide += onHide;
                currentMenu.Hide();
            } else
            {
                _statesStack.Push( menuState );
                menuState.Show( this );
            }
        }

        private IEnumerator StartGame()
        {
            var currentScene = SceneManager.GetActiveScene();
            var loadOp = SceneManager.LoadSceneAsync( "GameProto", LoadSceneMode.Additive );
            yield return new WaitUntil( () => loadOp.isDone );

            // Найти игрока на текущей сцене
            var player = GameObject.FindGameObjectWithTag( "Player" );
            if (player != null)
            {
                // Переместить игрока на игровую сцену
                Scene gameScene = SceneManager.GetSceneByName( "GameProto" );
                SceneManager.MoveGameObjectToScene( player.gameObject, gameScene );
            }

            // Установить активную сцену
            SceneManager.SetActiveScene( SceneManager.GetSceneByName( "GameProto" ) );

            // Выгрузить текущую сцену, если это необходимо
            var unloadOp = SceneManager.UnloadSceneAsync( currentScene );
            yield return new WaitUntil( () => unloadOp.isDone );
        }
    }
}