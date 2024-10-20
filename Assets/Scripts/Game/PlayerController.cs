using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine;

public enum PlayerType
{
    GAME,
    MENU,
    COUNTING_ENTER,
    COUNTING_LEAVE,
    COUNTING_PLAYER
}

public class PlayerController : MonoBehaviour
{
    public bool inGame = false;
    public float moveSpeed = 1f;
    public bool isFacingRight = true;
    public bool isMoving = false;

    public PlayerType playerType;

    public Animator anim;
    public int skin;

    public Vector3 targetPos;

    float[] gridCellX = {
        -4f,
        -1.72f,
        0.56f,
        2.84f,
        5.12f,
        7.4f
    };

    float[] gridCellY = {
        4.43f,
        2.51f,
        0.59f,
        -1.33f,
        -3.25f
    };

    void Start()
    {
        skin = Random.Range(0, 8);
        anim = gameObject.GetComponent<Animator>();
        anim.SetInteger("SkinNum", skin);

        if (GameObject.Find("GameManager").GetComponent<GameManager>().gameState == GameState.MAIN_MENU)
        {
            if (transform.position.x > 0)
            {
                targetPos = new Vector3(-12, Random.Range(-2f, -4.2f), 0);
            }
            else
            {
                targetPos = new Vector3(12, Random.Range(-2f, -4.2f), 0);
            }
        }
        else if (GameObject.Find("GameManager").GetComponent<GameManager>().gameType == GameType.COUNTING && GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.START_GAME)
        {
            GameObject.Find("CountingManager").GetComponent<CountingManager>().gameCharacters.Add(gameObject);
            GameObject.Find("CountingManager").GetComponent<CountingManager>().skinNum.Add(skin);
            targetPos = transform.position;
            isMoving = false;
            anim.speed = 0f;

        }
        else if(GameObject.Find("GameManager").GetComponent<GameManager>().gameType == GameType.COUNTING && GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.ROUND)
        {
            playerType = PlayerType.COUNTING_ENTER;
            targetPos = new Vector3(0, -3.4f, 0);
        }
        else if(GameObject.Find("GameManager").GetComponent<GameManager>().gameType == GameType.COUNTING && GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.WAITING)
        {
            playerType = PlayerType.COUNTING_PLAYER;
            targetPos = new Vector3(Random.Range(-5, 8.1f), Random.Range(1.1f, 5), 0);
        }
        else
        {
            //first manual target pos
            targetPos = new Vector3(3.65f, 5, 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!inGame && GameObject.Find("GameManager").GetComponent<GameManager>().gameState != GameState.MAIN_MENU && GameObject.Find("GameManager").GetComponent<GameManager>().gameType != GameType.COUNTING)
        {
            if (transform.position != targetPos)
            {
                MovePlayer(targetPos);
            }
            else
            {
                // second manual target pos
                targetPos = new Vector3(3.65f, 3.5f, 0);

                if (transform.position == targetPos)
                {
                    int randomX = Random.Range(0, gridCellX.Length);
                    int randomY = Random.Range(0, gridCellY.Length);
                    SetTargetPos(randomX, randomY);
                    inGame = true;
                }
            }
        }
        else if (!inGame && GameObject.Find("GameManager").GetComponent<GameManager>().gameState == GameState.MAIN_MENU)
        {
            MovePlayer(targetPos);
            if (transform.position == targetPos)
            {
                Debug.Log("arrived");
                for (int i = 0; i < GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.Count; i++)
                {
                    if (GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers[i].userID == int.Parse(gameObject.name))
                    {
                        GameObject.Find("GameManager").GetComponent<GameManager>().gameUsers.RemoveAt(i);
                        Destroy(gameObject);
                    }
                }
            }
        }
        else if(!inGame && (GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.ROUND || GameObject.Find("CountingManager").GetComponent<CountingManager>().countingGameState == CountingGameState.TIME_TO_ANSWER))
        {
            MovePlayer(targetPos);
            if (transform.position == targetPos)
            {
                if(playerType == PlayerType.COUNTING_ENTER)
                {
                    GameObject.Find("CountingManager").GetComponent<CountingManager>().gameCharacters.Add(gameObject);
                    GameObject.Find("CountingManager").GetComponent<CountingManager>().skinNum.Add(skin);
                    transform.position = new Vector3(3.25f, -2, 0);
                    gameObject.SetActive(false);
                }
                if(playerType == PlayerType.COUNTING_LEAVE)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            MovePlayer(targetPos);
        }

        CheckOrientation();
    }

    void CheckOrientation()
    {
        if (transform.position.x > targetPos.x && isFacingRight)
        {
            anim.speed = 1f;

            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            Vector3 lsText = transform.Find("UserName").GetComponent<Transform>().localScale;
            lsText.x *= -1f;
            transform.Find("UserName").GetComponent<Transform>().localScale = lsText;
            transform.Find("Outline").GetComponent<Transform>().localScale = lsText;

            isFacingRight = false;
        }
        else if (transform.position.x < targetPos.x && !isFacingRight)
        {
            anim.speed = 1f;

            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            Vector3 lsText = transform.Find("UserName").GetComponent<Transform>().localScale;
            lsText.x *= -1f;
            transform.Find("UserName").GetComponent<Transform>().localScale = lsText;
            transform.Find("Outline").GetComponent<Transform>().localScale = lsText;

            isFacingRight = true;
        }
        else if (transform.position == targetPos && isMoving == true)
        {
            isMoving = false;
            anim.speed = 0f;
        }
        else if (transform.position != targetPos && isMoving == false)
        {
            isMoving = true;
            anim.speed = 1f;
        }
    }

    public void SetTargetPos(int indexX, int indexY)
    {
        targetPos.x = Random.Range(gridCellX[indexX], gridCellX[indexX] + 1.56f);
        targetPos.y = Random.Range(gridCellY[indexY] - 0.35f, gridCellY[indexY] - 1.43f);
        targetPos.z = 0;
    }

    void MovePlayer(Vector3 pos)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    public void Stop()
    {
        targetPos = transform.position;
    }

    public void KillPlayer()
    {
        GameObject PlayerDieParticle = Instantiate(GameObject.Find("ColorGameManager").GetComponent<ColorGameScene>().particlesPrefab, new Vector3(transform.position.x, transform.position.y - 0.33f, 0), Quaternion.identity);

        Destroy(gameObject, 1f);
        Destroy(PlayerDieParticle, 1f);
    }

    public Vector2Int GetGridPosition()
    {
        int xIndex = -1;
        int yIndex = -1;

        for (int i = 0; i < gridCellX.Length - 1; i++)
        {
            if ((transform.position.x - 0.18f) >= (gridCellX[i] - 0.32f) && (transform.position.x + 0.18f) < (gridCellX[i + 1] - 0.32f))
            {
                xIndex = i;
                break;
            }
        }

        if ((transform.position.x - 0.18f) >= gridCellX[gridCellX.Length - 1] - 0.32f)
        {
            xIndex = gridCellX.Length - 1;
        }

        for (int j = 0; j < gridCellY.Length - 1; j++)
        {
            if (transform.position.y <= gridCellY[j] && transform.position.y - 0.33f > gridCellY[j + 1])
            {
                yIndex = j;
                break;
            }
        }

        if (transform.position.y <= gridCellY[gridCellY.Length - 1] + 0.48f)
        {
            yIndex = gridCellY.Length - 1;
        }

        Vector2Int returnValue = new Vector2Int(xIndex, yIndex);
        return returnValue;
    }
}
