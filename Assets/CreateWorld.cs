using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateWorld : MonoBehaviour
{
    [SerializeField]
    public Text nameTaken;

    public void onSubmit()
    {
        InputField iF = FindObjectOfType<InputField>();
        var worldName = iF.text;

        ArrayList worlds = (ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds"));

        MyUtil.PrintArrayList(worlds);

        if (worlds.Contains(worldName))
        {
            //MyUtil.ShowAndroidToastMessage("The name '"+worldName+"' is already taken.");
            nameTaken.color = new Color(nameTaken.color.r, nameTaken.color.g, nameTaken.color.b,1);
        } 
        else
        {
            var gameLogic = FindObjectOfType<GameLogic>();
            gameLogic.setWorldName(worldName);
            SceneManager.LoadScene("Game");
        }
    }
}