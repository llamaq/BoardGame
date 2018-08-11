using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public HexObject prefab;

    private int width = 5;
    private int height = 7;
    public HexObject[] hexes;

    void Awake()
    {
        //Create map with index 0
        CreateMap(0);


    }

    void CreateMap(int index)
    {
        hexes = new HexObject[width * height];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                hexes[i] = CreateCell(x, z, Maps.mapArray[index][i]);
                i++;
            }
        }
    }

    HexObject CreateCell(int x, int z, char c)
    {
        float xPos = x * (HexObject.innerRadius * 0.95f);
        float zPos = (z + x * 0.5f - x / 2) * (HexObject.innerRadius * 1.1f);
        Vector3 pos = new Vector3(xPos, 0f, zPos);

        HexObject hexCell = Instantiate<HexObject>(prefab);
        hexCell.Coords = new Vector2(x, z);
        hexCell.transform.SetParent(transform, false);
        hexCell.transform.localPosition = pos;
        hexCell.hexType = HexObject.GetTypeFromChar(c);

        if (c == 'k')
            hexCell.unit = new Unit("");

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
                ArrayList tempList = new ArrayList();
                List<HexObject> closeRangeHexes = GetNeighbours(selectedHex, 1);

                tempList.AddRange(oldList);

                //Remove every hex which is in the inner-ring
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
        }
        return newList;
    }
}
