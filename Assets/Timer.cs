using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timeStart = 0f;
    public Text loading;
    public GameObject search;
    public Button obj;

    public bool isSearch;
    
    
    

    void Start()
    {
        search.SetActive(false);
        isSearch = false;
       
        
        loading.text = timeStart.ToString();
          
    }

     void Update()
     {
        
        if (isSearch)
        {
            timeStart += Time.deltaTime;
            loading.text = Mathf.Round(timeStart).ToString();
            search.SetActive(true);
        }
            

     }


    public void OnSearchClick()
    {
        isSearch = !isSearch;
        

        timeStart = 0f;

    }
}
