using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

public class TwitchChat : MonoBehaviour
{
    GameManager gameManager;

    // Twitch channel variables
    public string user = ""; //
    public string OAuth = ""; //

    //Network variables
    public TcpClient twitchClient;
    StreamReader reader;
    StreamWriter writer;
    float pingCounter;
    float reconnectAfter;

    string[] accInfo = System.IO.File.ReadAllLines("Assets/Trivia/trivia.txt");

    public List<ChatUser> usersList = new List<ChatUser>();

    TriviaGameManager triviaGameManager;
    ColorGameScene colorGameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (GameObject.Find("TriviaManager") != null && gameManager.gameState != GameState.MAIN_MENU)
        {
            triviaGameManager = GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>();
        }
        else if (GameObject.Find("ColorGameManager") != null && gameManager.gameState != GameState.MAIN_MENU)
        {
            colorGameManager = GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>();
        }
        reconnectAfter = 60.0f;
        Connect();
    }

    void Start()
    {
        if (!Directory.Exists("DontShowChat/userinfo"))
        {
            Directory.CreateDirectory("DontShowChat/userinfo");
        }

        if (File.Exists("DontShowChat/userinfo/account.txt"))
        {
            LoadInfo();
        }

        Connect();

        gameManager.gameType = (GameType)PlayerPrefs.GetInt("GameType");

        if (GameObject.Find("TriviaManager") != null && gameManager.gameState != GameState.MAIN_MENU)
        {
            triviaGameManager = GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>();
        }
        else if (GameObject.Find("ColorGameManager") != null && gameManager.gameState != GameState.MAIN_MENU)
        {
            colorGameManager = GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>();
        }
    }

    void Update()
    {
        pingCounter += Time.deltaTime;
        if (pingCounter > 60)
        {
            writer.WriteLine("PING " + "irc.chat.twitch.tv");
            writer.Flush();
            pingCounter = 0;
        }
        if (!twitchClient.Connected)
        {
            Connect();
        }

        ReadChat();
    }

    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + OAuth);
        writer.WriteLine("NICK " + user.ToLower());
        writer.WriteLine("USER " + user.ToLower() + " 8 * :" + user.ToLower());
        writer.WriteLine("JOIN #" + user.ToLower());
        writer.WriteLine("CAP REQ :twitch.tv/commands twitch.tv/tags twitch.tv/membership");
        writer.Flush();
    }

    public ChatUser ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            //message received from twitch
            string message = reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                //Convert the message into a user
                //Debug.Log(message.ToLower());
                ChatUser userInfo = CreateUser(message.ToLower());

                //Show username + message in console (debug purpose)
                Debug.Log(userInfo.username + ": " + userInfo.message);

                switch (gameManager.gameState)
                {
                    case GameState.MAIN_MENU:
                        gameManager.CheckUser(userInfo);
                        break;

                    case GameState.WAITING_USERS:
                        if (gameManager.gameState == GameState.WAITING_USERS)
                        {
                            if (userInfo.message == "!play" && !gameManager.gameUsers.Any<ChatUser>((x => x.userID == userInfo.userID)))
                            {
                                gameManager.CheckUser(userInfo);
                                Debug.Log(userInfo.username + ": joined the game");
                                if (GameObject.Find("TriviaManager") != null)
                                {
                                    GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().ProcessMessage(userInfo.username, userInfo.message);
                                    if (userInfo.color == "")
                                    {
                                        GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().participantsNames.GetComponent<TMP_Text>().text += "<color=#ffffff>" + userInfo.username + "   </color>";
                                    }
                                    else
                                    {
                                        GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().participantsNames.GetComponent<TMP_Text>().text += "<color=" + userInfo.color + ">" + userInfo.username + "   </color>";
                                    }
                                }
                                else if (GameObject.Find("ColorGameManager") != null)
                                {
                                    GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().SpawnPlayer(userInfo);
                                }
                                else if (GameObject.Find("CountingManager") != null)
                                {
                                    GameObject.Find("CountingManager").GetComponent<CountingManager>().SpawnPlayer(userInfo);
                                }
                            }
                            else
                            {
                                if (GameObject.Find("TriviaManager") != null)
                                {

                                }
                                else if (GameObject.Find("ColorGameManager") != null)
                                {
                                    if (IsUserListed(userInfo) && userInfo.message.StartsWith("!"))
                                    {
                                        GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().ProcessMessage(userInfo);
                                    }
                                }
                            }

                        }
                        break;

                    case GameState.PLAYING:
                        if (GameObject.Find("TriviaManager") != null) //trivia
                        {
                            if (IsUserListed(userInfo) && userInfo.message.StartsWith("!"))
                            {
                                GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().ProcessMessage(userInfo.username, userInfo.message);
                            }
                        }
                        else if (GameObject.Find("ColorGameManager") != null) //color game
                        {
                            if (IsUserListed(userInfo) && userInfo.message.StartsWith("!") && colorGameManager.colorGameState != ColorGameState.ROUND_END && colorGameManager.colorGameState != ColorGameState.TIME_UP)
                            {
                                GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().ProcessMessage(userInfo);
                            }
                        }
                        else if (GameObject.Find("HangmanManager") != null) //hangman game
                        {
                            if (userInfo.message.StartsWith("!") && GameObject.Find("HangmanManager").GetComponent<HangmanManager>().hangmanState == HangmanState.PLAYING)
                            {
                                GameObject.Find("HangmanManager").GetComponent<HangmanManager>().ProcessMessage(userInfo);
                            }
                        }
                        else if (GameObject.Find("CountingManager") != null) //counting game
                        {
                            if (IsUserListed(userInfo) && userInfo.message.StartsWith("!") && GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.TIME_TO_ANSWER)
                            {
                                GameObject.Find("CountingManager").GetComponent<CountingManager>().ProcessMessage(userInfo);
                            }
                        }
                        break;
                }

                return userInfo;
            }
        }

        return null;
    }

    public void SetTwitchName()
    {
        user = GameObject.Find("TwitchName").GetComponent<TMP_InputField>().text;
        Debug.Log(user);

        if(File.Exists("DontShowChat/userinfo/account.txt"))
        {
            string[] textLines = File.ReadAllLines("DontShowChat/userinfo/account.txt");
            textLines[0] = user;
            textLines[1] = OAuth;
            File.WriteAllLines("DontShowChat/userinfo/account.txt", textLines);
        }
        else
        {
            var sr = File.CreateText("DontShowChat/userinfo/account.txt");
            sr.WriteLine(user);
            sr.WriteLine(OAuth);
            sr.Close();
        }
    }

    public void SetOauth()
    {
        OAuth = GameObject.Find("TwitchOauth").GetComponent<TMP_InputField>().text;
        Debug.Log(OAuth);
    }

    public void LoadInfo()
    {
        string[] textLines = File.ReadAllLines("DontShowChat/userinfo/account.txt");
        user = textLines[0];
        OAuth = textLines[1]; 
        Connect();
    }


    public ChatUser CreateUser(string message)
    {
        string[] stringSeparator;
        string[] result;
        string temp;
        int splitPoint;

        //string para saber que parámetros tiene el usuario
        string beforeChatMessage = message.Substring(0, message.IndexOf("tmi.twitch.tv"));
        //Debug.Log(beforeChatMessage);

        ChatUser user = new ChatUser();

        stringSeparator = new string[] { "user-id=" };
        result = message.Split(stringSeparator, System.StringSplitOptions.None);
        splitPoint = result[1].IndexOf(";", 0);
        temp = result[1].Substring(0, splitPoint);
        user.userID = int.Parse(temp);

        stringSeparator = new string[] { "display-name=" };
        result = message.Split(stringSeparator, System.StringSplitOptions.None);
        splitPoint = result[1].IndexOf(";", 0);
        temp = result[1].Substring(0, splitPoint);
        user.username = temp;

        stringSeparator = new string[] { "privmsg" };
        result = message.Split(stringSeparator, System.StringSplitOptions.None);
        splitPoint = result[1].IndexOf(":") + 1;
        temp = result[1].Substring(splitPoint);
        if (temp.EndsWith(" 󠀀"))
        {
            temp = temp.Replace(" 󠀀", "");
        }
        user.message = temp;

        stringSeparator = new string[] { "color=" };
        result = message.Split(stringSeparator, System.StringSplitOptions.None);
        splitPoint = result[1].IndexOf(";", 0);
        temp = result[1].Substring(0, splitPoint);
        user.color = temp;

        stringSeparator = new string[] { "subscriber=" };
        result = message.Split(stringSeparator, System.StringSplitOptions.None);
        splitPoint = result[1].IndexOf(";", 0);
        temp = result[1].Substring(0, splitPoint);
        int sub = int.Parse(temp);
        user.subscriber = sub == 1 ? true : false;

        if (beforeChatMessage.Contains("badges=broadcaster"))
        {
            user.userType = "broadcaster";
        }
        else if (beforeChatMessage.Contains("vip=1"))
        {
            user.userType = "vip";
        }
        else if (beforeChatMessage.Contains("mod=1"))
        {
            user.userType = "mod";
        }
        else
        {
            user.userType = "viewer";
        }

        return user;
    }

    bool IsUserListed(ChatUser user)
    {
        for (int i = 0; i < gameManager.gameUsers.Count; i++)
        {
            if (gameManager.gameUsers[i].userID == user.userID)
            {
                return true;
            }
        }

        return false;
    }
}