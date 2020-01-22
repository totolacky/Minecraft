using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScrollViewAdapter : MonoBehaviour
{
    [SerializeField]
    private GameObject listContent;
    [SerializeField]
    private Button buttonPrefab;

    private GameLogic gameLogic;

    // Start is called before the first frame update
    void Start()
    {
        var worlds = new ArrayList();

        if (PlayerPrefs.HasKey("worlds"))
        {
            worlds = (ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds"));
        }
        else
        {
            PlayerPrefs.SetString("worlds",MyUtil.ObjectToString(worlds));
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetString("worlds", MyUtil.ObjectToString(worlds));
        PlayerPrefs.Save();

        for (int i=0; i<worlds.Count; i++)
        {
            var worldName = (string)worlds[i];
            Button newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(listContent.transform, false);
            newButton.GetComponentInChildren<Text>().text = worldName;

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                gameLogic = FindObjectOfType<GameLogic>();
                gameLogic.setWorldName(worldName);
                SceneManager.LoadScene("Game");
            });
        }
    }

    public void createWorld()
    {
        SceneManager.LoadScene("World create scene");   
    }

    public void connectServer()
    {
        string userName = PlayerPrefs.GetString("userName");
        Debug.Log("username: "+userName);
        if (userName == null || userName == "")
        {
            SceneManager.LoadScene("Login scene");
        }
        else
        {
            SceneManager.LoadScene("Market download scene");
        }
    }

    public void reset()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("World choice scene");
    }
}
