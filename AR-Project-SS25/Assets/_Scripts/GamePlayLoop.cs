using System;
using UnityEngine;

[Obsolete("Has been replaced in GameStateManager")]
public class GamePlayLoop : MonoBehaviour
{
    // public CanvasPage_Gameplay gameplayPage;
    
    
    
    
    // private void Start()
    // {
    //     // if(gameplayPage != null)
    //     //     gameplayPage.onCastSpell += OnCastSpell;
    // }
    
    
    
    #region Try to Cast Spell
    
    // void OnCastSpell(Tuple<SpellType?, ElementType?> spellData)
    // {
    //     if (spellData?.Item1 == null)
    //     {
    //         SpellManager.Instance.SetSpellPreviewActive(false);
    //         return;
    //     }
    //     
    //     var spellType = spellData.Item1.Value;
    //     var elementType = spellData.Item2 ?? ElementType.None;
    //     
    //     // Spawn Spell or Preview Icon (If only preview icon, then it is not player's turn)
    //     if (!SpellManager.Instance.SpawnSpell(spellType, elementType))
    //     {
    //         return;
    //     }
    //     
    //     
    //     //TODO: Do this after a timer, so the spell animation is finished
    //     // 1. Spawn Spell
    //     // 2. Wait until impact
    //     // 3. Calculate & apply damage to the player
    //     // 4. End Turn
    //     
    //     // Todo: calculate damage and apply it to the player
    //     int damage = 15; // for now hardcoded
    //     GameStateManager.Instance.EndTurnRequestServerRpc(damage);
    // }

    #endregion
    
}
