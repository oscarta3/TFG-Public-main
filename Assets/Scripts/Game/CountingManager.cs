using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public enum CountingGameState
{
    WAITING,
    START_GAME,
    STARING_ROUND,
    ROUND,
    TIME_TO_ANSWER,
    ROUND_END,
    END_GAME,
    AUTOPLAY
}

public struct AnswersContainer
{
    public string name;
    public string answer;
}

public class CountingManager : MonoBehaviour
{
    public CountingGameState countingGameState;
    public GameObject charactersPrefabs;
    public GameObject houseContainer;
    public GameObject startButton;
    public GameObject numberOfParticipants;

    public GameObject circleParticlePrefab;
    public GameObject rectangleParticlePrefab;
    public GameObject timeUpPrefab;

    public GameObject answer;

    public GameObject countdownPrefab;

    public List<GameObject> PlayersList;
    public List<GameObject> gameCharacters;
    public List<int> skinNum;
    public List<AnswersContainer> PlayersAnswers = new List<AnswersContainer>();
    public List<GameObject> ParticlesList = new List<GameObject>();

    int round = 1;
    int difficulty = 1;

    float countdown = 3f;
    float maxCountdown = 3f;

    float startRoundTime = 3f;
    float startMaxRoundTime = 3f;

    float answerTime = 15f;
    float maxAnswerTime = 15f;

    float roundTime = 20f;
    float maxRoundTime = 20f;

    float showResultsTime = 3f;
    float maxShowResultsTime = 3f;

    float inAndOutTime = 4f;
    float maxInAndOutTime = 4f;

    float endGameTime = 6f;
    float maxEndGameTime = 6f;

    float gridCellStartingX = -0.18f;
    float gridCellStartingY = -3f;

    void Start()
    {
        if (System.Convert.ToBoolean(PlayerPrefs.GetInt("autoplay")))
        {
            startButton.SetActive(false);
        }

        answer.SetActive(false);
        GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
    }

    void Update()
    {
        switch (countingGameState)
        {
            case CountingGameState.WAITING:
                numberOfParticipants.SetActive(true);
                numberOfParticipants.GetComponent<TMP_Text>().text = PlayersList.Count.ToString() + "/50 players";
                numberOfParticipants.transform.Find("Outline").GetComponent<TMP_Text>().text = numberOfParticipants.GetComponent<TMP_Text>().text;
                break;
            case CountingGameState.START_GAME:
                if (countdown == maxCountdown)
                {
                    StartCoroutine(StartCountdown());
                    SetAnswers();
                    StartRound();
                    numberOfParticipants.SetActive(false);
                    countdown -= Time.deltaTime;
                }
                else if (countdown >= 0)
                {
                    countdown -= Time.deltaTime;
                }
                else
                {
                    countdown = maxCountdown;
                    countingGameState = CountingGameState.STARING_ROUND;
                }
                break;
            case CountingGameState.STARING_ROUND:
                if (startRoundTime == startMaxRoundTime)
                {
                    houseContainer.transform.position = new Vector3(houseContainer.transform.position.x, houseContainer.transform.position.y, houseContainer.transform.position.z);
                    FallHouse();
                    startRoundTime -= Time.deltaTime;
                }
                else if (startRoundTime > 0)
                {
                    startRoundTime -= Time.deltaTime;
                }
                else
                {
                    startRoundTime = startMaxRoundTime;
                    inAndOutTime = 4.5f - round * 0.35f;
                    maxInAndOutTime = inAndOutTime;
                    for (int i = 0; i < gameCharacters.Count; i++)
                    {
                        gameCharacters[i].SetActive(false);
                    }
                    countingGameState = CountingGameState.ROUND;
                }
                break;
            case CountingGameState.ROUND:
                if (roundTime >= 0 && inAndOutTime >= 0)
                {
                    roundTime -= Time.deltaTime;
                    inAndOutTime -= Time.deltaTime;
                }
                else if (roundTime > 0 && inAndOutTime < 0)
                {
                    roundTime -= Time.deltaTime;
                    CharactersBehaviour();
                }
                else
                {
                    GameObject go = Instantiate(timeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
                    go.GetComponent<TMP_Text>().text = "Time to answer!";
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = "Time to answer!";

                    roundTime = maxRoundTime;
                    countingGameState = CountingGameState.TIME_TO_ANSWER;
                }

                break;
            case CountingGameState.TIME_TO_ANSWER:
                if (answerTime > 0)
                {
                    answerTime -= Time.deltaTime;
                }
                else
                {
                    if (round != 7)
                    {
                        round += 1;
                        difficulty += 1;
                    }
                    GameObject go = Instantiate(timeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);
                    go.GetComponent<TMP_Text>().text = "TIME UP!";
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = "TIME UP!";
                    answerTime = maxAnswerTime;
                    countingGameState = CountingGameState.ROUND_END;
                }
                break;
            case CountingGameState.ROUND_END:
                if (showResultsTime == maxShowResultsTime)
                {
                    UpHouse();
                    SortCharacters();
                    SetPoints();
                    showResultsTime -= Time.deltaTime;
                }
                else if (showResultsTime > 0)
                {
                    showResultsTime -= Time.deltaTime;
                }
                else
                {
                    answer.SetActive(false);
                    ClearLastRound();
                    showResultsTime = maxShowResultsTime;
                    if (PlayersList.Count > 1 && round < 7)
                    {
                        countingGameState = CountingGameState.START_GAME;
                    }
                    else
                    {
                        countingGameState = CountingGameState.END_GAME;
                    }
                }
                break;
            case CountingGameState.END_GAME:
                if (endGameTime == maxEndGameTime)
                {
                    if (PlayersList.Count >= 1)
                    {
                        //somebody won
                        SpawnConfetti();
                    }
                    endGameTime -= Time.deltaTime;
                }
                else if (endGameTime > 0)
                {
                    endGameTime -= Time.deltaTime;
                }
                else
                {
                    endGameTime = maxEndGameTime;

                    for(int j = 0; j < PlayersList.Count; j++)
                    {
                        Destroy(PlayersList[j]);
                    }
                    PlayersList.Clear();
                    GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.Clear();

                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        Destroy(ParticlesList[i]);
                    }
                    ParticlesList.Clear();

                    if (GameObject.Find("GameManager").GetComponent<GameManager>().autoplay)
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().PickRandomGame();
                        countingGameState = CountingGameState.AUTOPLAY;
                    }
                    else
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                        startButton.SetActive(true);
                        numberOfParticipants.SetActive(true);
                        countingGameState = CountingGameState.WAITING;
                    }
                }
                break;

