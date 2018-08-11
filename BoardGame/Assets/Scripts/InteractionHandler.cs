using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{

    public HexGrid grid;

    void Update()
    {
        if (Input.GetMouseButton(0))
            HandleInput();
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
            ClickHex(hit.transform.gameObject);
    }

    void ClickHex(GameObject hex)
    {
        foreach (HexObject hexObject in grid.hexes)
            hexObject.SetHighlighted(false);

        HexObject obj = hex.GetComponent<HexObject>();

        List<HexObject> hexArray = grid.GetNeighbours(obj.Coords, 2);
        hexArray = grid.FilterHexes(obj.Coords, hexArray, Unit.UnitType.CAVELRY);

        foreach (HexObject hexObject in hexArray)
            hexObject.SetHighlighted(true);
    }
}
