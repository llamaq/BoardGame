using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public bool isPlayerTurn = true;
    public Text turnText;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            turnText.text = "Computer's turn";

            GetComponent<EnemyAI>().ExecuteTurn();
        }
        else
        {
            isPlayerTurn = true;
            turnText.text = "Player's Turn";
        }

    }
}
