using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public HexGrid grid;
    public List<Unit> units;

    public void Start()
    {
        UpdateUnits();
    }

    private void UpdateUnits()
    {
        units = new List<Unit>();
        List<Unit> allUnits = grid.units;

        foreach (Unit unit in allUnits)
        {
            if (unit.unitTeam == Unit.UnitTeamType.ENEMY)
                units.Add(unit);
        }
    }

    public void ExecuteTurn()
    {
        //Update the list to see if we lost any units
        UpdateUnits();

        List<HexScoreObject> scoreList = new List<HexScoreObject>();

        //For each unit, see which tile is the best to go to
        foreach (Unit unit in units)
        {
            HexScoreObject unitScore = CalculateScore(unit);
            scoreList.Add(unitScore);
        }

        HexScoreObject highestScore = null;

        foreach (HexScoreObject scoreObject in scoreList)
        {
            if (highestScore == null)
                highestScore = scoreObject;

            if (scoreObject != null)
                if (scoreObject.score > highestScore.score)
                    highestScore = scoreObject;
        }

        if (highestScore != null)
        {
            //Check what we want to do, if the tile is empty, just move, if there is an enemy on it: Attack! 
            if (highestScore.hexObject.unit == null)
                MoveUnit(highestScore.unit.transform.parent.GetComponent<HexObject>(), highestScore.hexObject);
            else
            {
                bool isDead = highestScore.hexObject.unit.Damage(grid.currentSelectedHex.unit.attack);
                if (isDead)
                {
                    grid.UpdateUnits();

                    if (highestScore.unit.unitType != Unit.UnitType.BOWMAN)
                        MoveUnit(highestScore.unit.transform.parent.GetComponent<HexObject>(), highestScore.hexObject);
                }
            }
        }

        GetComponent<GameHandler>().NextTurn();
    }

    public HexScoreObject CalculateScore(Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();

        HexObject hex = unit.transform.parent.GetComponent<HexObject>();

        //==============================================
        //Step 1: Check the nearest neighbours
        scoreList.AddRange(CheckNearestNeighbours(hex, unit));

        if (scoreList.Count == 0)
            scoreList.AddRange(CheckFurtherNeighbours(hex, unit));
        //==============================================        

        HexScoreObject highestScore = null;

        foreach (HexScoreObject scoreObject in scoreList)
        {
            if (highestScore == null)
                highestScore = scoreObject;

            if (scoreObject.score > highestScore.score)
                highestScore = scoreObject;
        }

        return highestScore;
    }

    public List<HexScoreObject> CheckNearestNeighbours(HexObject hex, Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();
        List<HexObject> hexes = grid.GetNeighbours(hex.Coords, 1);

        foreach (HexObject neighbourHex in hexes)
        {
            //If there is no enemy unit next to us, skip to next hex
            if (neighbourHex.unit == null)
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
            }
        }
        return scoreList;
    }

    public List<HexScoreObject> CheckFurtherNeighbours(HexObject hex, Unit unit)
    {
        List<HexScoreObject> scoreList = new List<HexScoreObject>();
        List<HexObject> hexes = grid.GetNeighbours(hex.Coords, 2);
        hexes = grid.FilterHexes(hex.Coords, hexes, unit.unitType);

        foreach (HexObject neighbourHex in hexes)
        {
            //If there is no unit on it, skip to next hex
            if (neighbourHex.unit == null)
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

                    scoreList.Add(new HexScoreObject(unit, commonHexes[0], 7));
                }
            }
        }

        return scoreList;
    }

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