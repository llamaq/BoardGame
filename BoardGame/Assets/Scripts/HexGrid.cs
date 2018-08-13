using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public HexObject hexPrefab;
    public Unit unitPrefab;
    public List<Unit> units;

    public HexObject currentSelectedHex = null;

    private int width = 5;
    private int height = 7;
    public List<HexObject> hexes;

    void Awake()
    {
        //Create map with index 0
        CreateMap(0);
    }

    public void UpdateUnits()
    {
        List<Unit> newUnits = new List<Unit>();
        foreach (HexObject hex in hexes)
        {
            if (hex.unit != null)
                newUnits.Add(hex.unit);
        }
        this.units = newUnits;
    }

    void CreateMap(int index)
    {
        hexes = new List<HexObject>();

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                hexes.Add(CreateCell(x, z, Maps.mapArray[index][i], i));
                i++;
            }
        }
    }

    HexObject CreateCell(int x, int z, char c, int i)
    {
        float xPos = x * (HexObject.innerRadius * 0.95f);
        float zPos = (z + x * 0.5f - x / 2) * (HexObject.innerRadius * 1.1f);
        Vector3 pos = new Vector3(xPos, 0f, zPos);

        HexObject hexCell = Instantiate<HexObject>(hexPrefab);
        hexCell.Coords = new Vector2(x, z);
        hexCell.transform.SetParent(transform, false);
        hexCell.transform.localPosition = pos;
        hexCell.hexType = HexObject.GetTypeFromChar(c);

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
            units.Add(unit);
        }

        return hexCell;
    }

    public List<HexObject> GetNeighbours(Vector2 coords, int range)
    {
        List<HexObject> hexList = new List<HexObject>();

        foreach (HexObject obj in hexes)
        {
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

                foreach (HexObject hex in closeRangeHexes)
                {
                    if (hex.hexType == HexObject.HexType.MOUNTAIN)
                        tempList.Remove(hex);
                }

                newList.AddRange(tempList);
                break;
        }

        return newList;
    }
}
