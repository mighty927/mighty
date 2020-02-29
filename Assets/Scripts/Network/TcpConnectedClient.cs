using Checkers;
using NetMessaging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using NetMessaging.GameLogic;

public class TcpConnectedClient
{
    #region Data
    /// <summary>
    /// For Clients, the connection to the server.
    /// For Servers, the connection to a client.
    /// </summary>
    public readonly TcpClient connection;

    public readonly byte[] readBuffer = new byte[5000];

    //readonly Client client;

    public UserInfo currentUser;

    //NetworkController networkController;

    public NetworkStream stream
    {
        get
        {
            return connection?.GetStream();
        }
    }
    #endregion

    #region Init
    public TcpConnectedClient(TcpClient tcpClient)
    {
        this.connection = tcpClient;
        //this.client = client;        
        this.connection.NoDelay = true; // Disable Nagle's cache algorithm
    }

    internal void Close()
    {
        stream?.Close();
        connection?.Close();
    }
    #endregion

    #region Async Events
    public void OnRead(IAsyncResult ar)
    {
        int length = 0;
        try
        {
            length = stream.EndRead(ar);
        }
        catch
        {
            Client.instance.OnDisconnect();
            Debug.Log("Сервер не доступен");
        }
        

        if (length <= 0)
        { // Connection closed
            //Client.Instance.OnDisconnect(this);
            Client.instance.OnDisconnect();
            Debug.Log("При чтении получил нихуя байтов че за хуйня?");
            return;
        }

        string newMessage = System.Text.Encoding.UTF8.GetString(readBuffer, 0, length);
        var jsonMessage = JsonConvert.DeserializeObject<JsonMessage>(newMessage);

        switch (jsonMessage.Type)
        {
            case nameof(UserInfo):
                {
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(jsonMessage.Data);
                    currentUser = userInfo;
                    Authentication.instance.awaiter = false;

                    Debug.Log(userInfo.Token);
                }
                break;
            case nameof(CancelSearchInfo):
                {
                    //CANCEL SEARCH
                    UserContoller.instance.cancelCommand = true;
                }
                break;
            case nameof(LobbyInfo):
                {

                    var lobbyInfo = JsonConvert.DeserializeObject<LobbyInfo>(jsonMessage.Data);
                    //START GAME

                    Debug.Log(lobbyInfo.FirstTurnPlayerName);
                    if (lobbyInfo.GameMode == GameMode.PVP)
                    {
                        var enemyName = lobbyInfo.FirstPlayer.Equals(currentUser.UserName) ? lobbyInfo.SecondPlayer : lobbyInfo.FirstPlayer;
                        Client.instance.enemyName = enemyName;
                    }
                    else
                    {
                        //Если майнинг то полюбому катаем белыми
                        Client.instance.UserCheckerColor = CheckerColor.White;
                    }
                    Client.instance.lobbyInfo = lobbyInfo;
                    Client.instance.StartCommand = true;
                    Debug.Log($"Lobby ID: {lobbyInfo.Id}, GameMode: {lobbyInfo.GameMode.ToString()}");

                    
                }
                break;
            case nameof(TurnInfo):
                {
                    var turnInfo = JsonConvert.DeserializeObject<TurnInfo>(jsonMessage.Data);
                    Debug.Log(turnInfo.Square.Id + $"Position:{turnInfo.Square.Position.X}, {turnInfo.Square.Position.Y}");

                    NetworkController.instance.TurnInfo = turnInfo;
                    NetworkController.instance.TurnCommand = true;
                }
                break;
            case nameof(SuperCheckerInfo):
                {
                    var superCheckerInfo = JsonConvert.DeserializeObject<SuperCheckerInfo>(jsonMessage.Data);
                    NetworkController.instance.SuperChecker = superCheckerInfo;
                    NetworkController.instance.SuperCheckerCommand = true;
                }
                break;
            case nameof(VictoryInfo):
                {
                    var victoryInfo = JsonConvert.DeserializeObject<VictoryInfo>(jsonMessage.Data);
                    NetworkController.instance.GameEndCommand = true;
                    NetworkController.instance.VictoryInfo = victoryInfo;
                    UserContoller.instance.endGameCoroutine = NetworkController.instance.EndGameServerResponse();
                }
                break;
            case nameof(ErrorInfo):
                {
                    var errorInfo = JsonConvert.DeserializeObject<ErrorInfo>(jsonMessage.Data);
                    //LOAD AUTH SCENE
                    Authentication.instance.awaiter = false;
                    Authentication.instance.displayError = errorInfo.Message;
                    Debug.Log(errorInfo.Message);

                }
                break;
            default:
                break;
        }

        if(stream.CanRead)
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        else
        {
            Debug.Log("Ошибка на чтении обычном");
            Client.instance.OnDisconnect();
        }
    }

    #region SEND INFO
    public void SendTurn(Checker checker, Square square, Square target = null)
    {
        var CheckerDto = new NetMessaging.Turns.Checker { Id = checker.Id, IsSuperChecker = checker.IsSuperChecker };
        var SquareDto = new NetMessaging.Turns.Square { Id = square.Id, Position = new NetMessaging.Turns.Position { X = square.Position.X, Y = square.Position.Y } };

        CheckerColor checkerColor;

        if (NetworkController.instance.GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.White)
        {            
            checkerColor = CheckerColor.White;
        }
        else
        {            
            checkerColor = CheckerColor.Black;
        }

        var turnInfo = new TurnInfo
        {
            LobbyId = Client.instance.lobbyInfo.Id,
            Token = currentUser.Token,
            Checker = CheckerDto,
            CheckerColor = checkerColor,
            Square = SquareDto,
            IntermediateSquare = target != null ? new NetMessaging.Turns.Square { Id = target.Id } : null
        };

        var message = Client.MakeJsonMessage(nameof(TurnInfo), turnInfo);

        Send(message);
    }

    public void SendSuperChecker(int id)
    {
        var superChekerDto = new SuperCheckerInfo
        {
            Id = id,
            CheckerColor = Client.instance.UserCheckerColor, 
            LobbyId = Client.instance.lobbyInfo.Id
        };

        Send(Client.MakeJsonMessage(nameof(SuperCheckerInfo), superChekerDto));

    }

    public void SendVictory(string token)
    {
        var victoryInfo = new VictoryInfo
        {
            LobbyId = Client.instance.lobbyInfo.Id,
            VictoryToken = token
        };

        var jmessage = Client.MakeJsonMessage(nameof(VictoryInfo), victoryInfo);
        Send(jmessage);
    }
    #endregion
    

    internal void EndConnect(IAsyncResult ar)
    {
        try
        {
            connection.EndConnect(ar);

            if (stream.CanRead)
                stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
            else
            {
                Debug.Log("Ошибка чтения в конец подключения");
                Client.instance.OnDisconnect();
            }
        }
        catch
        {
            throw new SocketException();
        }

        //Когда подключились отдаем инфу

        //Ловим мод
        //var mode = NetworkController.instance.GameControllerComponent.Mode == GameMode.Mining ? GameMode.Mining : GameMode.PVP;

        //var message = Client.MakeJsonMessage(nameof(UserInfo), userInfo);

        //Send(message);
        
    }
    #endregion

    #region API
    internal void Send(string message)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

        if(stream.CanWrite)
            stream.Write(buffer, 0, buffer.Length);
        else
        {
            Debug.Log("Ошибка поссылки сообщения блять сука нахуй");
            Client.instance.OnDisconnect();
        }
    }
    #endregion
}
