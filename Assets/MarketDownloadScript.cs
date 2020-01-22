using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using Leguar.TotalJSON;
using UnityEngine.SceneManagement;
using System;

public class MarketDownloadScript : MonoBehaviour
{
    [SerializeField]
    private GameObject listContent;
    [SerializeField]
    private Button buttonPrefab;
    [SerializeField]
    private Text loadingText;
    [SerializeField]
    private Text emptyText;
    [SerializeField]
    private Text usernameText;

    private SocketIOComponent socket;
    private string userName;

    // Start is called before the first frame update
    void Start()
    {
        userName = PlayerPrefs.GetString("userName");
        Debug.Log("in Market, userName: "+userName);
        usernameText.text = "Username: " + userName;

        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b,1);
        emptyText.color = new Color(emptyText.color.r, emptyText.color.g, emptyText.color.b, 0);

        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();

        socket.On("connected", (SocketIOEvent e) => {
            Debug.Log("connected");
            socket.Emit("dbInit");  // Initializes database
            //socket.Emit("requireList");
        });

        // Recieve a list of world names on server
        socket.On("sendList", (SocketIOEvent e) => {
            Debug.Log("sendList");
            loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, 0);
            
            //Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data["worldList"].type));
            string[] worldList = ((JArray)JSON.ParseString(e.data.ToString())["worldList"]).AsStringArray();

            Debug.Log(worldList.Length);

            for (int i = 0; i < worldList.Length; i++)
            {
                string worldName = worldList[i];
                Button newButton = Instantiate(buttonPrefab);
                newButton.transform.SetParent(listContent.transform, false);
                newButton.GetComponentInChildren<Text>().text = worldName;

                newButton.onClick.RemoveAllListeners();
                newButton.onClick.AddListener(() =>
                {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data["worldName"] = worldName;
                    socket.Emit("download", new JSONObject(data));
                });
            }

            if(worldList.Length == 0)
            {
                emptyText.color = new Color(emptyText.color.r, emptyText.color.g, emptyText.color.b,1);
            }
        });

        // Recieve the required world data
        socket.On("sendWorld", (SocketIOEvent e) => {
            Debug.Log("sendWorld");

            //Debug.Log((e.data.ToString()));

            var data = JSON.ParseString(e.data.ToString());
            //Debug.Log(data.CreatePrettyString());

            char[] trim = { '"' };

            string worldName = ((JString)(data["worldName"])).CreateString().Trim(trim);
            string worldData = ((JString)(data["worldData"])).CreateString().Trim(trim);

            //Debug.Log("worldName: "+worldName+" worldData: "+worldData);

            string prefix = userName + ": ";

            Debug.Log("prefix = " + prefix);
            Debug.Log("worldName = " + worldName);
            Debug.Log("worldName.Substring(0, prefix.Length) = " + worldName.Substring(0, prefix.Length));
            Debug.Log("prefix.Equals(worldName.Substring(0, prefix.Length)) = " + prefix.Equals(worldName.Substring(0, prefix.Length)));

            if (prefix.Equals(worldName.Substring(0, prefix.Length)))
            {
                worldName = worldName.Substring(prefix.Length, worldName.Length - prefix.Length);
            }
            
            PlayerPrefs.DeleteKey("W:"+worldName);
            PlayerPrefs.SetString("W:"+worldName, worldData);

            var worlds = (ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds"));
            if (!worlds.Contains(worldName))
            {
                worlds.Add(worldName);
                PlayerPrefs.DeleteKey("worlds");
                PlayerPrefs.SetString("worlds", MyUtil.ObjectToString(worlds));
            }
            PlayerPrefs.Save();
            MyUtil.PrintPlayerPref();
        });
    }

    public void toUpload()
    {
        SceneManager.LoadScene("Market upload scene");
    }
}
