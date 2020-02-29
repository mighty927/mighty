using NetMessaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserContoller : MonoBehaviour
{
    public static UserContoller instance;

    public Text UsernameText;
    public Text BeetText;

    public UserInfo userInfo { get; set; }

    public bool cancelCommand;

    public IEnumerator endGameCoroutine;

    private void Awake()
    {
        instance = this;
        //DontDestroyOnLoad(gameObject);
        userInfo = Client.instance.connectedClient.currentUser;
    }
   
    private void Update()
    {

        if (!string.IsNullOrEmpty(userInfo?.UserName))
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == 1)
            {
                if (UsernameText != null)
                    UsernameText.text = userInfo.UserName;


                if (BeetText != null)
                    if (userInfo?.BeetCount >= 0)
                        BeetText.text = userInfo.BeetCount.ToString();
            }
        }
        
    }

    public void OnLogout()
    {
        Client.instance.SendLogout();
        Client.instance.ResetClient();
        Destroy(Client.instance.gameObject);
        //SEND LOGOUT

        SceneManager.LoadScene(0);
    }
}
