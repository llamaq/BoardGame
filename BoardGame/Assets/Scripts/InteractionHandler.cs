using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{

    public HexGrid grid;
    public GameHandler handler;

    void Update()
    {
        if (handler.isPlayerTurn)
            if (Input.GetMouseButton(0))
                HandleInput();
    }

    ///<summary>
    ///Handles the left mouse input, used for moving units
    ///</summary>
    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
            ClickHex(hit.transform.gameObject);
    }

    ///<summary>
    ///Called when a GameObject has been clicked
    ///</summary>
    private void ClickHex(GameObject hex)
    {
        //The HexObject class of the clicked object
        HexObject clickedHex = hex.GetComponent<HexObject>();

        //If the clicked object is not a HexObject, return
        if (clickedHex == null)
            return;

        //if the clicked hex is already highlighted, a unit is selected and the unit can move to it, or attack the unit on there
        if (clickedHex.isHighlighted)
        {
            //Check if the clicked hex has a unit on it
            if (clickedHex.unit != null)
            {
                //If the unit on the hex is a player (our own team) the unit cannot move there, so do not highlight it
                if (clickedHex.unit.unitTeam == Unit.UnitTeamType.PLAYER)
                    SelectHex(clickedHex);
                else
                {
                    //the clicked hex has an ememy on it, and we want to attack it
                    bool isDead = clickedHex.unit.Damage(grid.currentSelectedHex.unit.attack);
                    if (isDead)
                    {
                        //Unit died, so we need to update the Units in the grid
                        grid.UpdateUnits();

                        //Bowmen do not move after they kill someone, but others do
                        if (grid.currentSelectedHex.unit.unitType != Unit.UnitType.BOWMAN)
                            MoveUnit(clickedHex);
                    }
                }
                //Attacked an enemy, end turn
                handler.NextTurn();
            }
            else
            {
                //We want to move the selected unit                
                MoveUnit(clickedHex);

                //Turn off the highlights
                foreach (HexObject hexObject in grid.hexes)
                    hexObject.SetHighlighted(false);

                //We moved
                handler.NextTurn();
                return;
            }
        }

        SelectHex(clickedHex);
    }

    ///<summary>
    ///Select the clicked hex
    ///</summary>
    private void SelectHex(HexObject clickedHex)
    {
        //The clicked hex was not highlighted, which means it's a fresh click
        grid.currentSelectedHex = clickedHex;

        //Remove every highlight
        foreach (HexObject hexObject in grid.hexes)
            hexObject.SetHighlighted(false);

        //If the clicked hex does not contain a Unit, or the unit is an enemy, return
        if (clickedHex.unit == null || clickedHex.unit.unitTeam == Unit.UnitTeamType.ENEMY)
            return;

        int radius = clickedHex.unit.unitType == Unit.UnitType.KNIGHT ? 1 : 2;

        List<HexObject> hexList = grid.GetNeighbours(clickedHex.Coords, radius);
        hexList = grid.FilterHexes(clickedHex.Coords, hexList, clickedHex.unit.unitType);

        foreach (HexObject hexObject in hexList)
            hexObject.SetHighlighted(true);
    }

    ///<summary>
    ///Moves the current selected unit to the given Hex
    ///</summary>
    ///<param name="clickedHex">The Hex to move to</param>
    private void MoveUnit(HexObject clickedHex)
    {
        Unit currentSelectedUnit = grid.currentSelectedHex.unit;
        grid.currentSelectedHex.unit = null;

        grid.currentSelectedHex = clickedHex;
        clickedHex.unit = currentSelectedUnit;
        grid.currentSelectedHex.unit.gameObject.transform.SetParent(clickedHex.transform);
        clickedHex.unit.transform.localPosition = clickedHex.transform.localPosition;
    }
}
