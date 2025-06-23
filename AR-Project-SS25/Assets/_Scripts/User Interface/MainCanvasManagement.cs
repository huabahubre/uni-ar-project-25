using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasManagement : Singleton<MainCanvasManagement>
{
    
    [ShowInInspector, ReadOnly, BoxGroup("CanvasPageControllerIX")] protected List<CanvasPage> CanvasPages;


    [FoldoutGroup("References")] public GameObject GlobalMessagePanel;
    [FoldoutGroup("References")] public TextMeshProUGUI GlobalMessageText;
    [FoldoutGroup("References")] public Image GlobalMessageIcon;
    [FoldoutGroup("References")] public Image GlobalMessageIconBg;
    [FoldoutGroup("References")] public Image GlobalMessageBg;
    
    [FoldoutGroup("References")] public Sprite MessageIcon_Info;
    [FoldoutGroup("References")] public Sprite MessageIcon_Warning;
    [FoldoutGroup("References")] public Sprite MessageIcon_Error;
    [FoldoutGroup("References")] public Sprite MessageIcon_Success;
    
    [FoldoutGroup("References")] public Color MessageColor_Info;
    [FoldoutGroup("References")] public Color MessageColor_Warning;
    [FoldoutGroup("References")] public Color MessageColor_Error;
    [FoldoutGroup("References")] public Color MessageColor_Success;
    

    public enum MessageType
    {
        INFO,
        WARNING,
        ERROR,
        SUCCESS
    }
    
    private Coroutine messageCoroutine;
    
    protected virtual void Awake()
    {
        base.Awake();
        RefreshCanvasPageList();
    }

    void Start()
    {
        // GlobalMessagePanelAnim.SetBool("Active", false);
        
        Initialize();
    }


    #region Inspector Methods

    [Button, BoxGroup("CanvasPageControllerIX")]
    public void RefreshCanvasPageList()
    {
        // Debug.Log("Refreshing UI Pages");
        CanvasPages = new List<CanvasPage>();
        FindAllCanvasPages();
        HideAllPages();
    }

    #endregion
    
    #region Initialize
    
    protected virtual void FindAllCanvasPages(Transform parent = null)
    {
        // Get the type of the script
        System.Type scriptType = System.Type.GetType("CanvasPage");
        if (scriptType == null)
        {
            Debug.Log("Script type not found: " + "CanvasPage");
            return;
        }

        // Set Parent to own, if empty
        if (parent == null)
            parent = this.transform;

        // Check if the parent has the script and add it to the list if it does
        CanvasPage CanvasPageScript = parent.GetComponent<CanvasPage>();
        if (CanvasPageScript != null)
        {
            CanvasPages.Add(CanvasPageScript);
        }

        // Iterate through all children (including inactive ones)
        foreach (Transform child in parent.transform)
        {
            FindAllCanvasPages(child);
        }
    }


    void Initialize()
    {
        foreach (var canvasPage in CanvasPages)
        {
            canvasPage.Initialize();
        }
    }

    #endregion



    public virtual bool isPageActive(string pageName)
    {
        // Get Page
        if (!GetCanvasPage(pageName, out CanvasPage CanvasPage))
        {
            return false;
        }

        return CanvasPage.gameObject.activeSelf;
    }
    
    
    
    public virtual void ShowPage(string pageName)
    {
        // Get Page
        if (!GetCanvasPage(pageName, out CanvasPage CanvasPage))
        {
            return;
        }
        
        // Hide all other Pages
        HideAllPages();
        
        // Show Page
        CanvasPage.Show();
    }
    
    public virtual void ShowPage(CanvasPage CanvasPage)
    {
        // Show Page
        CanvasPage.Show();
    }
    
    
    
    public virtual void HidePage(string pageName)
    {
        // Get Page
        if (!GetCanvasPage(pageName, out CanvasPage CanvasPage))
        {
            return;
        }
        
        // Show Page
        CanvasPage.Hide();
    }
    
    public virtual void HidePage(CanvasPage CanvasPage)
    {
        // Show Page
        CanvasPage.Hide();
    }
    
    
    
    public virtual void HideAllPages()
    {
        foreach (var VARIABLE in CanvasPages)
        {
            VARIABLE.Hide();
        }
    }

    private bool GetCanvasPage(string pageName, out CanvasPage CanvasPage)
    {
        CanvasPage = null;
        
        List<CanvasPage> toShowPages = null;
        toShowPages = CanvasPages.FindAll(page => page.Name.Equals(pageName));

        if (toShowPages.Count == 0)
        {
            Debug.Log("Couldn't find CanvasPage for: " + pageName);
            return false;
        }
        if (toShowPages.Count > 1)
        {
            Debug.Log("Found more than one CanvasPage for: " + pageName);
        }

        CanvasPage = toShowPages[0];
        return true;
    }
    
    
    #region Global Message

    [Button]
    public void ShowMessage(string message,  MessageType messageType = MessageType.INFO, int duration = 5)
    {
        if (GlobalMessagePanel == null || GlobalMessageText == null)
        {
            Debug.LogWarning("GlobalMessagePanel or GlobalMessageText is not assigned.");
            return;
        }

        // Color
        Color ogColor = Color.black;
        
        switch (messageType)
        {
            case MessageType.INFO:
                GlobalMessageIcon.sprite = MessageIcon_Info;

                ogColor = MessageColor_Info;
                GlobalMessageIconBg.color = MessageColor_Info;
                GlobalMessageBg.color = MessageColor_Info;
                break;
            case MessageType.WARNING:
                GlobalMessageIcon.sprite = MessageIcon_Warning;
                ogColor = MessageColor_Warning;
                GlobalMessageIconBg.color = MessageColor_Warning;
                GlobalMessageBg.color = MessageColor_Warning;
                break;
            case MessageType.ERROR:
                GlobalMessageIcon.sprite = MessageIcon_Error;
                ogColor = MessageColor_Error;
                break;
            
            case MessageType.SUCCESS:
                GlobalMessageIcon.sprite = MessageIcon_Success;
                ogColor = MessageColor_Success;
                break;
        }
        
        
        // Setting Color
        GlobalMessageIconBg.color = ogColor;
        Color fadedOgColor = ogColor;
        fadedOgColor.a = 0.5f;
        GlobalMessageBg.color = fadedOgColor;

        GlobalMessagePanel.SetActive(true);
        GlobalMessageText.text = message;

        // If a message is already being displayed, stop it and restart
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
    }

    private IEnumerator HideMessageAfterDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        GlobalMessagePanel.SetActive(false);
    }

    #endregion
    
    
}
