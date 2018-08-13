using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public bool isPlayerTurn = true;
    public Text turnText;
    public HexGrid grid;
    public GameObject menu;
    public GameObject menuButton;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenLevel(int level)
    {
        menu.SetActive(false);
        grid.CreateMap(level);
        menuButton.SetActive(true);
    }

    public void BackToMenu()
    {
        menuButton.SetActive(false);
        grid.RemoveMap();
        menu.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void NextTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            //turnText.text = "Computer's turn";

            GetComponent<EnemyAI>().ExecuteTurn();
        }
        else
        {
            isPlayerTurn = true;
            //turnText.text = "Player's turn";
        }

    }
}
