using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public enum ColorGameState
{
    WAITING,
    START_GAME,
    SET_COLOR,
    SHOW_COLORS,
    TIME_TO_MOVE,
    TIME_UP,
    ROUND_END,
    TIME_BETWEEN_ROUNDS,
    WINNER,
    END_GAME,
    AUTOPLAY
}

public class ColorGameScene : MonoBehaviour
{
    public ColorGameState colorGameState;
    public GameObject colorPrefab;
    public GameObject choosedColorPrefab;
    public GameObject playersPrefab;
    public GameObject TimeUpPrefab;
    public GameObject countdownPrefab;
    public GameObject particlesPrefab;
    public GameObject numberOfParticipants;
    public GameObject timePrefab;

    public GameObject startButton;

    public GameObject circleParticlePrefab;
    public GameObject rectangleParticlePrefab;

    public GameObject colorsContainer;
    public List<GameObject> ColorsList = new List<GameObject>();
    public List<GameObject> PlayersList = new List<GameObject>();
    public List<GameObject> ParticlesList = new List<GameObject>();

    int currentRound = 1;
    public int maxRange = 5;
    public int[,] ColorGrid = new int[6, 5]; //30
    public bool[,] ColorGridBool = new bool[6, 5]; //30
    char[] possibleSolutionsX = { 'a', 'b', 'c', 'd', 'e', 'f' };
    char[] possibleSolutionsY = { '1', '2', '3', '4', '5' };

    int choosedColor;

    bool isColorChoosed = false;

    float showColorsTime;

    float startTime = 3f;
    float maxStartTime = 3f;

    float killTime = 3f;
    float maxKillTime = 3f;

    float betweenRoundsTime = 3f;
    float maxBetweenRoundsTime = 3f;

    float groundTime = 2.5f;
    float maxGroundTime = 2.5f;

    float movingTime = 18f;
    float maxMovingTime = 18f;

    float timeUpTime = 2f;
    float maxTimeUpTime = 2f;

    float winnerTime = 5f;
    float maxWinnerTime = 5f;

    bool ShownTime = false;

    Color[] colors = {
        new Color(254 / 255f, 121 / 255f, 104 / 255f, 1),
        new Color(155 / 255f, 219 / 255f, 231 / 255f, 1),
        new Color(255 / 255f, 255 / 255f, 159 / 255f, 1),
        new Color(139 / 255f, 192 / 255f, 162 / 255f, 1),
        new Color(189 / 255f, 197 / 255f, 234 / 255f, 1),
        new Color(255 / 255f, 183 / 255f, 135 / 255f, 1),
        new Color(242 / 255f, 157 / 255f, 180 / 255f, 1),
        new Color(222 / 255f, 239 / 255f, 194 / 255f, 1),
    };

    // Color pos: -573 384 0
    // Color size: 228 120

    void Start()
    {
        if (System.Convert.ToBoolean(PlayerPrefs.GetInt("autoplay")))
        {
            startButton.SetActive(false);
        }

        GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
    }

    void Update()
    {
        switch (colorGameState)
        {
            case ColorGameState.WAITING:
                numberOfParticipants.SetActive(true);
                timePrefab.SetActive(false);
                numberOfParticipants.GetComponent<TMP_Text>().text = PlayersList.Count.ToString() + "/50 players";
                numberOfParticipants.transform.Find("Outline").GetComponent<TMP_Text>().text = numberOfParticipants.GetComponent<TMP_Text>().text;

                break;

            case ColorGameState.START_GAME:
                // COUNTDOWN
                if (startTime == maxStartTime)
                {
                    numberOfParticipants.SetActive(false);
                    StartCoroutine(StartCountdown());
                    startTime -= Time.deltaTime;
                }
                else if (startTime > 0)
                {
                    startTime -= Time.deltaTime;
                }
                else
                {
                    startTime = maxStartTime;
                    colorGameState = ColorGameState.SET_COLOR;
                }
                break;

            case ColorGameState.SET_COLOR:
                SetColorGrid();
                showColorsTime = maxGroundTime * 6;
                betweenRoundsTime = maxBetweenRoundsTime;
                killTime = maxKillTime;
                movingTime = maxMovingTime;
                timeUpTime = maxTimeUpTime;
                ShownTime = false;
                groundTime = -1;
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        ColorGridBool[i, j] = false;
                    }
                }

