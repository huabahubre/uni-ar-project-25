using Sirenix.OdinInspector;
using UnityEngine;

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
    }

    public virtual void OnShow()
    {
        
    }

    #endregion
    
    #region Data

    public virtual void Refresh()
    {
        
    }

    #endregion
    
}