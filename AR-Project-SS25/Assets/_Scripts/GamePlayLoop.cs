using UnityEngine;

public class GamePlayLoop : MonoBehaviour
{
    void Update()
    {
        var craftingResult = GridManagement.Instance.CheckCraftingResult();
        if (craftingResult?.Item1 == null)
        {
            SpellManager.Instance.SetSpellPreviewActive(false);
            return;
        }
        
        var spellType = craftingResult.Item1.Value;
        var elementType = craftingResult.Item2 ?? ElementType.None;
        // Spawn Spell or Preview Icon (If only preview icon, then it is not player's turn)
        if (!SpellManager.Instance.SpawnSpell(spellType, elementType))
        {
            return;
        }
        
        // Todo: calculate damage and apply it to the player
        int damage = SpellDamage.InvokeSpell(spellType, elementType); 
        GameStateManager.Instance.EndTurnRequestServerRpc(damage);
    }
}