                colorGameState = ColorGameState.SHOW_COLORS;
                break;

            case ColorGameState.SHOW_COLORS:
                if (showColorsTime >= 0 && groundTime >= 0)
                {
                    showColorsTime -= Time.deltaTime;
                    groundTime -= Time.deltaTime;
                }
                else if (showColorsTime > 0 && groundTime < 0)
                {
                    ShowFloor();
                }
                else
                {
                    for (int j = 0; j < ColorsList.Count; j++)
                    {
                        ColorsList[j].SetActive(false);
                    }
                    timePrefab.SetActive(true);
                    colorGameState = ColorGameState.TIME_TO_MOVE;
                }
                break;

            case ColorGameState.TIME_TO_MOVE:

                timePrefab.GetComponent<TMP_Text>().text = "Time left: " + (int)movingTime;

                if (!isColorChoosed)
                {
                    ChooseColor();
                    isColorChoosed = true;
                }

                if (movingTime >= 0)
                {
                    movingTime -= Time.deltaTime;
                }
                else
                {
                    isColorChoosed = false;
                    timePrefab.SetActive(false);
                    colorGameState = ColorGameState.TIME_UP;
                }
                break;
            case ColorGameState.TIME_UP:
                if (!ShownTime)
                {
                    ShownTime = true;
                    Instantiate(TimeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
                }
                if (timeUpTime == maxTimeUpTime)
                {
                    for (int j = 0; j < PlayersList.Count; j++)
                    {
                        PlayersList[j].GetComponent<PlayerController>().Stop();
                    }

                    timeUpTime -= Time.deltaTime;
                }
                else if (timeUpTime >= 0)
                {
                    timeUpTime -= Time.deltaTime;
                }
                else
                {
                    colorGameState = ColorGameState.ROUND_END;
                }
                break;

            case ColorGameState.ROUND_END:
                if (killTime == maxKillTime)
                {
                    for (int i = 0; i < ColorsList.Count; i++)
                    {
                        if (ColorsList[i].GetComponent<SpriteRenderer>().color == GameObject.Find("Choosed Color").GetComponent<SpriteRenderer>().color)
                        {
                            ColorsList[i].SetActive(true);
                        }
                    }

                    KillPlayers();
                    SpawnFire();

                    killTime -= Time.deltaTime;
                }
                else if (killTime >= 0)
                {
                    killTime -= Time.deltaTime;
                }
                else
                {
                    Destroy(GameObject.Find("Choosed Color"));
                    ClearList(ColorsList);
                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        ParticlesList[i].transform.Find("FireParticle1").GetComponent<ParticleSystem>().Stop();
                        ParticlesList[i].transform.Find("FireParticle2").GetComponent<ParticleSystem>().Stop();
                    }

                    if (PlayersList.Count <= 1)
                    {
                        colorGameState = ColorGameState.WINNER;
                    }
                    else if (PlayersList.Count == 0)
                    {
                        colorGameState = ColorGameState.WINNER;
                    }
                    else
                    {
                        colorGameState = ColorGameState.TIME_BETWEEN_ROUNDS;
                    }
                }
                break;

