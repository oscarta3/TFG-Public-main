using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public Vector3 targetPos;
    float speed;

    void Start()
    {
        if (transform.position.x > 0)
        {
            targetPos = new Vector3(-12.5f, transform.position.y, transform.position.z);
        }
        else
        {
            targetPos = new Vector3(12.5f, transform.position.y, transform.position.z);
        }

        speed = Random.Range(0.2f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        MoveCloud();

        if (transform.position.x == targetPos.x)
        {
            GameObject.Find("MainMenuController").GetComponent<MainMenuController>().DestroyCloud();
        }
    }

    void MoveCloud()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }
}
