using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType { KNIGHT, BOWMAN, CAVELRY }
    public enum UnitTeamType { PLAYER, ENEMY }

    public Sprite[] textures = new Sprite[3];
    private Renderer rend;

    public UnitType unitType;
    public UnitTeamType unitTeam;
    public int health = 1;
    public int attack = 1;

    ///<summary>
    ///Set the Unit type, and the attack and health
    ///</summary>
    ///<param name="type">The unit type to set</param>
    public void SetUnitType(UnitType type)
    {
        this.unitType = type;
        SpriteRenderer rend = this.transform.GetComponentInChildren<SpriteRenderer>();
        rend.sprite = textures[(int)type];

        switch (type)
        {
            case UnitType.BOWMAN:
                health = 1;
                attack = 1;
                break;
            case UnitType.CAVELRY:
                health = 2;
                attack = 3;
                break;
            default:
                health = 3;
                attack = 2;
                break;
        }
    }

    ///<summary>
    ///Do damage to the unit
    ///</summary>
    ///<param name="damage">The amount of damage to do</param>
    ///<returns>Returns whether the unit has died or not</returns>
    public bool Damage(int damage)
    {
        this.health -= damage;

        if (health <= 0)
        {
            this.transform.parent.GetComponent<HexObject>().unit = null;
            GameObject.Destroy(this.gameObject);
            return true;
        }
        return false;
    }
}
