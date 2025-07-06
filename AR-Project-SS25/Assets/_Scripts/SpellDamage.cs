using Unity.Netcode;
using UnityEngine;

public static class SpellDamage
{
    public static int InvokeSpell(SpellType spellType, ElementType element)
    {
        if (spellType == SpellType.Shield)
        {
            var currentPlayerId = (int)GameStateManager.Instance.GetOwnID();
            var currentShield = GameStateManager.Instance.GetPlayerShieldTuple(currentPlayerId);
            var shieldType = currentShield.Item2;
            var shieldHealth = currentShield.Item1;
            if (shieldType == element)
            {
                var newShieldHealth = Mathf.Min(shieldHealth + 20, 100);
                GameStateManager.Instance.SetPlayerShield(currentPlayerId, newShieldHealth, element);
                return 0; // No damage dealt, just setting shield
            }
            GameStateManager.Instance.SetPlayerShield(currentPlayerId, 20, element);
            return 0; // No damage dealt, just setting shield
        }

        int enemyID = (int)GameStateManager.Instance.GetEnemyID();
        var shield = GameStateManager.Instance.GetPlayerShieldTuple(enemyID);

        if (shield.Item1 != 0)
        {
            int multiplier = GetElementWeakness(element, shield.Item2);
            switch (spellType)
            {
                case SpellType.SingleShot:
                    GameStateManager.Instance.SetPlayerShield(enemyID, Mathf.Max(0, shield.Item1 - 1 * multiplier),
                        shield.Item2);
                    return 0;
                case SpellType.Spear:
                    GameStateManager.Instance.SetPlayerShield(enemyID, Mathf.Max(0, shield.Item1 - 5 * multiplier),
                        shield.Item2);
                    return 1;
                case SpellType.WideShot:
                    GameStateManager.Instance.SetPlayerShield(enemyID, Mathf.Max(0, shield.Item1 - 10 * multiplier),
                        shield.Item2);
                    return 0;
                case SpellType.GroundPound:
                    GameStateManager.Instance.SetPlayerShield(enemyID, Mathf.Max(0, shield.Item1 - 10 * multiplier),
                        shield.Item2);
                    return 15;
                default:
                    return 0;
            }
        }
        else
        {
            int multiplier = 2;
            switch (spellType)
            {
                case SpellType.SingleShot:
                    return 1 * multiplier;
                case SpellType.Spear:
                    return 5 * multiplier;
                case SpellType.WideShot:
                    return 3 * multiplier;
                case SpellType.GroundPound:
                    return 15 * multiplier;
                default:
                    return 0;
            }
        }
    }

    private static int GetElementWeakness(ElementType attack, ElementType defense)
    {
        switch (attack)
        {
            case ElementType.Fire:
                return defense switch
                {
                    ElementType.Air => 3,
                    ElementType.Water => 1,
                    _ => 2
                };
            case ElementType.Water:
                return defense switch
                {
                    ElementType.Fire => 3,
                    ElementType.Earth => 1,
                    _ => 2
                };
            case ElementType.Earth:
                return defense switch
                {
                    ElementType.Air => 3,
                    ElementType.Water => 1,
                    _ => 2
                };
            case ElementType.Air:
                return defense switch
                {
                    ElementType.Earth => 3,
                    ElementType.Fire => 1,
                    _ => 2
                };
            default:
                return 2; // No weakness
        }
    }
}