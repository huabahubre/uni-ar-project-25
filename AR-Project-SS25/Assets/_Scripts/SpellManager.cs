using System;
using System.Collections.Generic;
using PixPlays.ElementalVFX;
using Sirenix.OdinInspector;
using UnityEngine;


public class SpellManager : MonoBehaviour
{
	public static SpellManager Instance;
    [SerializeField] private Transform centerMarker;
    [SerializeField] private Transform[] lifeCrystals;
    [SerializeField] private GameObject spellPreview;
    private SpellType _currentSpellType;
    private ElementType _currentElementType;
    private GameObject[] _currentShieldObject = new GameObject[2];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        Debug.Log("Starting Spell Manager");
        spellPreview.SetActive(false);
    }
    
    public void SetSpellPreviewActive(bool active)
    {
        spellPreview?.SetActive(active);
    }


    public bool SpawnSpell(SpellType spellType, ElementType elementType)
    {
        // !!! THIS WILL ONLY SPAWN VFX AND PREVIEW ICONS !!!
        // DAMANGE IS HANDLED BY SpellDamage.cs
        bool isPlayerTurn = GameStateManager.Instance.IsCurrentPlayersTurn();
    
        // Prevent duplicate spell preview when not player's turn
        if (!isPlayerTurn)
        {
            if (spellType == _currentSpellType)
                return false;
    
            SpawnSpellPreview(spellType);
            return false;
        }
    
        // Only spawn attack if a valid element is selected
        if (elementType != ElementType.None)
        {
            SpawnSpellAttack(spellType, elementType);
            _currentSpellType = spellType;
            _currentElementType = elementType;
            return true;
        }
    
        // Show preview if no valid element is selected
        SpawnSpellPreview(spellType);
        return false;
    }
    
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

    private void SpawnSpellAttack(SpellType spellType, ElementType elementType, int duration = 5)
    {
        var spellData = DataManagement.Instance.spellDataList.Find(data => data.Recipe == spellType);
        var visualData = spellData.GetPrefabTuple(elementType);
        if (visualData == null)
        {
            Debug.LogError($"No prefab found for spell {spellType} and element {elementType}");
            return;
        }
    
        var currentPlayerId = (int) GameStateManager.Instance.activePlayerClientId.Value;
    
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        Vector3 targetPosition;
        float radius = 0.5f; // Default radius
        
        switch (spellType)
        {
            case SpellType.GroundPound:
                (spawnPosition, spawnRotation) = GetGroundPoundSpawn(currentPlayerId);
                targetPosition = GridManagement.Instance.enemyHealthVisual.transform.position;
                radius = 1.0f;
                break;
            case SpellType.Shield:
                return; // Will be handled by GameStateManager
            default:
                (spawnPosition, spawnRotation) = GetDefaultSpellSpawn(currentPlayerId);
                targetPosition = GridManagement.Instance.enemyHealthVisual.transform.position;
                break;
        }
        
        _ = InstantiateSpellPrefab(duration, visualData, spawnPosition, spawnRotation, targetPosition, radius);
    }

    private static GameObject InstantiateSpellPrefab(int duration, SpellVisualData visualData, Vector3 spawnPosition,
        Quaternion spawnRotation, Vector3 targetPosition, float radius)
    {
        var spellInstance = Instantiate(visualData.VisualPrefab, spawnPosition, spawnRotation);

        var vfxData = new VfxData(
            spawnPosition,
            targetPosition,
            duration,
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

        return spellInstance;
    }

    private (Vector3, Quaternion) GetGroundPoundSpawn(int currentPlayerId)
    {
        int enemyId = (currentPlayerId == 0) ? 1 : 0; // TODO: do we need this or maybe when spawning crystal?
        var crystalPosition = GridManagement.Instance.enemyHealthVisual.transform.position;
        Vector3 spawnPosition = crystalPosition;
        Quaternion spawnRotation = Quaternion.identity;
        return (spawnPosition, spawnRotation);
    }
    
    private (Vector3, Quaternion) GetShieldSpawn(int currentPlayerId)
    {
        Vector3 spawnPosition = GridManagement.Instance.playerHealthVisual.transform.position; // TODO: maybe need distance from crystal
        Vector3 direction = (centerMarker.position - spawnPosition).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(direction);
        return (spawnPosition, spawnRotation);
    }
    
    private (Vector3, Quaternion) GetDefaultSpellSpawn(int currentPlayerId)
    {
        int enemyId = (currentPlayerId == 0) ? 1 : 0; // TODO: do we need this or maybe when spawning crystal?
        var crystalPosition = GridManagement.Instance.enemyHealthVisual.transform.position;
        Vector3 direction = (crystalPosition - centerMarker.position).normalized;
        Vector3 spawnPosition = centerMarker.position; // TODO: maybe needs to be closer or further away from crystal
        Quaternion spawnRotation = Quaternion.LookRotation(direction);
        return (spawnPosition, spawnRotation);
    }
    
    public void RemoveShield(int playerId)
    {
        if (_currentShieldObject[playerId] != null)
        {
            Destroy(_currentShieldObject[playerId]);
            _currentShieldObject[playerId] = null;
        }
        else
        {
            Debug.LogWarning($"No shield object to remove for player {playerId}");
        }
    }
    
    public void SetShield(int playerId, ElementType elementType)
    {
        var spawn = GetShieldSpawn(playerId);
        var spawnPosition = spawn.Item1;
        var spawnRotation = spawn.Item2;
        var targetPosition = GridManagement.Instance.playerHealthVisual.transform.position;
        var spellData = DataManagement.Instance.spellDataList.Find(data => data.Recipe == SpellType.Shield);
        var visualData = spellData.GetPrefabTuple(elementType);
        var shieldObject = InstantiateSpellPrefab(-1, visualData, spawnPosition, spawnRotation, targetPosition, 0.5f);
        _currentShieldObject[playerId] = shieldObject;
    }
}