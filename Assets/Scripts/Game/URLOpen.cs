using UnityEngine;
using System.Collections;

public class URLOpen : MonoBehaviour
{
    public void OpenURL()
     {
         Application.OpenURL("https://twitchapps.com/tmi/");
         Debug.Log("is this working?");
     }
}