                case CountingGameState.AUTOPLAY:
                break;
        }
    }

    void FallHouse()
    {
        houseContainer.GetComponent<Animator>().SetBool("Falling", true);
    }
    void UpHouse()
    {
        houseContainer.GetComponent<Animator>().SetBool("Falling", false);
    }

    void SetAnswers()
    {
        PlayersAnswers.Clear();

        for(int i = 0; i < PlayersList.Count; i++)
        {
            AnswersContainer temp;
            temp.name = PlayersList[i].name;
            temp.answer = "";

            PlayersAnswers.Add(temp);
        }
    }

    void ClearLastRound()
    {
        for (int i = 0; i < gameCharacters.Count; i++)
        {
            Destroy(gameCharacters[i]);
        }
        gameCharacters.Clear();
        skinNum.Clear();
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

    void StartRound()
    {
        if (round > 0)
        {
            int randCharacters = Random.Range(2, 6 + difficulty);

            Vector3 firstpos = new Vector3(-0.18f, -3, 0);
            int count = 0;
            int ymulti = 0;

            for (int i = 0; i < randCharacters; i++)
            {
                if (i < 4)
                {
                    GameObject go = Instantiate(charactersPrefabs, new Vector3(firstpos.x + 0.71f * i, firstpos.y + (ymulti * 0.71f), 0), Quaternion.identity);
                    go.transform.Find("UserName").gameObject.SetActive(false);
                    go.transform.Find("Outline").gameObject.SetActive(false);
                    go.GetComponent<PlayerController>().moveSpeed = 1f + round * 0.75f;
                }
                else
                {
                    count++;
                    GameObject go = Instantiate(charactersPrefabs, new Vector3(firstpos.x - 0.71f + (0.71f * (i - (4 + (ymulti - 1) * 6))), firstpos.y + (ymulti * 0.71f), 0), Quaternion.identity);
                    go.transform.Find("UserName").gameObject.SetActive(false);
                    go.transform.Find("Outline").gameObject.SetActive(false);
                    go.GetComponent<PlayerController>().moveSpeed = 1f + round * 0.75f;
                }
                if (i == 3)
                {
                    ymulti++;
                }
                if (count == 6)
                {
                    count = 0;
                    ymulti += 1;
                }
            }
        }
    }

    public void SpawnPlayer(ChatUser user)
    {
        Debug.Log("Creating player");
        GameObject go = Instantiate(charactersPrefabs, new Vector3(12, Random.Range(1.1f, 5), 0), Quaternion.identity);
        go.GetComponent<PlayerController>().playerType = PlayerType.COUNTING_PLAYER;
        go.name = user.username;
        go.transform.Find("UserName").GetComponent<TMP_Text>().text = user.username;
        go.transform.Find("Outline").GetComponent<TMP_Text>().text = user.username;
        Color newCol;
        ColorUtility.TryParseHtmlString(user.color, out newCol);
        go.transform.Find("UserName").GetComponent<TMP_Text>().color = newCol;

        PlayersList.Add(go);
    }

    public void ProcessMessage(ChatUser user)
    {
        AnswersContainer temp;

        temp.name = user.username;
        temp.answer = user.message.Substring(1);

        if (PlayersAnswers.Any(AnswersContainer => AnswersContainer.name == temp.name))
        {
            for (int i = 0; i < PlayersAnswers.Count; i++)
            {
                if (PlayersAnswers[i].name == temp.name)
                {
                    PlayersAnswers.RemoveAt(i);
                    PlayersAnswers.Add(temp);
                }
            }
        }
        else
        {
            PlayersAnswers.Add(temp);
        }
    }

    void CharactersBehaviour()
    {
        int inOrOut = Random.Range(0, 1 + difficulty);
        int inOrOut2 = Random.Range(0, 2);
        int cuantosSalen = Random.Range(1, 3 + difficulty);
        int cuantosEntran = Random.Range(1, 3 + difficulty);

        //entra o sale
        if (inOrOut == 0)
        {

            //solo salen
            if (inOrOut2 == 0)
            {
                if (gameCharacters.Count > 0 && gameCharacters.Count > cuantosSalen)
                {
                    CharacterLeaves(cuantosSalen);
                }
                else if (gameCharacters.Count > 0 && gameCharacters.Count < cuantosSalen)
                {
                    CharacterLeaves(gameCharacters.Count);
                }
                else
                {
                    CharacterJoins(cuantosEntran);
                }
            }
            else
            {
                Debug.Log("solo entran");
                CharacterJoins(cuantosEntran);
            }
        }
        // entran Y salen
        else
        {
            Debug.Log("entran " + cuantosEntran + " y salen " + cuantosSalen);
            if (gameCharacters.Count > 0 && gameCharacters.Count > cuantosSalen)
            {
                CharacterLeaves(cuantosSalen);
            }
            else
            {
                CharacterLeaves(gameCharacters.Count);
            }

            CharacterJoins(cuantosEntran);
        }

        inAndOutTime = maxInAndOutTime;
    }

    void CharacterJoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(charactersPrefabs, new Vector3(-2.5f - ((i + 1) * 0.8f), -6, 0), Quaternion.identity);
            if (round > 1)
            {
                go.GetComponent<PlayerController>().moveSpeed = 1f + round * 0.75f;
            }
            go.transform.Find("UserName").gameObject.SetActive(false);
            go.transform.Find("Outline").gameObject.SetActive(false);
            go.GetComponent<PlayerController>().playerType = PlayerType.COUNTING_ENTER;
        }
    }

    void CharacterLeaves(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Debug.Log("está saliendo uno");
            int index = Random.Range(0, gameCharacters.Count);
            gameCharacters[index].SetActive(true);
            if (round > 1)
            {
                gameCharacters[index].GetComponent<PlayerController>().moveSpeed = 1f + round * 0.75f;
            }
            gameCharacters[index].transform.position = new Vector3(3.25f - ((i + 1) * 0.6f), -2, 0);
            gameCharacters[index].GetComponent<PlayerController>().targetPos = new Vector3(10, -2, 0);
            gameCharacters[index].GetComponent<Animator>().SetInteger("SkinNum", skinNum[index]);
            gameCharacters[index].GetComponent<PlayerController>().playerType = PlayerType.COUNTING_LEAVE;
            gameCharacters.RemoveAt(index);
            skinNum.RemoveAt(index);
        }
    }

    void SetPoints()
    {
        answer.SetActive(true);
        answer.GetComponent<TMP_Text>().text = "There are " + gameCharacters.Count.ToString() + " characters in the house!";
        answer.transform.Find("Outline").GetComponent<TMP_Text>().text = answer.GetComponent<TMP_Text>().text;

        for (int i = 0; i < PlayersAnswers.Count; i++)
        {
            if (PlayersAnswers[i].answer != gameCharacters.Count.ToString())
            {
                for (int j = 0; j < PlayersList.Count; j++)
                {
                    if (PlayersList[j].name == PlayersAnswers[i].name)
                    {
                        Destroy(PlayersList[j]);
                        PlayersList.RemoveAt(j);
                        break;
                    }
                }
                for (int k = 0; k < GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.Count; k++)
                {
                    if (GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers[k].username == PlayersAnswers[i].name)
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.RemoveAt(k);
                    }
                }

                PlayersAnswers.RemoveAt(i);
                i -= 1;
            }
        }
    }

    void SortCharacters()
    {
        Vector3 firstpos = new Vector3(-0.18f, -3, 0);
        int count = 0;
        int ymulti = 0;

        for (int i = 0; i < gameCharacters.Count; i++)
        {
            gameCharacters[i].SetActive(true);

            if (i < 4)
            {
                gameCharacters[i].transform.position = new Vector3(firstpos.x + 0.71f * i, firstpos.y + (ymulti * 0.71f), 0);
                gameCharacters[i].GetComponent<PlayerController>().targetPos = gameCharacters[i].transform.position;
                gameCharacters[i].GetComponent<Animator>().SetInteger("SkinNum", skinNum[i]);
            }
            else
            {
                count++;
                gameCharacters[i].transform.position = new Vector3(firstpos.x - 0.71f + (0.71f * (i - (4 + (ymulti - 1) * 6))), firstpos.y + (ymulti * 0.71f), 0);
                gameCharacters[i].GetComponent<PlayerController>().targetPos = gameCharacters[i].transform.position;
                gameCharacters[i].GetComponent<Animator>().SetInteger("SkinNum", skinNum[i]);
            }
            if (i == 3)
            {
                ymulti++;
            }
            if (count == 6)
            {
                count = 0;
                ymulti += 1;
            }

        }
    }
}
