using Checkers;
using NetMessaging;
using NetMessaging.GameLogic;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public static Client instance;

    public float timeStart = 0f;
    public bool isSearch;

    public TcpConnectedClient connectedClient { get; private set; }
    public IEnumerator startGameCoroutine;

    public bool StartCommand { get; set; }
    public LobbyInfo lobbyInfo;
    public string enemyName;
    public CheckerColor UserCheckerColor { get; set; }

    
    public bool TurnCommand { get; set; }
    public TurnInfo TurnInfo { get; set; }


    public bool SuperCheckerCommand { get; set; }
    public SuperCheckerInfo SuperChecker { get; set; }


    public bool GameEndCommand { get; set; }
    public VictoryInfo VictoryInfo { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Connect();
        
        //SceneManager.LoadScene(1);
    }

    public void Connect()
    {
        instance = this;
        var TcpClient = new TcpClient();
        connectedClient = new TcpConnectedClient(TcpClient);
        TcpClient.BeginConnect("192.168.0.107", 6323, (ar) => connectedClient.EndConnect(ar), null); //13.53.190.82 //
    }

    public void Update()
    {
        //if (startCommand && lobbyInfo != null)
        //{
        //    if (lobbyInfo.GameMode == GameMode.PVP)
        //        NetworkController.instance.StartPVPGame(connectedClient.currentUser.Name, enemyName, lobbyInfo.FirstTurnPlayerName);
        //    else
        //        NetworkController.instance.StartMiningGame(connectedClient.currentUser.Name);
        //}

        //if (turnCommand && turnInfo != null)
        //    NetworkController.instance.TurnCheckerFromServer(turnInfo);

        //if (superCheckerCommand && superChecker != null)
        //    NetworkController.instance.BecomeSuperCheckerFromServer(superChecker);

        //if (gameEndCommand && victoryInfo != null)
        //    NetworkController.instance.EndGameFromServer(victoryInfo);

        if (isSearch == true)
        {
            if (StartCommand == true)
            {
                if (startGameCoroutine != null)
                {
                    StopCoroutine(startGameCoroutine);
                    StartCoroutine(startGameCoroutine);
                }
            }
        }
    }


    public void SendLogin(LoginInfo loginInfo)
    {
        var message = MakeJsonMessage(nameof(LoginInfo), loginInfo);
        connectedClient?.Send(message);
    }

    public void SendLogout()
    {
        if (connectedClient?.currentUser != null)
        {
            var message = MakeJsonMessage(nameof(LogoutInfo), new LogoutInfo { Token = connectedClient.currentUser.Token });
            connectedClient?.Send(message);
        }
    }

    public void SendGameInfo(GameMode gameMode, string token)
    {
        var gameInfo = new GameInfo
        {
            GameMode = gameMode,
            Token = token
        };

        var message = MakeJsonMessage(nameof(GameInfo), gameInfo);
        connectedClient?.Send(message);
    }

    public static string MakeJsonMessage(string type, object data)
    {
        var serializedData = SerializeObject(data);

        var jmessage = new JsonMessage { Type = type, Data = serializedData };

        return SerializeObject(jmessage);
    }

    public static string SerializeObject(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public void OnDisconnect()
    {
        Destroy(gameObject);
        connectedClient?.Close();
    }

    public void ResetClient()
    {
        StartCommand = false;
        lobbyInfo = null;
        enemyName = string.Empty;
        TurnCommand = false;
        TurnInfo = null;
        SuperCheckerCommand = false;
        SuperChecker = null;
        GameEndCommand = false;
        VictoryInfo = null;

        connectedClient.currentUser = null;
        connectedClient.connection?.Close();        
    }

    private void OnApplicationQuit()
    {
        SendLogout();
        if(connectedClient?.connection?.Connected == true)
            connectedClient?.Close();
    }

}