            case ColorGameState.TIME_BETWEEN_ROUNDS:
                if (betweenRoundsTime >= 0)
                    betweenRoundsTime -= Time.deltaTime;
                else if (currentRound < 10)
                {
                    currentRound += 1;
                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        Destroy(ParticlesList[i]);
                    }
                    ParticlesList.Clear();
                    colorGameState = ColorGameState.SET_COLOR;
                }
                else
                {
                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        Destroy(ParticlesList[i]);
                    }
                    ParticlesList.Clear();
                    colorGameState = ColorGameState.WINNER;
                }
                break;

            case ColorGameState.WINNER:
                if (winnerTime == maxWinnerTime && PlayersList.Count > 0)
                {
                    //winner text

                    string winnertext = "CONGRATS";

                    GameObject go = Instantiate(TimeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
                    for (int i = 0; i < PlayersList.Count; i++)
                    {
                        winnertext += "\n" + GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers[i].username;
                    }

                    go.GetComponent<TMP_Text>().text = winnertext;
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = winnertext;

                    SpawnConfetti();
                    winnerTime -= Time.deltaTime;
                }
                else if (winnerTime == maxWinnerTime && PlayersList.Count == 0)
                {
                    //no winner text

                    GameObject go = Instantiate(TimeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
                    go.GetComponent<TMP_Text>().text = "EVERYBODY DIES";
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = "EVERYBODY DIES";
                    winnerTime -= Time.deltaTime;
                }
                else if (winnerTime < maxWinnerTime && winnerTime > 0)
                {
                    winnerTime -= Time.deltaTime;
                }
                else
                {
                    colorGameState = ColorGameState.END_GAME;
                    winnerTime = maxWinnerTime;
                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        Destroy(ParticlesList[i]);
                    }
                    ParticlesList.Clear();
                }
                break;

            case ColorGameState.END_GAME:

                for (int i = 0; i < GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.Count; i++)
                {
                    Destroy(GameObject.Find(GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers[i].userID.ToString()));
                }
                GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.Clear();
                PlayersList.Clear();

                if (GameObject.Find("GameManager").GetComponent<GameManager>().autoplay)
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().PickRandomGame();
                    colorGameState = ColorGameState.AUTOPLAY;
                }
                else
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                    startButton.SetActive(true);
                    numberOfParticipants.SetActive(true);
                    colorGameState = ColorGameState.WAITING;
                }
                break;
            case ColorGameState.AUTOPLAY:
                break;
        }
    }

    void SetColorGrid()
    {
        if (currentRound % 3 == 0)
        {
            maxRange += 1;
        }

        for (int i = 0; i < ColorGrid.GetLength(0); i++)
        {
            for (int j = 0; j < ColorGrid.GetLength(1); j++)
            {
                ColorGrid[i, j] = Random.Range(1, maxRange);
                GameObject go = Instantiate(colorPrefab, new Vector3(-3.22f + (2.28f * i), 3.48f - (1.92f * j), 0), Quaternion.identity);
                go.transform.SetParent(colorsContainer.transform, false);
                go.GetComponent<SpriteRenderer>().color = colors[ColorGrid[i, j] - 1];
                go.SetActive(false);
                ColorsList.Add(go);
            }
        }
    }

    void ChooseColor()
    {
        int choosedColorX = Random.Range(0, ColorGrid.GetLength(0));
        int choosedColorY = Random.Range(0, ColorGrid.GetLength(1));
        choosedColor = ColorGrid[choosedColorX, choosedColorY] - 1;

        GameObject go = Instantiate(choosedColorPrefab, new Vector3(-5.21f, 3.52f, 0), Quaternion.identity);
        go.GetComponent<SpriteRenderer>().color = colors[choosedColor];
        go.name = "Choosed Color";

        for (int i = 0; i < ColorsList.Count; i++)
        {
            if (ColorsList[i].GetComponent<SpriteRenderer>().color == colors[choosedColor])
            {
                ColorsList[i].name = "Safe spot";
            }
            else
            {
                ColorsList[i].name = "Die spot";
            }
        }
    }

    void ShowFloor()
    {
        int count = 0;

        for (int k = 0; k < 6; k++)
        {
            for (int f = 0; f < 5; f++)
            {
                if (ColorGridBool[k, f] == true)
                {
                    count++;
                }
            }

            if (count >= 30)
            {
                for (int j = 0; j < ColorsList.Count; j++)
                {
                    ColorsList[j].SetActive(false);
                }
                groundTime = maxGroundTime;
                return;
            }
        }

        for (int j = 0; j < ColorsList.Count; j++)
        {
            ColorsList[j].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            int tempX = Random.Range(0, 6);
            int tempY = Random.Range(0, 5);

            if (ColorGridBool[tempX, tempY] == true)
            {
                i--;
            }
            else
            {
                ColorGridBool[tempX, tempY] = true;
                ColorsList[tempX * 5 + tempY].SetActive(true);
            }
        }

        groundTime = maxGroundTime;
    }

    void ClearList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Destroy(list[i]);
        }

        list.Clear();
    }

    public void SpawnPlayer(ChatUser user)
    {
        Debug.Log("Creating player");
        GameObject go = Instantiate(playersPrefab, new Vector3(12, 5, 0), Quaternion.identity);
        go.name = user.userID.ToString();
        go.transform.Find("UserName").GetComponent<TMP_Text>().text = user.username;
        go.transform.Find("Outline").GetComponent<TMP_Text>().text = user.username;
        Color newCol;
        ColorUtility.TryParseHtmlString(user.color, out newCol);
        go.transform.Find("UserName").GetComponent<TMP_Text>().color = newCol;

        PlayersList.Add(go);
    }

    void KillPlayers()
    {
        for (int i = 0; i < PlayersList.Count; i++)
        {
            Vector2Int temp = PlayersList[i].GetComponent<PlayerController>().GetGridPosition();

            if (temp.x == -1 || temp.y == -1)
            {
                GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.RemoveAt(i);
                PlayersList[i].GetComponent<PlayerController>().KillPlayer();
                PlayersList.RemoveAt(i);
                i--;
            }
            else
            {
                if ((ColorGrid[temp.x, temp.y] - 1) != choosedColor)
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.RemoveAt(i);
                    Debug.Log("Muere");
                    PlayersList[i].GetComponent<PlayerController>().KillPlayer();
                    PlayersList.RemoveAt(i);
                    i--;
                }
                else
                {
                    Debug.Log("Salvado");
                }
            }

        }
    }

    void SpawnFire()
    {
        for (int i = 0; i < ColorGrid.GetLength(0); i++)
        {
            for (int j = 0; j < ColorGrid.GetLength(1); j++)
            {
                if ((ColorGrid[i, j] - 1) != choosedColor)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        GameObject FireParticle = Instantiate(particlesPrefab, new Vector3(Random.Range(-3.22f + (2.28f * i) - 1, -3.22f + (2.28f * i) + 1), Random.Range(3.48f - (1.92f * j) + 0.8f, 3.48f - (1.92f * j) - 0.8f), 0), Quaternion.identity);
                        ParticlesList.Add(FireParticle);
                    }
                }
            }
        }
    }

    void SpawnConfetti()
    {
        GameObject confettiParticle = Instantiate(rectangleParticlePrefab, new Vector3(9.86f, -8, 0), Quaternion.Euler(-74.192f, -90, 90));
        ParticlesList.Add(confettiParticle);
        GameObject confettiParticle2 = Instantiate(circleParticlePrefab, new Vector3(9.86f, -8, 0), Quaternion.Euler(-74.192f, -90, 90));
        ParticlesList.Add(confettiParticle2);
        GameObject confettiParticle3 = Instantiate(rectangleParticlePrefab, new Vector3(-4.95f, -8, 0), Quaternion.Euler(-106.642f, -90, 90));
        ParticlesList.Add(confettiParticle3);
        GameObject confettiParticle4 = Instantiate(circleParticlePrefab, new Vector3(-4.95f, -8, 0), Quaternion.Euler(-106.642f, -90, 90));
        ParticlesList.Add(confettiParticle4);
    }

    IEnumerator StartCountdown()
    {
        GameObject go = Instantiate(countdownPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
        Destroy(go, 1f);
        yield return new WaitForSeconds(1f);
        GameObject go2 = Instantiate(countdownPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
        go2.GetComponent<TMP_Text>().text = "2";
        go2.transform.Find("Outline").GetComponent<TMP_Text>().text = "2";
        Destroy(go2, 1f);
        yield return new WaitForSeconds(1f);
        GameObject go3 = Instantiate(countdownPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
        go3.GetComponent<TMP_Text>().text = "1";
        go3.transform.Find("Outline").GetComponent<TMP_Text>().text = "1";
        Destroy(go3, 1f);
        yield return new WaitForSeconds(1f);
        StopAllCoroutines();
    }

    public void ProcessMessage(ChatUser user)
    {
        if (!GameObject.Find(user.userID.ToString()).GetComponent<PlayerController>().inGame)
        {
            return;
        }
        else
        {
            int pos2sendX = 10;
            int pos2sendY = 10;

            for (int i = 0; i < possibleSolutionsX.Length; i++)
            {
                if (user.message[1] == possibleSolutionsX[i])
                {
                    pos2sendX = i;
                }
            }

            for (int j = 0; j < possibleSolutionsY.Length; j++)
            {
                if (user.message[2] == possibleSolutionsY[j])
                {
                    pos2sendY = j;
                }
            }

            if (pos2sendX != 10 && pos2sendY != 10)
            {
                GameObject.Find(user.userID.ToString()).GetComponent<PlayerController>().SetTargetPos(pos2sendX, pos2sendY);
            }
        }

    }
}
