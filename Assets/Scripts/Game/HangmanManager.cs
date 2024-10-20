using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UIElements;
using System.Text;

public enum HangmanState
{
    BEFORE_WAIT,
    WAITING,
    SET_WORD,
    PLAYING,
    CHOOSED_LETTER,
    WIN,
    LOSE,
    AUTOPLAY
}

public struct WordsContainer
{
    public string user;
    public string word;
}

public struct TopWordsStruct
{
    public string word;
    public int count;
}

public class HangmanManager : MonoBehaviour
{
    public HangmanState hangmanState;
    public GameObject startButton;

    public GameObject Part1, Part2, Part3, Part4, Part5, Part6, Part7, Part8, Part9, Part10;
    public GameObject circleParticlePrefab;
    public GameObject rectangleParticlePrefab;
    public GameObject inputField;

    public int lifePoints = 10;
    public string wordToGuess;

    List<WordsContainer> wordsContainers = new List<WordsContainer>();
    List<TopWordsStruct> topWords = new List<TopWordsStruct>();
    public List<GameObject> ParticlesList = new List<GameObject>();

    public GameObject TimeUpPrefab;

    float timeToChoose = 10f;
    float maxTimeToChoose = 10f;

    float endTime = 5f;
    float maxEndTime = 5f;

    void Start()
    {
        inputField = GameObject.Find("InputWord");
        Part1.SetActive(false);
        Part2.SetActive(false);
        Part3.SetActive(false);
        Part4.SetActive(false);
        Part5.SetActive(false);
        Part6.SetActive(false);
        Part7.SetActive(false);
        Part8.SetActive(false);
        Part9.SetActive(false);
        Part10.SetActive(false);
    }

    void Update()
    {
        switch (hangmanState)
        {
            case HangmanState.BEFORE_WAIT:
                lifePoints = 10;
                GameObject.Find("GameManager").GetComponent<GameManager>().gameState = GameState.WAITING_USERS;
                startButton.SetActive(true);
                inputField.SetActive(true);
                Part1.SetActive(false);
                Part2.SetActive(false);
                Part3.SetActive(false);
                Part4.SetActive(false);
                Part5.SetActive(false);
                Part6.SetActive(false);
                Part7.SetActive(false);
                Part8.SetActive(false);
                Part9.SetActive(false);
                Part10.SetActive(false);
                hangmanState = HangmanState.WAITING;

                break;
            case HangmanState.WAITING:
                //GameObject.Find("InputWord").SetActive(true);          
                break;

            case HangmanState.SET_WORD:
                string underlineTemp = "";
                string guessingTemp = "";

                for (int i = 0; i < wordToGuess.Length; i++)
                {
                    if (wordToGuess[i] != ' ')
                    {
                        GameObject.Find("Underline").GetComponent<TMP_Text>().text = underlineTemp += "_";
                        GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text = guessingTemp += "?";
                    }
                    else
                    {
                        GameObject.Find("Underline").GetComponent<TMP_Text>().text = underlineTemp += "-";
                        GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text = guessingTemp += " ";
                    }
                }

                hangmanState = HangmanState.PLAYING;

                break;

            case HangmanState.PLAYING:
                if (timeToChoose >= 0 && wordsContainers.Count != 0)
                {
                    timeToChoose -= Time.deltaTime;
                }
                if (timeToChoose < 0)
                {
                    hangmanState = HangmanState.CHOOSED_LETTER;
                    timeToChoose = maxTimeToChoose;
                }
                break;

            case HangmanState.CHOOSED_LETTER:
                TopWords();

                wordsContainers.Clear();
                topWords.Clear();

                break;

            case HangmanState.WIN:
                if (endTime == maxEndTime)
                {
                    GameObject go = Instantiate(TimeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);

                    go.GetComponent<TMP_Text>().text = "CHAT WINS";
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = "CHAT WINS";
                    SpawnConfetti();
                    endTime -= Time.deltaTime;
                }
                else if (endTime > 0)
                {
                    endTime -= Time.deltaTime;
                }
                else
                {
                    for (int i = 0; i < ParticlesList.Count; i++)
                    {
                        Destroy(ParticlesList[i]);
                    }
                    ParticlesList.Clear();
                    endTime = maxEndTime;
                    if (GameObject.Find("GameManager").GetComponent<GameManager>().autoplay)
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().PickRandomGame();
                        hangmanState = HangmanState.AUTOPLAY;
                    }
                    else
                    {

                        hangmanState = HangmanState.BEFORE_WAIT;
                    }
                }
                break;

            case HangmanState.LOSE:
                if (endTime == maxEndTime)
                {
                    GameObject go = Instantiate(TimeUpPrefab, new Vector3(2.48f, -0.44f, 0), Quaternion.identity);

                    go.GetComponent<TMP_Text>().text = "CHAT LOSES";
                    go.transform.Find("Outline").GetComponent<TMP_Text>().text = "CHAT LOSES";
                    endTime -= Time.deltaTime;
                }
                else if (endTime > 0)
                {
                    endTime -= Time.deltaTime;
                }
                else
                {
                    if (GameObject.Find("GameManager").GetComponent<GameManager>().autoplay)
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().PickRandomGame();
                        hangmanState = HangmanState.AUTOPLAY;
                    }
                    else
                    {

                        hangmanState = HangmanState.BEFORE_WAIT;
                    }
                    endTime = maxEndTime;
                }
                break;

