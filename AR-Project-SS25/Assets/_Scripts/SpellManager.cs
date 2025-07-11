using System;
using System.Collections;
using System.Collections.Generic;
using PixPlays.ElementalVFX;
using Sirenix.OdinInspector;
using Unity.Services.Matchmaker.Models;
using UnityEngine;


public class SpellManager : Singleton<SpellManager>
{
    [SerializeField] private GameObject spellPreview;
    
    
    private SpellType _currentSpellType;
    private ElementType _currentElementType;
    
    
    private void Start()
    {
        Debug.Log("Starting Spell Manager");
        spellPreview.SetActive(false);
    }
    
    public void SetSpellPreviewActive(bool active)
    {
        spellPreview?.SetActive(active);
    }


    public bool SpawnSpell(bool isLocalPlayerCaster, SpellType spellType, ElementType elementType)
    {
        // bool isPlayerTurn = GameStateManager.Instance.IsMyTurn();
    
        // Prevent duplicate spell preview when not player's turn
        // if (!isPlayerTurn)
        // {
        //     if (spellType == _currentSpellType)
        //         return false;
        //
        //     SpawnSpellPreview(spellType);
        //     return false;
        // }
    
        // Only spawn attack if a valid element is selected --> This will alawys be valid, as server now checks this before spawning
        if (elementType != ElementType.None)
        {
            _currentSpellType = spellType;
            _currentElementType = elementType;
            
            // Debug.Log( $"[SpellManager] {spellType} with element: {elementType} for local player: {isLocalPlayerCaster}");
            
            // Spawn the spell attack
            SpawnSpellAttack(isLocalPlayerCaster, spellType, elementType);

            // This waits until the spell animation is complete
            StartCoroutine(SpellAnimationRoutine());
            return true;
        }
    
        // Show preview if no valid element is selected
        // SpawnSpellPreview(spellType);
        return false;
    }
    
    
    // TODO: @Juli Actually set the correct values here
    private IEnumerator SpellAnimationRoutine()
    {
        float waitTime = 0f;

        switch (_currentSpellType)
        {
            case SpellType.GroundPound:
                waitTime = 1.5f;
                break;
            case SpellType.Shield:
                waitTime = 2.0f;
                break;
            default:
                waitTime = 1.0f;
                break;
        }
        
        yield return new WaitForSeconds(waitTime);
        
        // Notify the server that the spell animation on local client is complete
        GameStateManager.Instance.NotifySpellAnimationCompleteServerRpc();
    }
    
    
    // Do we need this?
    private void SpawnSpellPreview(SpellType spellType)
    {
        var spellData = DataManagement.Instance.spellDataList.Find(data => data.Recipe == spellType);
        var visualData = spellData.GetPrefabTuple(ElementType.Fire);
        if (visualData == null)
        {
            Debug.LogError($"No prefab found for spell {spellType}");
            return;
        }
        spellPreview.GetComponent<SpriteRenderer>().sprite = visualData.Icon;
        DisplaySpellPreview();
    }
    
    private void DisplaySpellPreview()
    {
        // Render iconToDisplay sprite in the center of the grid
        var spawnPosition = CraftingGrid.Instance.middleRow[1].transform.position; // TODO: depends on which player we are
        spellPreview.transform.position = spawnPosition + new Vector3(0, 0.5f, 0); // Adjust height if needed
        spellPreview.transform.localScale = new Vector3(1, 1, 1) * 0.25f; 
        spellPreview.transform.rotation = 
            Camera.main ?
            Quaternion.LookRotation(Camera.main.transform.forward) :
            Quaternion.identity;
        spellPreview.SetActive(true);
    }

    private void SpawnSpellAttack(bool isLocalPlayerCaster, SpellType spellType, ElementType elementType)
    {
        var spellData = DataManagement.Instance.spellDataList.Find(data => data.Recipe == spellType);
        var visualData = spellData.GetPrefabTuple(elementType);
        
        // Skip spell spawning if no visual data is found
        if (visualData == null)
        {
            Debug.LogError($"No prefab found for spell {spellType} and element {elementType}");
            return;
        }
    
        Debug.Log( $"[SpellManager] Found Spell Tuple: {spellType} with element: {visualData.Element}");
        
        int currentPlayerId = isLocalPlayerCaster ? (int)PlayerState.LocalPlayer.OwnerClientId : (int)PlayerState.EnemyPlayer.OwnerClientId;
    
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        Vector3 targetPosition;
        float radius = 0.5f; // Default radius
        
        // Calculate spawn position and rotation based on spell type
        switch (spellType)
        {
            case SpellType.GroundPound:
                (spawnPosition, spawnRotation) = GetGroundPoundSpawn(currentPlayerId);
                targetPosition = PlayfieldManagement.Instance.enemyHealthVisual.transform.position;
                radius = 1.0f;
                break;
            case SpellType.Shield:
                (spawnPosition, spawnRotation) = GetShieldSpawn(currentPlayerId);
                targetPosition = PlayfieldManagement.Instance.playerHealthVisual.transform.position;
                break;
            default:
                (spawnPosition, spawnRotation) = GetDefaultSpellSpawn(currentPlayerId);
                targetPosition = PlayfieldManagement.Instance.enemyHealthVisual.transform.position;
                break;
        }
        
        // Instantiate the spell visual prefab
        var spellInstance = Instantiate(visualData.VisualPrefab, spawnPosition, spawnRotation);

        var vfxData = new VfxData(
            spawnPosition,
            targetPosition,
            5f,
            radius
        );

        if (spellInstance.TryGetComponent<BaseVfx>(out var baseVfx))
        {
            baseVfx.Play(vfxData);
        }
        else
        {
            Debug.LogWarning("Spawned VFX prefab does not have a BaseVfx component attached.");
        }
    }
    
    private (Vector3, Quaternion) GetGroundPoundSpawn(int currentPlayerId)
    {
        int enemyId = (currentPlayerId == 0) ? 1 : 0; // TODO: do we need this or maybe when spawning crystal?
        var crystalPosition = PlayfieldManagement.Instance.enemyHealthVisual.transform.position;
        Vector3 spawnPosition = crystalPosition;
        Quaternion spawnRotation = Quaternion.identity;
        return (spawnPosition, spawnRotation);
    }
    
    private (Vector3, Quaternion) GetShieldSpawn(int currentPlayerId)
    {
        Vector3 spawnPosition = PlayfieldManagement.Instance.playerHealthVisual.transform.position; // TODO: maybe need distance from crystal
        Vector3 direction = (PlayfieldManagement.Instance.playFieldVisual.transform.position - spawnPosition).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(direction);
        return (spawnPosition, spawnRotation);
    }
    
    private (Vector3, Quaternion) GetDefaultSpellSpawn(int currentPlayerId)
    {
        int enemyId = (currentPlayerId == 0) ? 1 : 0; // TODO: do we need this or maybe when spawning crystal?
        var crystalPosition = PlayfieldManagement.Instance.enemyHealthVisual.transform.position;
        Vector3 direction = (crystalPosition - PlayfieldManagement.Instance.playFieldVisual.transform.position).normalized;
        Vector3 spawnPosition = PlayfieldManagement.Instance.playFieldVisual.transform.position; // TODO: maybe needs to be closer or further away from crystal
        Quaternion spawnRotation = Quaternion.LookRotation(direction);
        return (spawnPosition, spawnRotation);
    }
}