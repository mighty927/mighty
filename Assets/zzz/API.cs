using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;  

public class API : MonoBehaviour
{
    public InputField UsernameField;
    public InputField PasswordField;
    //public Text TokenText;
   

    public User user;
    public UserSecurity security;   
    private LUI_UIAnimManager UIAnimManager;


    public void Awake()
    {
        UIAnimManager = gameObject.GetComponent<LUI_UIAnimManager>();
    }

    public void OnLoginButton()
    {
        user = new User
        {
            username = UsernameField.text,
            password = PasswordField.text
        };
        
        //TokenText.text = security.token;

        StartCoroutine(nameof(LoginRequest));

    }

    public IEnumerator LoginRequest()
    {
        var message = JsonConvert.SerializeObject(user);

        var request = new UnityWebRequest("http://192.168.0.107:4000/api/auth/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(message);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();


        if (request.responseCode == 200)
        {
            Debug.Log("VSE OK");
            var otvet = request.downloadHandler.text;
            var webUserResponse = JsonConvert.DeserializeObject<UserSecurity>(otvet);

            if (webUserResponse != null)
            {
                UIAnimManager.enabled = true;
                //UIAnimManager.TaskOnClick();

                //TCP CLient
            }

            //

            security = webUserResponse;
            //TokenText.text = security.token;
        }
        else
        {
            Debug.Log("OSHIBKA");
        }
    }
         
}

public class User
{
    public string username { get; set; }
    public string password { get; set; }
}

public class UserSecurity
{
    public string token { get; set; }
    public string refreshToken { get; set; }
}




