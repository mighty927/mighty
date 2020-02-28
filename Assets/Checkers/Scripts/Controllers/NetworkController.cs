using System;
using UnityEngine;
using Checkers;
using NetMessaging.GameLogic;
using NetMessaging;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
    public static NetworkController instance;

    public GameController GameControllerComponent;


    //private const string HOSTADDRESS = "127.0.0.1";
    //private const int PORT = 6322;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        instance = this;
    }


    //public void StartPVPGame(string me, string enemy, string firstTurnNamePlayer)
    //{
    //    Client.instance.StartCommand = false;

    //    GameControllerComponent.UiViewComponent.FirstPlayerName = me;
    //    GameControllerComponent.UiViewComponent.SecondPlayerName = enemy;
    //    //GameControllerComponent.ChooseMode(GameMode.PVP);

    //    //if (me.Equals(firstTurnNamePlayer))
    //    //{
    //    //    GameControllerComponent.BoardViewCompoennt.SetColorPlayer(UserColor.White);
    //    //    Client.instance.connectedClient.currentUser.UserColor = UserColor.White;
    //    //}
    //    //else
    //    //{
    //    //    GameControllerComponent.BoardViewCompoennt.SetColorPlayer(UserColor.Black);
    //    //    Client.instance.connectedClient.currentUser.UserColor = UserColor.Black;

    //    //    if (GameControllerComponent._moveCoroutine != null)
    //    //        StopCoroutine(GameControllerComponent._moveCoroutine);

    //    //    GameControllerComponent._moveCoroutine = GameControllerComponent.CoreInstance.SecondPlayerMove();
    //    //    StartCoroutine(GameControllerComponent._moveCoroutine);


    //    //}
    //}

    //public void StartMiningGame(string me)
    //{
    //    //Client.instance.StartCommand = false;
    //    //GameControllerComponent.UiViewComponent.FirstPlayerName = me;
    //    ////GameControllerComponent.ChooseMode(GameMode.Mining);
    //    //GameControllerComponent.BoardViewCompoennt.SetColorPlayer(UserColor.White);
    //    ////Client.instance.connectedClient.currentUser.UserColor = UserColor.White;
    //}

    public void TurnCheckerFromServer(TurnInfo turnInfo)
    {
        Client.instance.TurnCommand = false;


        var square = GameControllerComponent.CoreInstance._squaresData[turnInfo.Square.Id];
        var checker = GameControllerComponent.CoreInstance._checkersData[turnInfo.Checker.Id];

        Square intermediateSquare = null;
        if (turnInfo.IntermediateSquare != null)
            intermediateSquare = GameControllerComponent.CoreInstance._squaresData[turnInfo.IntermediateSquare.Id];
        


        GameControllerComponent.CoreInstance.TryToMoveChekerAsync(checker, square, intermediateSquare);

        //Если мой ход
        if (GameControllerComponent.CoreInstance.CurrentMoveColor != checker.Color)
        {
            //if (turnInfo.Username.Equals(Client.instance.connectedClient.currentUser.Name))
            //{
            //    if (GameControllerComponent._moveCoroutine != null)
            //        StopCoroutine(GameControllerComponent._moveCoroutine);

            //    GameControllerComponent._moveCoroutine = GameControllerComponent.CoreInstance.SecondPlayerMove();
            //    StartCoroutine(GameControllerComponent._moveCoroutine);
            //}
            //else
            //{
            //    GameControllerComponent.CoreInstance.IsBeatProcessActive = false;
            //    GameControllerComponent.CoreInstance.IsEnemyMove = false;
            //}

        }
        else
        {
            GameControllerComponent.CoreInstance.IsBeatProcessActive = false;
            GameControllerComponent.CoreInstance.IsEnemyMove = false;
        }
    }

    public void BecomeSuperCheckerFromServer(SuperCheckerInfo superChecker)
    {
        Client.instance.SuperCheckerCommand = false;
        GameControllerComponent.CoreInstance.BecomeSuperCheckAsync(superChecker.Id);
    }

    public IEnumerator EndGameServerResponse()
    {
        yield return new WaitUntil(() => Client.instance.GameEndCommand);

        if (Client.instance.VictoryInfo != null && Client.instance.GameEndCommand)
        {
            GameControllerComponent.IsGameStart = false;
            Client.instance.GameEndCommand = false;
            GameControllerComponent.CoreInstance.GameEnd = true;
            UiViewController.Instance.ShowWinMessage(Client.instance.VictoryInfo.VictoryName);

            if (Client.instance.VictoryInfo.VictoryName == Client.instance.connectedClient.currentUser.UserName)
            {
                AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Win);
            }
            else
            {
                AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Lose);
            }
        }
    }

    #region Buttons
    //public void OnMiningClicked()
    //{
    //    try
    //    {
    //        Panel.SetActive(false);

    //        //Проверка на подключение


    //        //var client = Instantiate(ClientPrefab).GetComponent<Client>();
    //        //client.connectedClient.currentUser.Name = "Unity" + new System.Random().Next(11111, 99999);
    //        //client.connectedClient.currentUser.GameMode = GameMode.Mining;
    //        //if (string.IsNullOrEmpty(client.connectedClient.currentUser.Name))
    //        //    client.connectedClient.currentUser.Name = "Unity";

    //        GameControllerComponent.ChooseMode(GameMode.Mining);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log(e.Message);
    //    }
    //}

    //public void OnPVPClicked()
    //{      

    //    try
    //    {
    //        Panel.SetActive(false);

    //        //Проверка на подключение

    //        //var client = Instantiate(ClientPrefab).GetComponent<Client>();
    //        //client.connectedClient.currentUser.Name = "Unity" + new System.Random().Next(11111, 99999);
    //        //if (string.IsNullOrEmpty(client.connectedClient.currentUser.Name))
    //        //    client.connectedClient.currentUser.Name = "Unity";

    //        GameControllerComponent.ChooseMode(GameMode.PVP);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log(e.Message);
    //    }
    //}

    public void OnCancelButton()
    {

        Client client = FindObjectOfType<Client>();
        if (client != null)
            Destroy(client.gameObject);


    }
    #endregion
}
