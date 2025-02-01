using System;
using System.Linq;
using UnityEngine;

namespace NN
{
    public class MainContext : MonoBehaviour
    {
        public static MainContext _instance;

        public static MainContext Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<MainContext>();
                    if (!_instance)
                    {
                        _instance = new GameObject().AddComponent<MainContext>();
                    }
                    _instance.Load();
                }
                return _instance;
            }
        }

        [SerializeField] private int _defaultPort = 7777;
        [SerializeField] private string _defaultIP = "127.0.0.1";

        public string IP
        {
            get => _ip;
            set
            {
                if (ValidateIPv4( value ))
                {
                    _ip = value;
                } else
                {
                    throw new Exception( "IP adress is invalid" );
                }
            }
        }

        public string Port
        {
            get => _port.ToString();
            set
            {
                if (ValidatePort( value ))
                {
                    _port = int.Parse( value );
                } else
                {
                    throw new Exception( "Port is invalid" );
                }
            }
        }

        public string SelfPort
        {
            get => _selfPort.ToString();
            set
            {
                if (ValidatePort( value ))
                {
                    _selfPort = int.Parse( value );
                } else
                {
                    throw new Exception( "Port is invalid" );
                }
            }
        }

        public string ErrMsg
        {
            get => _errMsg;
            set => _errMsg = value;
        }

        public string SplashText
        {
            get => _splashText;
            set => _splashText = value;
        }

        public bool IsServer
        {
            get => _isServer;
            set => _isServer = value;
        }

        public bool RunFromMenu
        {
            get => _runFromMenu;
            set => _runFromMenu = value;
        }

        public void Save()
        {
            PlayerPrefs.SetInt( "MainContext/SelfPort", _selfPort );
            PlayerPrefs.SetInt( "MainContext/Port", _port );
            PlayerPrefs.SetString( "MainContext/IP", _ip );
        }

        public void Load()
        {
            _selfPort = PlayerPrefs.GetInt( "MainContext/SelfPort", _defaultPort );
            _port = PlayerPrefs.GetInt( "MainContext/Port", _defaultPort );
            _ip = PlayerPrefs.GetString( "MainContext/IP", _defaultIP );
        }

        private string _errMsg;
        private string _splashText;
        private string _ip;
        private int _port;
        private int _selfPort;
        private bool _isServer;
        private bool _runFromMenu;

        private void Start()
        {
            DontDestroyOnLoad( gameObject );
        }

        private bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace( ipString ))
            {
                return false;
            }

            string[] parts = ipString.Split( '.' );
            if (parts.Length != 4)
            {
                return false;
            }

            return parts.All( s => byte.TryParse( s, out _ ) );
        }

        private bool ValidatePort(string port)
        {
            return int.TryParse( port.ToString(), out int p ) && p > 0 && p <= 65535;
        }
    }
}