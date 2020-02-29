using Checkers;
using DG.Tweening.Core;
using NetMessaging.GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinController : MonoBehaviour
{
    public static WinController instance;

    public GameController GameControllerComponent;

    private void Awake()
    {
        instance = this;
    }
    
    public void RestartGame()
    {
        if (Client.instance.isSearch)
            Client.instance.isSearch = false;

        ClearClient();

        Client.instance.timeStart = 0f;
        Client.instance.isSearch = true;

        //START PLAY MINING
        //SearchInfo(true, false, false, true);
        //pvpButtonObject.enabled = false; ВЫКЛЮЧИТЬ АНИМАТОРЫ
        Client.instance.SendGameInfo(GameMode.Mining, UserContoller.instance.userInfo.Token);
        Client.instance.startGameCoroutine = WaitGameServerResponse();
    }

    private IEnumerator WaitGameServerResponse()
    {
        yield return new WaitUntil(() => Client.instance.StartCommand);


        if (Client.instance.StartCommand && Client.instance?.lobbyInfo?.GameMode == GameMode.Mining)
        {
            if (GameControllerComponent._moveCoroutine != null)
            {
                StopCoroutine(GameControllerComponent._moveCoroutine);
            }

            GameControllerComponent.BoardViewCompoennt.Reset();
            GameControllerComponent.UiViewComponent.ResetUIView();
            GameControllerComponent.StartMining();
        }
        else if (Client.instance.StartCommand && Client.instance?.lobbyInfo?.GameMode == GameMode.PVP)
        {
            GameControllerComponent.StartPVPGame();
        }
    }

    private void ClearClient()
    {
        UserContoller.instance.BeetText = null;
        UserContoller.instance.UsernameText = null;
        Client.instance.lobbyInfo = null;
        Client.instance.StartCommand = false;
        NetworkController.instance.GameEndCommand = false;
        GameControllerComponent.IsGameStart = false;
    }

    public void OnBackMenuClick()
    {       
        var doTween = FindObjectOfType<DOTweenComponent>();
        Destroy(doTween?.gameObject);

        SceneManager.LoadSceneAsync(1);
        //SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