            case HangmanState.AUTOPLAY:
                break;
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

    public void SetWord(string word)
    {
        wordToGuess = word.ToLower();
    }

    void LoseLife()
    {
        lifePoints -= 1;

        switch (lifePoints)
        {
            case 9:
                Part1.SetActive(true);
                break;
            case 8:
                Part2.SetActive(true);
                break;
            case 7:
                Part3.SetActive(true);
                break;
            case 6:
                Part4.SetActive(true);
                break;
            case 5:
                Part5.SetActive(true);
                break;
            case 4:
                Part6.SetActive(true);
                break;
            case 3:
                Part7.SetActive(true);
                break;
            case 2:
                Part8.SetActive(true);
                break;
            case 1:
                Part9.SetActive(true);
                break;
            case 0:
                Part10.SetActive(true);
                hangmanState = HangmanState.LOSE;
                break;

        }
    }

    public void TopWords()
    {
        for (int i = 0; i < wordsContainers.Count; i++)
        {
            Debug.Log("WC - " + wordsContainers[i].word);
            if (topWords.Any(TopWords => TopWords.word == wordsContainers[i].word))
            {
                for (int j = 0; j < topWords.Count; j++)
                {
                    if (wordsContainers[i].word == topWords[j].word)
                    {
                        TopWordsStruct temp = new TopWordsStruct();
                        temp.word = topWords[j].word;
                        temp.count = topWords[j].count + 1;
                        topWords.RemoveAt(j);
                        topWords.Add(temp);
                    }
                }
            }
            else
            {
                TopWordsStruct temp2 = new TopWordsStruct();
                temp2.word = wordsContainers[i].word;
                temp2.count = 1;

                topWords.Add(temp2);
            }
        }

        for (int k = 0; k < topWords.Count - 1; k++)
        {
            if (topWords[k].count < topWords[k + 1].count)
            {
                TopWordsStruct temp3 = new TopWordsStruct();
                temp3.word = topWords[k].word;
                temp3.count = topWords[k].count;

                TopWordsStruct temp4 = new TopWordsStruct();
                temp4.word = topWords[k + 1].word;
                temp4.count = topWords[k + 1].count;

                topWords[k] = temp4;
                topWords[k + 1] = temp3;

                k = 0;
            }
        }

        for (int h = 0; h < topWords.Count - 1; h++)
        {
            Debug.Log("TW - " + topWords[h].word + " - " + topWords[h].count);
        }

        CheckLetter(topWords[0].word.Substring(1));
        GameObject.Find("LastTry").GetComponent<TMP_Text>().text = "Last try: " + topWords[0].word.Substring(1);
    }

    void CheckLetter(string guess)
    {
        if (guess.Length > 1 && wordToGuess != guess)
        {
            LoseLife();
        }
        else if (!wordToGuess.Contains(guess) && guess.Length == 1)
        {
            LoseLife();
        }
        else
        {
            for (int i = 0; i < wordToGuess.Length; i++)
            {
                if (wordToGuess[i] == guess[0])
                {
                    StringBuilder sb = new StringBuilder(GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text);
                    sb[i] = guess[0];
                    GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text = sb.ToString();
                }
            }
        }


        if (wordToGuess == GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text)
        {
            hangmanState = HangmanState.WIN;
        }
        else if (lifePoints != 0)
        {
            hangmanState = HangmanState.PLAYING;
        }
    }



    public void ProcessMessage(ChatUser user)
    {
        WordsContainer temp = new WordsContainer();

        temp.user = user.username;
        temp.word = user.message;

        if (temp.word.Substring(1) == wordToGuess)
        {
            GameObject.Find("LastTry").GetComponent<TMP_Text>().text = "<color=#FF1000>" + user.username + " </color> guessed the correct word!";
            GameObject.Find("GuessingWord").GetComponent<TMP_Text>().text = wordToGuess;
            hangmanState = HangmanState.WIN;
        }
        else if (wordsContainers.Any(WordsContainer => WordsContainer.user == temp.user))
        {
            for (int i = 0; i < wordsContainers.Count; i++)
            {
                if (wordsContainers[i].user == temp.user)
                {
                    wordsContainers.RemoveAt(i);
                    wordsContainers.Add(temp);
                }
            }
        }
        else
        {
            wordsContainers.Add(temp);
        }
    }
}
