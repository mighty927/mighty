using Checkers;
using NetMessaging;
using NetMessaging.GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SearchButton : MonoBehaviour
{
    public GameObject miningObject;
    public GameObject pvpObject;
    public GameObject bsObject;
    public Text timerText;


    public Button miningButtonObject;
    public Button pvpButtonObject;
    public GameObject cancelObject;
    public GameObject backObject;

    public GameMode ModeType;

    private void Awake()
    {
        SearchInfo(false, false, true, false);
        OffModeButtons(true, true, false);
    }

    public void OnSearchClick()
    {
        switch (ModeType)
        {
            case GameMode.Mining:
                {
                    if (Client.instance.isSearch)
                        Client.instance.isSearch = false;

                    Client.instance.timeStart = 0f;
                    Client.instance.isSearch = true;
                    //START PLAY MINING
                    SearchInfo(true, false, false, true);
                    OffModeButtons(false, false, true);
                    //pvpButtonObject.enabled = false; ВЫКЛЮЧИТЬ АНИМАТОРЫ
                    Client.instance.SendGameInfo(GameMode.Mining, UserContoller.instance.userInfo.Token);
                    Client.instance.startGameCoroutine = WaitGameServerResponse();
                }
                break;
            case GameMode.PVP:
                {
                    if (Client.instance.isSearch)
                        Client.instance.isSearch = false;

                    Client.instance.timeStart = 0f;
                    Client.instance.isSearch = true;
                    //START PLAY PVp
                    SearchInfo(false, true, false, true);
                    OffModeButtons(false, false, true);
                    //miningButtonObject.enabled = false;
                    Client.instance.SendGameInfo(GameMode.PVP, UserContoller.instance.userInfo.Token);
                    Client.instance.startGameCoroutine = WaitGameServerResponse();
                }
                break;
            default:
                break;
        }
    }

    public void OnCancelClick()
    {
        //Отправить запрос на отмену
        var message = Client.MakeJsonMessage(nameof(CancelSearchInfo), new CancelSearchInfo { Username = UserContoller.instance.userInfo.UserName });
        Client.instance.connectedClient.Send(message);
    }

    private void Update()
    {

        if (Client.instance?.isSearch == true)
        {
            Client.instance.timeStart += Time.deltaTime;
            timerText.text = Mathf.Round(Client.instance.timeStart).ToString();
        }

        if(UserContoller.instance.cancelCommand)
        {
            UserContoller.instance.cancelCommand = false;
            SearchInfo(false, false, true, false);
            OffModeButtons(true, true, false);
        }
    }

    private void SearchInfo(bool mining, bool pvp, bool bs, bool timer)
    {
        miningObject.SetActive(mining);
        pvpObject.SetActive(pvp);
        bsObject.SetActive(bs);
        timerText.gameObject.SetActive(timer);
    }

    private void OffModeButtons(bool mining, bool pvp, bool cancel)
    {
        miningButtonObject.enabled = mining;
        pvpButtonObject.enabled = pvp;
        cancelObject.SetActive(cancel);
        backObject.SetActive(!cancel);
    }


    private IEnumerator WaitGameServerResponse()
    {
        yield return new WaitUntil(() => Client.instance.StartCommand);
        if (Client.instance.lobbyInfo != null && Client.instance.StartCommand)
        {
            SceneManager.LoadScene(2);
        }
    }
}
