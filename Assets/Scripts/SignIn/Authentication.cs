using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetMessaging;
using System;
using UnityEngine.SceneManagement;
using System.Net.Sockets;

public class Authentication : MonoBehaviour
{
    public static Authentication instance;

    public InputField UsernameField;
    public InputField PasswordField;
    public Button connectButton;
    public Text errorText;

    public LoginInfo loginInfo;
    public GameObject loadingCircleObject;

    //CLIENT SEVER OBJECT
    public GameObject ClientPrefab;

    public bool awaiter;
    public string displayError;
    public float timeStart;

    private IEnumerator authenticateCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        errorText.text = displayError;

        if (!awaiter)
        {
            loadingCircleObject.SetActive(false);
            connectButton.enabled = true;
            if (authenticateCoroutine != null)
            {
                StopCoroutine(authenticateCoroutine);
                StartCoroutine(authenticateCoroutine);
            }
        }
        else
        {
            if (timeStart >= 120.0f)
            {
                displayError = "Тайм-аут подключения к серверу BS Games";
                //var oldClient = FindObjectOfType<Client>();
                //Destroy(oldClient?.gameObject);
                ResetConnection();
                return;
            }
            else
            {
                timeStart += Time.deltaTime;
                loadingCircleObject.SetActive(true);
                connectButton.enabled = false;
            }
        }

        //if (!string.IsNullOrEmpty(Client.instance?.connectedClient?.currentUser?.Token))
        //    SceneManager.LoadScene(1);
    }

    public void OnLoginButton()
    {
        displayError = "";

        //Проверка на ClientValidation
        if (!ValidateUser())
            return;

        ResetConnection();

        awaiter = true;

        loginInfo = new LoginInfo
        {
            username = UsernameField.text, //Проверить на Regex Trim ToLower
            password = PasswordField.text
        };
        
        StartCoroutine(nameof(LoginRequest));
    }

    private bool ValidateUser()
    {
        UsernameField.text = UsernameField.text.ToLower().Trim();
        if (string.IsNullOrEmpty(UsernameField.text))
        {
            displayError = "Введите логин";
            awaiter = false;
            return false;
        }

        if(string.IsNullOrEmpty(PasswordField.text))
        {
            displayError = "Введите пароль";
            awaiter = false;
            return false;
        }

        return true;
    }

    private IEnumerator LoginRequest()
    {
        var oldClient = FindObjectOfType<Client>();
        try
        {
            if (oldClient == null)
            {
                oldClient = Instantiate(ClientPrefab).GetComponent<Client>();
                oldClient.gameObject.name = "Client" + new System.Random().Next(1, 150);
                oldClient.SendLogin(loginInfo);
                authenticateCoroutine = WaitLoginRequest();
            }
            else if(oldClient?.connectedClient?.connection?.Connected == true)
            {                
                oldClient.SendLogin(loginInfo);
                authenticateCoroutine = WaitLoginRequest();
            }
            else
            {
                Debug.Log("Бля чет подключение наебнулось");
            }
        }
        catch(SocketException)
        {
            displayError = "Удаленный сервер не доступен";
        }
        catch(InvalidOperationException)
        {
            displayError = "Не удалось подключиться, повторите еще раз";
        }

        if (authenticateCoroutine == null)
        {
            //displayError = "Сервер не доступен, проверьте ваше соединение с интернетом";            
            awaiter = false;
        }

        yield break;
    }
    private IEnumerator WaitLoginRequest()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(Client.instance?.connectedClient?.currentUser?.Token));

        if (!string.IsNullOrEmpty(Client.instance?.connectedClient?.currentUser?.Token))
            SceneManager.LoadScene(1);

        //Enable btn
    }

    private void ResetConnection()
    {
        //Отключить все
        StopAllCoroutines();
        awaiter = false;
        timeStart = 0f;
        authenticateCoroutine = null;
        //Client.instance?.connectedClient.connection?.Close();
        //if(Client.instance?.gameObject != null)
        //    Destroy(Client.instance?.gameObject);
        //var clients = FindObjectsOfType<Client>();

        //foreach (var client in clients)
        //{
        //    Destroy(client.gameObject);
        //}
    }
}
