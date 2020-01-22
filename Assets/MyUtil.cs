using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MyUtil : MonoBehaviour
{
    public static string ObjectToString(object obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            new BinaryFormatter().Serialize(ms, obj);
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public static object StringToObject(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
        {
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return new BinaryFormatter().Deserialize(ms);
        }
    }

    public static void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    public static void PrintArrayList(ArrayList ar)
    {
        Debug.Log("Printing ArrayList:");
        for(int i=0; i<ar.Count; i++)
        {
            Debug.Log((string)ar[i]);
        }
        Debug.Log("Printing done.");
    }

    public static void PrintPlayerPref()
    {
        Debug.Log("Printing playerpref:");
        ArrayList arr = (ArrayList)StringToObject(PlayerPrefs.GetString("worlds"));
        for (int i = 0; i<arr.Count; i++)
        {
            Debug.Log("world name: ["+arr[i]+"] data: ["+PlayerPrefs.GetString("W:"+arr[i])+"]");
        }
        Debug.Log("Printing done.");
    }
}
