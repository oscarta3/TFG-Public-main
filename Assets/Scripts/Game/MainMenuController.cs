using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    GameObject selectedGameOutline;
    GameObject selectedGame;

    bool autoplayBool = false;

    public GameObject gamesContainer;
    public GameObject autoplay;
    public GameObject cloudPrefab;
    public GameObject checkSettings;

    public List<GameObject> CloudsContainer;

    public Sprite[] gamemodesSprites;
    public Sprite[] cloudsSprites;

    public void Start()
    {
        selectedGameOutline = GameObject.Find("SelectedGameOutline");
        selectedGame = GameObject.Find("SelectedGame");
        gamesContainer = GameObject.Find("GamesContainer");
        autoplay = GameObject.Find("AutoplayButton");
        selectedGameOutline.transform.localPosition = new Vector3(-246f + ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType * 160), 85, 0);
        selectedGame.GetComponent<Image>().sprite = gamemodesSprites[(int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType];
        GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.MAIN_MENU;
        checkSettings.SetActive(false);
        CreateCould();
    }

    public void ChangeGameMode(int gameType)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().gameType = (GameType)gameType;
        selectedGameOutline.transform.localPosition = new Vector3(-246f + ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType * 160), 85, 0);
        selectedGame.GetComponent<Image>().sprite = gamemodesSprites[(int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType];
    }

    void CreateCould()
    {
        float[] side = new float[2] { -12.5f, 12.5f };

        if (CloudsContainer.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    GameObject go = Instantiate(cloudPrefab, new Vector3(0, 2.16f, 0), Quaternion.identity);
                    go.GetComponent<SpriteRenderer>().sprite = cloudsSprites[Random.Range(0, cloudsSprites.Length)];
                    CloudsContainer.Add(go);
                }
                else
                {
                    GameObject go = Instantiate(cloudPrefab, new Vector3(side[Random.Range(0, 2)], Random.Range(0.26f, 4.3f), 0), Quaternion.identity);
                    go.GetComponent<SpriteRenderer>().sprite = cloudsSprites[Random.Range(0, cloudsSprites.Length)];
                    CloudsContainer.Add(go);
                }
            }
        }
        else if (CloudsContainer.Count < 5)
        {
            GameObject go = Instantiate(cloudPrefab, new Vector3(side[Random.Range(0, 2)], Random.Range(0.26f, 4.3f), 0), Quaternion.identity);
            go.GetComponent<SpriteRenderer>().sprite = cloudsSprites[Random.Range(0, cloudsSprites.Length)];
            CloudsContainer.Add(go);
        }
    }

    public void DestroyCloud()
    {
        for (int i = 0; i < CloudsContainer.Count; i++)
        {
            if (CloudsContainer[i].transform.position == CloudsContainer[i].GetComponent<CloudController>().targetPos)
            {
                Destroy(CloudsContainer[i]);
                CloudsContainer.RemoveAt(i);
                CreateCould();
            }
        }
    }

    public void OpenGameSelection()
    {
        Animator animator = gamesContainer.GetComponent<Animator>();
        if (animator != null)
        {
            bool isOpen = animator.GetBool("Open");

            animator.SetBool("Open", !isOpen);
        }
    }

    public void Autoplay()
    {
        Animator animator = autoplay.GetComponent<Animator>();
        if (animator != null)
        {
            bool isON = animator.GetBool("ON");

            animator.SetBool("ON", !isON);

            autoplayBool = !isON;
        }
    }

    public void StartGame()
    {
        if (GameObject.Find("TwitchManager").GetComponent<TwitchChat>().user == "" || GameObject.Find("TwitchManager").GetComponent<TwitchChat>().OAuth == "")
        {

            checkSettings.SetActive(true);
        }
        else
        {
            if ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType == 0)
            {
                SceneManager.LoadScene("TriviaScene");
                GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                PlayerPrefs.SetInt("GameType", 0);
            }
            else if ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType == 1)
            {
                SceneManager.LoadScene("ColorGameScene");
                GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                GameObject.Find("GameManager").GetComponent<GameManager>().gameType = GameType.COLORGAME;
                PlayerPrefs.SetInt("GameType", 1);
            }
            else if ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType == 2)
            {
                SceneManager.LoadScene("HangmanScene");
                GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                GameObject.Find("GameManager").GetComponent<GameManager>().gameType = GameType.HANGMAN;
                PlayerPrefs.SetInt("GameType", 2);
            }
            else if ((int)GameObject.Find("GameManager").GetComponent<GameManager>().gameType == 3)
            {
                SceneManager.LoadScene("CountingScene");
                GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                GameObject.Find("GameManager").GetComponent<GameManager>().gameType = GameType.HANGMAN;
                PlayerPrefs.SetInt("GameType", 3);
            }
        }


        if (autoplayBool)
        {
            PlayerPrefs.SetInt("autoplay", 1);
        }
        else
        {
            PlayerPrefs.SetInt("autoplay", 0);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
