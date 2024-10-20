using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.UI;


public enum GameState
{
    MAIN_MENU,
    WAITING_USERS,
    PLAYING,
}

public enum GameType
{
    TRIVIA,
    COLORGAME,
    HANGMAN,
    COUNTING
}

public class GameManager : MonoBehaviour
{
    public List<ChatUser> gameUsers = new List<ChatUser>();
    public GameObject startingText;
    public GameState gameState;
    public GameType gameType;
    public GameObject autoplayLeftMenu;

    public Image black;
    public Animator fadeAnim;

    public float waitingTime = 30f;
    public float maxWaitingTime = 30f;

    public bool autoplay = false;
    bool isGameOn = false;

    public GameObject playersPrefab;
    public List<GameObject> PlayersList = new List<GameObject>();

    void Start()
    {
        autoplay = System.Convert.ToBoolean(PlayerPrefs.GetInt("autoplay"));
        Debug.Log(PlayerPrefs.GetInt("autoplay"));
        if (gameType != GameType.HANGMAN && gameState != GameState.MAIN_MENU)
        {
            startingText = Instantiate(startingText, new Vector3(-0.6f, -4.12f, 0), Quaternion.identity);
        }
        if (gameState != GameState.MAIN_MENU)
        {
            if (autoplay)
            {
                autoplayLeftMenu.GetComponent<Animator>().SetBool("isAutoplayON", true);
            }
            else
            {
                autoplayLeftMenu.GetComponent<Animator>().SetBool("isAutoplayON", false);
            }
        }
        //gameState = GameState.WAITING_USERS;
    }

