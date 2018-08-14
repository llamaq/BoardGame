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

    ///<summary>
    ///Opens a level
    ///</summary>
    ///<param name="level">The index of the level</param>
    public void OpenLevel(int level)
    {
        menu.SetActive(false);
        grid.CreateMap(level);
        menuButton.SetActive(true);
    }

    ///<summary>
    ///Back to the menu
    ///</summary>
    public void BackToMenu()
    {
        menuButton.SetActive(false);
        grid.RemoveMap();
        grid.endText.text = "";
        menu.SetActive(true);
    }

    ///<summary>
    ///Quit the game
    ///</summary>
    public void Quit()
    {
        Application.Quit();
    }

    ///<summary>
    ///Used to execute the enemies turn, and take control from the player, and in reverse
    ///</summary>
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
