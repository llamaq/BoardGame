﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public HexObject hexPrefab;
    public Unit unitPrefab;
    public List<Unit> units;

    public HexObject currentSelectedHex = null;

    private int width = 5;
    private int height = 7;
    public List<HexObject> hexes;
    public Text endText;

    ///<summary>
    ///Update the list with Units
    ///</summary>
    public void UpdateUnits()
    {
        List<Unit> newUnits = new List<Unit>();
        foreach (HexObject hex in hexes)
            if (hex.unit != null)
                newUnits.Add(hex.unit);

        if (newUnits.FindAll(u => u.unitTeam == Unit.UnitTeamType.PLAYER).Count == 0)
        {
            //The player has lost
            endText.text = "You lose!";
        }

        if (newUnits.FindAll(u => u.unitTeam == Unit.UnitTeamType.ENEMY).Count == 0)
        {
            //The player has won
            endText.text = "You win!";
        }

        this.units = newUnits;
    }

    ///<summary>
    ///Destroys the map, used in going back to the menu
    ///</summary>
    public void RemoveMap()
    {
        foreach (HexObject hex in hexes)
            GameObject.Destroy(hex.gameObject);

        units = new List<Unit>();
    }

    ///<summary>
    ///Creates a map with the given index
    ///</summary>
    ///<param name="index">The map index</param>
    public void CreateMap(int index)
    {
        hexes = new List<HexObject>();

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                hexes.Add(CreateHex(x, z, Maps.mapArray[index][i], i));
                i++;
            }
        }
    }

    ///<summary>
    ///Create a hex on the given coordinates 
    ///</summary>
    ///<param name="x">The x Coord of the hex</param>
    ///<param name="z">The z Coord of the hex</param>
    ///<param name="c">The character on the map, used to create the units</param>
    ///<param name="i">The loop index</param>
    ///<returns>Returns the created HexObject</returns>
    HexObject CreateHex(int x, int z, char c, int i)
    {
        float xPos = x * (HexObject.innerRadius * 0.95f);
        float zPos = (z + x * 0.5f - x / 2) * (HexObject.innerRadius * 1.1f);
        Vector3 pos = new Vector3(xPos, 0f, zPos);

        //Instantiate a hex from the prefab
        HexObject hexCell = Instantiate<HexObject>(hexPrefab);
        hexCell.Coords = new Vector2(x, z);
        hexCell.transform.SetParent(transform, false);
        hexCell.transform.localPosition = pos;
        hexCell.hexType = HexObject.GetTypeFromChar(c);

        //If the given character is a unit character
        if (c == 'k' || c == 'c' || c == 'b')
        {
            Unit unit = Instantiate<Unit>(unitPrefab, hexCell.transform);
            unit.transform.localPosition = hexCell.transform.localPosition;
            hexCell.unit = unit;

            Unit.UnitType type;
            switch (c)
            {
                case 'c': type = Unit.UnitType.CAVELRY; break;
                case 'b': type = Unit.UnitType.BOWMAN; break;
                default: type = Unit.UnitType.KNIGHT; break;
            }
            unit.SetUnitType(type);

            unit.unitTeam = i < width * 2 ? Unit.UnitTeamType.PLAYER : Unit.UnitTeamType.ENEMY;

            Color unitColor = unit.unitTeam == Unit.UnitTeamType.PLAYER ? new Color(0.306f, 0.736f, 0.764f, 1) : new Color(0.773f, 0.412f, 0.353f, 1);
            unit.transform.GetComponentInChildren<Renderer>().material.SetColor("_Color", unitColor);

            units.Add(unit);
        }
        return hexCell;
    }

    ///<summary>
    ///Get the neighbours in the given range (1 or 2)
    ///</summary>
    ///<param name="coords">The coords of the hex of which we want the neighbours of</param>
    ///<param name="range">The range (1 or 2)</param>
    ///<returns>Returns a list of hexes which are neighbours</returns>
    public List<HexObject> GetNeighbours(Vector2 coords, int range)
    {
        List<HexObject> hexList = new List<HexObject>();

        //Loop through every hex on the board
        foreach (HexObject obj in hexes)
        {
            //Skip the given hex, we only want its neigbours
            if (obj.Coords.x == coords.x && obj.Coords.y == coords.y)
                continue;

            if (Vector2.Distance(obj.Coords, coords) <= range + 0.5f)
            {
                if (obj.Coords.x != coords.x && obj.Coords.y == coords.y + (coords.x % 2 == 0 ? range : -range))
                    continue;

                hexList.Add(obj);
            }
        }
        return hexList;
    }

    ///<summary>
    ///Filter the given list of hexes, removes mountains
    ///</summary>
    ///<param name="selectedHex">The selected hex where the unit stands on</param>
    ///<param name="oldList">The old list which needs to be filtered</param>
    ///<param name="unitType">The type of the unit, used to filter certain movement or attack</param>
    ///<returns>Returns a list of filtered hexes</returns>
    public List<HexObject> FilterHexes(Vector2 selectedHex, List<HexObject> oldList, Unit.UnitType unitType)
    {
        List<HexObject> newList = new List<HexObject>();

        switch (unitType)
        {
            case Unit.UnitType.CAVELRY:
                List<HexObject> tempList = new List<HexObject>();
                List<HexObject> closeRangeHexes = GetNeighbours(selectedHex, 1);

                tempList.AddRange(oldList);

                //Remove every hex which is in the inner-ring, if they are mountains
                foreach (HexObject closeHex in closeRangeHexes)
                {
                    tempList.Remove(closeHex);

                    //Add the inner-ring to the new list
                    if (closeHex.hexType != HexObject.HexType.MOUNTAIN)
                        newList.Add(closeHex);
                }

                //Add the outer-ring to the new list, except if we cannot reach it
                foreach (HexObject outerHex in tempList)
                {
                    //Get the neighbours of every outer hex 
                    List<HexObject> outerNeighbours = GetNeighbours(outerHex.Coords, 1);

                    foreach (HexObject closeRangeHex in closeRangeHexes)
                    {
                        //if one of the neighbours of the outerhex is also in the inner ring and it is not a mountain, a cavalry unit can reach it.
                        if (outerNeighbours.Contains(closeRangeHex) && closeRangeHex.hexType != HexObject.HexType.MOUNTAIN)
                            newList.Add(outerHex);
                    }
                }
                break;
            case Unit.UnitType.BOWMAN:
                tempList = new List<HexObject>();
                closeRangeHexes = GetNeighbours(selectedHex, 1);

                tempList.AddRange(oldList);

                //Remove every hex which is in the inner-ring, if they are mountains
                foreach (HexObject closeHex in closeRangeHexes)
                {
                    tempList.Remove(closeHex);

                    //Add the inner-ring to the new list
                    if (closeHex.hexType != HexObject.HexType.MOUNTAIN)
                        newList.Add(closeHex);
                }

                //Add the outer-ring to the new list, except if we cannot reach it
                foreach (HexObject outerHex in tempList)
                {
                    if (outerHex.unit == null)
                        continue;

                    //Get the neighbours of every outer hex 
                    List<HexObject> outerNeighbours = GetNeighbours(outerHex.Coords, 1);

                    foreach (HexObject closeRangeHex in closeRangeHexes)
                    {
                        //if one of the neighbours of the outerhex is also in the inner ring and it is not a mountain, a cavalry unit can reach it.
                        if (outerNeighbours.Contains(closeRangeHex) && closeRangeHex.hexType != HexObject.HexType.MOUNTAIN)
                            newList.Add(outerHex);
                    }
                }
                break;
            default:
                closeRangeHexes = GetNeighbours(selectedHex, 1);
                tempList = new List<HexObject>();
                tempList.AddRange(oldList);

                //Remove mountains
                foreach (HexObject hex in closeRangeHexes)
                    if (hex.hexType == HexObject.HexType.MOUNTAIN)
                        tempList.Remove(hex);

                newList.AddRange(tempList);
                break;
        }
        return newList;
    }
}