    public void CheckUser(ChatUser user)
    {
        if (gameUsers.Any<ChatUser>((x => x.userID == user.userID)))
        {
            return;
        }
        else
        {
            if (gameState == GameState.MAIN_MENU)
            {
                //main menu players

                int[] side = new int[2] { -12, 12, };

                Debug.Log("Creating player");
                GameObject go = Instantiate(playersPrefab, new Vector3(side[Random.Range(0, 2)], Random.Range(-2f, -4.2f), 0), Quaternion.identity);
                go.name = user.userID.ToString();
                go.transform.Find("UserName").GetComponent<TMP_Text>().text = user.username;
                go.transform.Find("Outline").GetComponent<TMP_Text>().text = user.username;
                Color newCol;
                ColorUtility.TryParseHtmlString(user.color, out newCol);
                go.transform.Find("UserName").GetComponent<TMP_Text>().color = newCol;

                PlayersList.Add(go);
            }
            gameUsers.Add(user);
        }

    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.MAIN_MENU:
                if (gameType != GameType.HANGMAN)
                {
                    startingText.SetActive(false);
                }
                break;

            case GameState.WAITING_USERS:
                isGameOn = false;
                if (autoplay && gameType != GameType.HANGMAN)
                {
                    startingText.SetActive(true);


                    if (gameUsers.Count > 1 && waitingTime > 0)
                    {
                        startingText.GetComponent<TMP_Text>().text = "Game starting in: " + (int)waitingTime;
                        startingText.transform.Find("Outline").GetComponent<TMP_Text>().text = startingText.GetComponent<TMP_Text>().text;
                        waitingTime -= Time.deltaTime;
                    }
                    if (gameUsers.Count == 0)
                    {
                        startingText.GetComponent<TMP_Text>().text = "Waiting players to !play";
                        startingText.transform.Find("Outline").GetComponent<TMP_Text>().text = startingText.GetComponent<TMP_Text>().text;
                    }

                    if (waitingTime < 0)
                    {
                        gameState = GameState.PLAYING;
                        waitingTime = maxWaitingTime;
                    }
                }
                else
                {
                    if (gameType != GameType.HANGMAN)
                    {
                        startingText.SetActive(false);
                    }
                }
                break;

            case GameState.PLAYING:
                if (!isGameOn)
                {
                    if (gameType != GameType.HANGMAN)
                    {
                        startingText.SetActive(false);
                    }

                    if (gameType == GameType.TRIVIA)
                    {
                        GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().triviaState = TriviaState.START_GAME;
                    }
                    else if (gameType == GameType.COLORGAME)
                    {
                        GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().colorGameState = ColorGameState.START_GAME;
                    }
                    else if (gameType == GameType.HANGMAN)
                    {
                        GameObject.Find("HangmanManager").GetComponent<HangmanManager>().hangmanState = HangmanState.SET_WORD;
                    }
                    else if (gameType == GameType.COUNTING)
                    {
                        GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState = CountingGameState.START_GAME;
                    }

                    isGameOn = true;
                }
                break;

        }
    }

    public void StartGame()
    {
        if (gameType == GameType.TRIVIA)
        {
            if (gameUsers.Count > 0)
            {
                GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().startButton.SetActive(false);
                gameState = GameState.PLAYING;
            }
        }
        if (gameType == GameType.COLORGAME)
        {
            if (gameUsers.Count > 0)
            {
                GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().startButton.SetActive(false);
                gameState = GameState.PLAYING;
            }
        }
        if (gameType == GameType.HANGMAN)
        {

            GameObject.Find("HangmanManager").GetComponent<HangmanManager>().startButton.SetActive(false);
            GameObject.Find("HangmanManager").GetComponent<HangmanManager>().SetWord(GameObject.Find("InputWord").GetComponent<TMP_InputField>().text);
            GameObject.Find("InputWord").SetActive(false);
            gameState = GameState.PLAYING;
        }
        if (gameType == GameType.COUNTING)
        {

            GameObject.Find("CountingManager").GetComponent<CountingManager>().startButton.SetActive(false);
            gameState = GameState.PLAYING;
        }
    }

    public void Autoplay()
    {
        Animator animator = autoplayLeftMenu.GetComponent<Animator>();
        if (animator != null)
        {
            bool isON = animator.GetBool("isAutoplayON");

            animator.SetBool("isAutoplayON", !isON);

            autoplay = !isON;
        }

        if (autoplay)
        {
            PlayerPrefs.SetInt("autoplay", 1);

            if (gameType == GameType.TRIVIA)
            {
                GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().startButton.SetActive(false);
            }
            if (gameType == GameType.COLORGAME)
            {
                GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().startButton.SetActive(false);
            }
            if (gameType == GameType.HANGMAN)
            {
                GameObject.Find("HangmanManager").GetComponent<HangmanManager>().startButton.SetActive(false);
            }
            if (gameType == GameType.COUNTING)
            {
                GameObject.Find("CountingManager").GetComponent<CountingManager>().startButton.SetActive(false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("autoplay", 0);

            if (gameType == GameType.TRIVIA)
            {
                GameObject.Find("TriviaManager").GetComponent<TriviaGameManager>().startButton.SetActive(true);
            }
            if (gameType == GameType.COLORGAME)
            {
                GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().startButton.SetActive(true);
            }
            if (gameType == GameType.HANGMAN)
            {
                GameObject.Find("HangmanManager").GetComponent<HangmanManager>().startButton.SetActive(true);
            }
            if (gameType == GameType.COUNTING)
            {
                GameObject.Find("CountingManager").GetComponent<CountingManager>().startButton.SetActive(true);
            }
        }
    }

    public void GoToMenu()
    {
        StartCoroutine(Fading("Menu"));
        gameState = GameState.MAIN_MENU;
    }

    public void PickRandomGame()
    {
        int a = Random.Range(0, 3);
        if ((GameType)a == gameType)
        {

        }
        else
        {
            if (a == 0)
            {
                StartCoroutine(Fading("TriviaScene"));
                gameState = GameState.WAITING_USERS;
                gameType = GameType.TRIVIA;
                PlayerPrefs.SetInt("GameType", 0);
            }
            else if (a == 1)
            {
                StartCoroutine(Fading("ColorGameScene"));
                gameState = GameState.WAITING_USERS;
                gameType = GameType.COLORGAME;
                PlayerPrefs.SetInt("GameType", 1);
            }
            else if (a == 2)
            {
                StartCoroutine(Fading("CountingScene"));
                gameState = GameState.WAITING_USERS;
                gameType = GameType.COUNTING;
                PlayerPrefs.SetInt("GameType", 3);
            }
        }
    }

    public IEnumerator Fading(string sceneName)
    {
        fadeAnim.SetBool("Fade", true);
        yield return new WaitUntil(() => black.color.a == 1);
        SceneManager.LoadScene(sceneName);
    }
}
