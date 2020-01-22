using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScript : MonoBehaviour
{
    [SerializeField]
    private Text noName;

    public void onSubmit()
    {
        InputField iF = FindObjectOfType<InputField>();
        var userName = iF.text;

        if (userName == "")
        {
            noName.color = new Color(noName.color.r, noName.color.g, noName.color.b,1);
        }
        else
        {
            PlayerPrefs.DeleteKey("userName");
            PlayerPrefs.SetString("userName", userName);
            SceneManager.LoadScene("Market download scene");
        }
    }
}
