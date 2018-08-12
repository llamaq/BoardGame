using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType { KNIGHT, BOWMEN, CAVELRY }
    public enum UnitTeamType { PLAYER, ENEMY }

    public Sprite[] textures = new Sprite[3];
    private Renderer rend;

    public UnitType unitType;
    public UnitTeamType unitTeam;
    public int health = 1;
    public int attack = 1;


    public void SetUnitType(UnitType type)
    {
        this.unitType = type;
        SpriteRenderer rend = this.transform.GetComponentInChildren<SpriteRenderer>();
        rend.sprite = textures[(int)type];

        switch (type)
        {
            case UnitType.BOWMEN:
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
