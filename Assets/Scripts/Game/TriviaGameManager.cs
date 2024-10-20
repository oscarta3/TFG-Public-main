using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
//using Random = UnityEngine.Random;


public enum TriviaState
{
    WAITING,
    START_GAME,
    SET_QUESTION,
    CORRECT_ANSWER,
    ANSWERING,
    QUESTION_RESULTS,
    GAME_END,
    AUTOPLAY
}

public struct TriviaUsers
{
    public string name;
    public string answer;
    public float lastAnswerTime;
    public float points;
}

public class TriviaGameManager : MonoBehaviour
{
    public GameManager _gameManager;
    public GameObject participantsGO;
    public GameObject answeringGO;
    public GameObject betweenQuestionsGO;
    public GameObject questionResultsGO;
    public GameObject endGameGO;
    public GameObject correctAnswers;

    public GameObject startButton;
    public GameObject participantsNames;

    public TriviaState triviaState;
    public GameObject Question, AnswerA, AnswerB, AnswerC, AnswerD;
    public GameObject questionTimer;
    public GameObject Top1, Top2, Top3, Top1Points, Top2Points, Top3Points;
    string[] textLines = System.IO.File.ReadAllLines("Assets/Trivia/trivia.txt");
    int maxQuestions = 124;
    int questionsLeft = 10;
    float timeBetweenQuestions = 5f;
    float timeToAnswer = 15f;
    float timeQuestionResults = 5f;

    char[] possibleSolutions = { 'a', 'b', 'c', 'd' };
    int currentAnswerIndex;

    public List<TriviaUsers> triviaPlayers = new List<TriviaUsers>();

    public List<int> AlreadyAsked = new List<int>();

    void Start()
    {
        participantsGO = GameObject.Find("WaitingUsers");
        answeringGO = GameObject.Find("OptionsScreen");
        betweenQuestionsGO = GameObject.Find("BetweenRounds");
        questionResultsGO = GameObject.Find("Top3Screen");
        endGameGO = GameObject.Find("EndScreen");
        correctAnswers = GameObject.Find("CorrectContainer");
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameManager.gameState = GameState.WAITING_USERS;
        triviaState = TriviaState.WAITING;

        if (System.Convert.ToBoolean(PlayerPrefs.GetInt("autoplay")))
        {
            startButton.SetActive(false);
        }
    }

    void Update()
    {
        switch (triviaState)
        {
            case TriviaState.WAITING:
                participantsNames.SetActive(true);
                participantsGO.SetActive(true);
                endGameGO.SetActive(false);
                answeringGO.SetActive(false);
                betweenQuestionsGO.SetActive(false);
                questionResultsGO.SetActive(false);
                correctAnswers.SetActive(false);
                break;

            case TriviaState.START_GAME:
                participantsNames.SetActive(false);
                participantsGO.SetActive(false);
                answeringGO.SetActive(true);
                betweenQuestionsGO.SetActive(false);
                questionResultsGO.SetActive(false);
                endGameGO.SetActive(false);
                correctAnswers.SetActive(false);
                triviaState = TriviaState.SET_QUESTION;
                break;

            case TriviaState.SET_QUESTION:
                SetQuestion();
                timeBetweenQuestions = 3f;
                questionResultsGO.SetActive(false);
                triviaState = TriviaState.ANSWERING;
                break;

            case TriviaState.CORRECT_ANSWER:
                if (timeBetweenQuestions == 3f)
                {
                    questionResultsGO.SetActive(false);
                    correctAnswers.transform.Find("Option1").GetComponent<SpriteRenderer>().color = new Color(255 / 255f, 81 / 255f, 81 / 255f, 1); 
                    correctAnswers.transform.Find("Option2").GetComponent<SpriteRenderer>().color = new Color(255 / 255f, 81 / 255f, 81 / 255f, 1);
                    correctAnswers.transform.Find("Option3").GetComponent<SpriteRenderer>().color = new Color(255 / 255f, 81 / 255f, 81 / 255f, 1);
                    correctAnswers.transform.Find("Option4").GetComponent<SpriteRenderer>().color = new Color(255 / 255f, 81 / 255f, 81 / 255f, 1);


                    Debug.Log(currentAnswerIndex);
                    GameObject.Find("Option" + (currentAnswerIndex + 1)).GetComponent<SpriteRenderer>().color = new Color(105 / 255f, 255 / 255f, 105 / 255f, 1);

                    // red new Color(255 / 255f, 81 / 255f, 81 / 255f, 1);
                    // green new Color(105 / 255f, 255 / 255f, 105 / 255f, 1);
                }
                else if (timeBetweenQuestions <= 0f)
                {
                    questionResultsGO.SetActive(true);
                    betweenQuestionsGO.SetActive(false);
                    correctAnswers.SetActive(false);
                    answeringGO.SetActive(false);
                    triviaState = TriviaState.QUESTION_RESULTS;
                }

                timeBetweenQuestions -= Time.deltaTime;
                break;

            case TriviaState.ANSWERING:
                if (timeToAnswer == 15f)
                {
                    answeringGO.SetActive(true);
                }
                else if (timeToAnswer <= 0f)
                {
                    timeToAnswer = 15f;
                    correctAnswers.SetActive(true);
                    questionResultsGO.SetActive(true);
                    questionsLeft -= 1;
                    SetPoints();
                    triviaState = TriviaState.CORRECT_ANSWER;
                }
                questionTimer.GetComponent<TMP_Text>().text = timeToAnswer.ToString("f2");
                timeToAnswer -= Time.deltaTime;
                break;

            case TriviaState.QUESTION_RESULTS:
                if (timeQuestionResults <= 0f && questionsLeft > 0)
                {
                    answeringGO.SetActive(true);
                    timeQuestionResults = 5f;
                    triviaState = TriviaState.SET_QUESTION;
                }
                else if (timeQuestionResults <= 0f && questionsLeft <= 0)
                {
                    timeQuestionResults = 5f;
                    triviaState = TriviaState.GAME_END;
                }
                else
                {
                    timeQuestionResults -= Time.deltaTime;
                }

                break;

            case TriviaState.GAME_END:
                if (triviaPlayers.Count > 0)
                {
                    participantsNames.GetComponent<TMP_Text>().text = "";
                    triviaPlayers.Clear();
                }
                if (GameObject.Find("GameManager").GetComponent<GameManager>().autoplay)
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().PickRandomGame();
                    triviaState = TriviaState.AUTOPLAY;
                }
                else
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                    startButton.SetActive(true);
                    triviaState = TriviaState.WAITING;
                }
                break;

