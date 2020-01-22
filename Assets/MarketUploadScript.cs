using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using Leguar.TotalJSON;
using UnityEngine.SceneManagement;

public class MarketUploadScript : MonoBehaviour
{
    [SerializeField]
    private GameObject listContent;
    [SerializeField]
    private Button buttonPrefab;
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

        emptyText.color = new Color(emptyText.color.r, emptyText.color.g, emptyText.color.b, 0);

        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();

        var worlds = new ArrayList();

        if (PlayerPrefs.HasKey("worlds"))
        {
            worlds = (ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds"));
        }
        else
        {
            PlayerPrefs.SetString("worlds", MyUtil.ObjectToString(worlds));
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetString("worlds", MyUtil.ObjectToString(worlds));
        PlayerPrefs.Save();

        for (int i = 0; i < worlds.Count; i++)
        {
            var worldName = (string)worlds[i];
            Button newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(listContent.transform, false);
            newButton.GetComponentInChildren<Text>().text = worldName;

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["worldName"] = userName+": "+worldName;
                data["worldData"] = PlayerPrefs.GetString("W:"+worldName);
                socket.Emit("upload", new JSONObject(data));
            });
        }

        if (worlds.Count == 0)
        {
            emptyText.color = new Color(emptyText.color.r, emptyText.color.g, emptyText.color.b, 1);
        }
    }

    public void toDownload()
    {
        SceneManager.LoadScene("Market download scene");
    }
}
