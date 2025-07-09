using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage : MonoBehaviour
{
    
    [BoxGroup("CanvasPage")] public string Name;

    [SerializeField, ReadOnly, BoxGroup("Runtime")]
    protected bool initialized = false;
    
    
    #region Init

    public virtual void Initialize()
    {
        if(initialized)
            return;

        initialized = true;
    }
    
    
    #endregion
    
    #region Hide & Show

    
    public void Hide()
    {
        this.OnHide();
        this.gameObject.SetActive(false);
    }


    public virtual void OnHide()
    {
        
    }
    
    public virtual void Show()
    {
        this.OnShow();
        this.gameObject.SetActive(true);
        RefreshAllLayoutGroups();
    }

    public virtual void OnShow()
    {
    }
    
    
    public virtual void RefreshAllLayoutGroups()
    {
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        // Wait one frame to ensure layout is stable
        yield return null;

        var layouts = GetComponentsInChildren<LayoutGroup>(true);

        foreach (var layout in layouts)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
        }
    }

    #endregion
    
    #region Data

    public virtual void Refresh()
    {
        
    }

    #endregion
    
}