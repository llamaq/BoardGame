using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType { KNIGHT, BOWMEN, CAVELRY }

    public UnitType unitType;
    public int health;
    public int attack;


    void Start()
    {

    }
}