            case TriviaState.AUTOPLAY:
                break;
        }
    }

    int GetQuestionNumber()
    {
        int rand = Random.Range(1, maxQuestions);

        //si el número de la pregunta no ha salido anteriormente, se usará esa pregunta
        if (AlreadyAsked.Contains(rand))
        {
            return GetQuestionNumber();
        }
        else
        {
            AlreadyAsked.Add(rand);
            return rand;
        }
    }

    public void SetQuestion()
    {
        int QuestionNumber = GetQuestionNumber();
        string[] answers = new string[4] { textLines[(QuestionNumber - 1) * 6 + 1], textLines[(QuestionNumber - 1) * 6 + 2], textLines[(QuestionNumber - 1) * 6 + 3], textLines[(QuestionNumber - 1) * 6 + 4] };

        Shuffle(answers);

        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i].EndsWith("*"))
            {
                currentAnswerIndex = i;
                answers[i] = answers[i].Remove(answers[i].Length - 1, 1);
                break;
            }
        }

        //No sumamos +1 al final pq textLines es un array de strings (empieza en el 0)
        Question.GetComponent<TMP_Text>().text = textLines[(QuestionNumber - 1) * 6];
        AnswerA.GetComponent<TMP_Text>().text = "!a: " + answers[0];
        AnswerB.GetComponent<TMP_Text>().text = "!b: " + answers[1];
        AnswerC.GetComponent<TMP_Text>().text = "!c: " + answers[2];
        AnswerD.GetComponent<TMP_Text>().text = "!d: " + answers[3];
    }

    void Shuffle(string[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public void ProcessMessage(string user, string message)
    {
        TriviaUsers temp = new TriviaUsers();

        temp.name = user;
        temp.answer = message;
        temp.lastAnswerTime = 15 - timeToAnswer;

        if (triviaPlayers.Any(TriviaUsers => TriviaUsers.name == temp.name))
        {
            for (int i = 0; i < triviaPlayers.Count; i++)
            {
                if (triviaPlayers[i].name == temp.name)
                {
                    temp.points = triviaPlayers[i].points;
                    triviaPlayers.RemoveAt(i);
                    triviaPlayers.Add(temp);
                }
            }
        }
        else
        {
            triviaPlayers.Add(temp);
        }
    }

    void SetPoints()
    {
        for (int i = 0; i < triviaPlayers.Count; i++)
        {
            if (triviaPlayers[i].answer.Length > 0)
            {
                if (triviaPlayers[i].answer.Substring(1) == possibleSolutions[currentAnswerIndex].ToString())
                {
                    TriviaUsers temp = triviaPlayers[i];
                    temp.points += 100 + (15f - temp.lastAnswerTime) * 10;
                    triviaPlayers[i] = temp;
                }
            }

            TriviaUsers temp2 = triviaPlayers[i];
            temp2.answer = "";
            triviaPlayers[i] = temp2;
        }

        SortList();
    }

    void SortList()
    {
        for (int i = 0; i < triviaPlayers.Count - 1; i++)
        {
            float n1 = triviaPlayers[i].points;
            float n2 = triviaPlayers[i + 1].points;

            if (n1 < n2)
            {
                TriviaUsers temp = triviaPlayers[i];
                triviaPlayers[i] = triviaPlayers[i + 1];
                triviaPlayers[i + 1] = temp;
                i = -1;
            }
        }

        if (triviaPlayers.Count >= 1)
        {
            Top1.GetComponent<TMP_Text>().text = triviaPlayers[0].name;
            Top1Points.GetComponent<TMP_Text>().text = triviaPlayers[0].points.ToString("f2");
        }
        if (triviaPlayers.Count >= 2)
        {
            Top2.GetComponent<TMP_Text>().text = triviaPlayers[1].name;
            Top2Points.GetComponent<TMP_Text>().text = triviaPlayers[1].points.ToString("f2");
        }
        if (triviaPlayers.Count >= 3)
        {
            Top3.GetComponent<TMP_Text>().text = triviaPlayers[2].name;
            Top3Points.GetComponent<TMP_Text>().text = triviaPlayers[2].points.ToString("f2");
        }
    }
}
