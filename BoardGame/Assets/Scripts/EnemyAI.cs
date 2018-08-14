using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyAI : MonoBehaviour
{

    public HexGrid grid;
    public List<Unit> units;

    public void Start()
    {
        UpdateUnits();
    }

    ///<summary>
    ///Get the updated list of Units from HexGrid
    ///</summary>
    private void UpdateUnits()
    {
        units = new List<Unit>();
        List<Unit> allUnits = grid.units;

        foreach (Unit unit in allUnits)
            if (unit.unitTeam == Unit.UnitTeamType.ENEMY)
                units.Add(unit);
    }

    ///<summary>
    ///Execute the AI turn
    ///</summary>
    public void ExecuteTurn()
    {
        HexScoreObject highestScore = null;

        //Update the list to see if we lost any units
        UpdateUnits();

        List<HexScoreObject> scoreList = new List<HexScoreObject>();

        //For each unit, see which tile is the best to go to
        foreach (Unit unit in units)
        {
            HexScoreObject unitScore = CalculateScore(unit);
            scoreList.Add(unitScore);
        }

        //Loop through the scores and grab the highest one
        foreach (HexScoreObject scoreObject in scoreList)
        {
            if (highestScore == null)
                highestScore = scoreObject;

            if (scoreObject != null)
                if (scoreObject.score > highestScore.score)
                    highestScore = scoreObject;
        }

        //There should always be a highest score, but we still check
        if (highestScore != null)
        {
            //Check what we want to do, if the tile is empty, just move, if there is an enemy on it: Attack! 
            if (highestScore.hexObject.unit == null)
                MoveUnit(highestScore.unit.transform.parent.GetComponent<HexObject>(), highestScore.hexObject);
            else
            {
                //Do damage to the unit and check if the attacked unit has died
                bool isDead = highestScore.hexObject.unit.Damage(grid.currentSelectedHex.unit.attack);
                if (isDead)
                {
                    //Update the units in the grid
                    grid.UpdateUnits();

                    //Bowmen do not move after killing a unit, others do
                    if (highestScore.unit.unitType != Unit.UnitType.BOWMAN)
                        MoveUnit(highestScore.unit.transform.parent.GetComponent<HexObject>(), highestScore.hexObject);
                }
            }
        }

        //End the turn
        GetComponent<GameHandler>().NextTurn();
    }

    ///<summary>
    ///Calculate the highest score for a unit    
    ///</summary>
    ///<param name="unit">The unit to calculate the score for</param>
    ///<returns>Returns a HexScoreObject with the tile the unit should move to, and the score the move has</returns>
    public HexScoreObject CalculateScore(Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();
        HexObject hex = unit.transform.parent.GetComponent<HexObject>();

        //==============================================
        //Step 1: Check the nearest neighbours
        scoreList.AddRange(CheckNearestNeighbours(hex, unit));

        //If step 1 failed (no player units next to the unit)

        //Step 2: Check the neighbours further away
        if (scoreList.Count == 0)
            scoreList.AddRange(CheckFurtherNeighbours(hex, unit));

        //Step 3: No one close to us, lets move towards enemies
        if (scoreList.Count == 0)
            scoreList.AddRange(CheckMoving(hex, unit));

        //==============================================        

        HexScoreObject highestScore = null;

        //Get the highest score
        foreach (HexScoreObject scoreObject in scoreList)
        {
            if (highestScore == null)
                highestScore = scoreObject;

            if (scoreObject.score > highestScore.score)
                highestScore = scoreObject;
        }

        return highestScore;
    }

    ///<summary>
    ///Check the tiles directly next to the given unit and calculate a possible score
    ///</summary>
    ///<param name="hex">The hex the unit is standing on</param>
    ///<param name="unit">The unit</param>
    ///<returns>Returns a list of possible hexes to move to, or perform an attack on</returns>
    public List<HexScoreObject> CheckNearestNeighbours(HexObject hex, Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();
        List<HexObject> hexes = grid.GetNeighbours(hex.Coords, 1);

        foreach (HexObject neighbourHex in hexes)
        {
            //If there is no enemy unit next to us, skip to next hex
            if (neighbourHex.unit == null || neighbourHex.unit.unitTeam == Unit.UnitTeamType.ENEMY)
                continue;

            if (neighbourHex.unit.health <= unit.attack)
            {
                //Can we kill it? give a score of 12, the highest score
                scoreList.Add(new HexScoreObject(unit, neighbourHex, 12));
            }
            else if (unit.unitType != Unit.UnitType.BOWMAN)
            {
                //We cannot kill it, but we can just attack it.However, if we are a bowman it might be better to just move away
                //otherwise this unit is dead next turn 
                scoreList.Add(new HexScoreObject(unit, neighbourHex, 10));
            }
            else if (neighbourHex.unit.unitType == Unit.UnitType.CAVELRY)
            {
                //If we are a bowman and the enemy is cavalry, we might be dead anyway. So attack (we do not have longtime planning)
                //We might be saved by other units, so give a lower score. 
                scoreList.Add(new HexScoreObject(unit, neighbourHex, 8));
            }
            else
            {
                //We are a bowman and a Knight is next to us, lets just run away
                //Search for the nearest friendly unit and move towards it and away from the enemy      

                //NEVER FINISHED :/              
            }
        }
        return scoreList;
    }

    ///<summary>
    ///Check the tiles a bit further away than next to the unit
    ///</summary>
    ///<param name="hex">The hex the unit is standing on</param>
    ///<param name="unit">The unit</param>
    ///<returns>Returns a list of possible hexes to move to, or perform an attack on</returns>
    public List<HexScoreObject> CheckFurtherNeighbours(HexObject hex, Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();
        List<HexObject> hexes = grid.GetNeighbours(hex.Coords, 2);
        hexes = grid.FilterHexes(hex.Coords, hexes, unit.unitType);

        foreach (HexObject neighbourHex in hexes)
        {
            //If there is no unit on it, skip to next hex
            if (neighbourHex.unit == null || neighbourHex.unit.unitTeam == Unit.UnitTeamType.ENEMY)
                continue;

            if (unit.unitType == Unit.UnitType.CAVELRY || unit.unitType == Unit.UnitType.BOWMAN)
            {
                //Are we a bowman or cavalry and can we attack the unit? Attack
                scoreList.Add(new HexScoreObject(unit, neighbourHex, 12));
            }
            else
            {
                //We are a knight
                if (neighbourHex.unit.unitType == Unit.UnitType.BOWMAN)
                {
                    //We should move towards it
                    List<HexObject> neighbourCloseNeighbours = grid.GetNeighbours(neighbourHex.Coords, 1);
                    neighbourCloseNeighbours = grid.FilterHexes(neighbourHex.Coords, neighbourCloseNeighbours, unit.unitType);

                    List<HexObject> UnitCloseNeighbours = grid.GetNeighbours(hex.Coords, 1);
                    UnitCloseNeighbours = grid.FilterHexes(hex.Coords, UnitCloseNeighbours, unit.unitType);

                    List<HexObject> commonHexes = new List<HexObject>();

                    //Loop through every hex in both the player's unit neighbours and the unit's neighbours. Add them to the commenHexes
                    foreach (HexObject closeHex in neighbourCloseNeighbours)
                        foreach (HexObject innerHex in UnitCloseNeighbours)
                            if (closeHex == innerHex)
                                commonHexes.Add(closeHex);

                    commonHexes = grid.FilterHexes(neighbourHex.Coords, commonHexes, unit.unitType);

                    List<HexObject> teamFilterList = new List<HexObject>();

                    foreach (HexObject obj in commonHexes)
                        if (obj.unit == false)
                            teamFilterList.Add(obj);

                    if (teamFilterList.Count > 0)
                    {
                        HexObject commonHex = teamFilterList.FirstOrDefault();
                        scoreList.Add(new HexScoreObject(unit, commonHex, 7));
                    }
                }
                else
                {
                    //We are a Knight and cavalry is in range to us, lets move away
                    //Search for the nearest friendly unit and move towards it and away from the enemy     

                    //NEVER FINISHED :/
                }
            }
        }
        return scoreList;
    }

    ///<summary>
    ///There is nothing really in range, so lets move towards the enemy in the south
    ///</summary>
    ///<param name="hex">The hex the unit is standing on</param>
    ///<param name="unit">The unit</param>
    ///<returns>Returns a list of possible hexes to move to, or perform an attack on</returns>
    public List<HexScoreObject> CheckMoving(HexObject hex, Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();

        //Get range 1 neighbours
        List<HexObject> nearNeighbours = grid.GetNeighbours(hex.Coords, 1);
        nearNeighbours = grid.FilterHexes(hex.Coords, nearNeighbours, unit.unitType);

        foreach (HexObject nearNeighbour in nearNeighbours)
        {
            //List of hexes only in the outer ring
            List<HexObject> outerRingNeighbours = grid.GetNeighbours(nearNeighbour.Coords, 2);

            if (outerRingNeighbours.FirstOrDefault(h => h.unit != null && h.unit.unitTeam == Unit.UnitTeamType.PLAYER) == null)
                if (nearNeighbour.unit == null)
                    scoreList.Add(new HexScoreObject(unit, nearNeighbour, (int)Mathf.Ceil(3 * (hex.Coords.y / 2))));
        }
        return scoreList;
    }

    ///<summary>
    ///Physically move the unit
    ///</summary>
    ///<param name="currentHex">The hex the unit is standing on</param>
    ///<param name="newHex">The new hex to move to</param>
    public void MoveUnit(HexObject currentHex, HexObject newHex)
    {
        grid.currentSelectedHex = newHex;
        newHex.unit = currentHex.unit;
        currentHex.unit = null;
        grid.currentSelectedHex.unit.gameObject.transform.SetParent(newHex.transform);
        newHex.unit.transform.localPosition = newHex.transform.localPosition;
    }
}

public class HexScoreObject
{
    public Unit unit;
    public HexObject hexObject;
    public int score;

    public HexScoreObject(Unit unit, HexObject hex, int score)
    {
        this.unit = unit;
        this.hexObject = hex;
        this.score = score;
    }
}