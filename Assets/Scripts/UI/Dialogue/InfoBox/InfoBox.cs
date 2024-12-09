using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A UI window that you can change *a single* text info inside to position UI info on screen from any script.
/// Can change:
///     (tested) x Text content
///     (tested) x Font sizing   
///     (tested) x Font alignment (vertical & horizontal)
///       Window sizing
///       Anchored position
///     (tested) x Box active state
///     (tested) x Box destroyable
/// 
/// </summary>
public class InfoBox : MonoBehaviour
{
    #region DELETE AFTER DONE TESTING
    private void Start()
    {
        // TODO: Debug only - DELETE AFTER DONE
        SetInfo("Hello world!");
        SetInfoFontSize(80f);
        SetHorizontalFontAlignment(HorizontalTextAlign.Right);
        SetVerticalFontAlignment(VerticalTextAlign.Middle);
    }
    #endregion

    [SerializeField] protected TextMeshProUGUI textField;

    /// <summary>
    /// What info box holds inside it.
    /// </summary>
    protected string textContent;



    /// <summary>
    /// Turn the info box on and off.
    /// </summary>
    /// <param name="value"></param>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void SetInfo(string info)
    {
        textContent = info;
        UpdateTextContents();
    }

    public void SetInfoFontSize(float size)
    {
        textField.fontSize = size;
    }

    public enum HorizontalTextAlign
    {
        Left,
        Center,
        Right
    }
    public enum VerticalTextAlign
    {
        Top,
        Middle,
        Bottom
    }
    public void SetHorizontalFontAlignment(HorizontalTextAlign alignment)
    {
        // Default to left?? Can't leave it null
        HorizontalAlignmentOptions option = HorizontalAlignmentOptions.Left;

        switch (alignment)
        {
            case HorizontalTextAlign.Left:
                option = HorizontalAlignmentOptions.Left;
                break;
            case HorizontalTextAlign.Center:
                option = HorizontalAlignmentOptions.Center;
                break;
            case HorizontalTextAlign.Right:
                option = HorizontalAlignmentOptions.Right;
                break;
            default:
                throw new System.Exception($"Horizontal alignment not supported: {alignment}");
        }

        textField.horizontalAlignment = option;
    }

    public void SetVerticalFontAlignment(VerticalTextAlign alignment)
    {
        // Default to top?? Can't leave it null
        VerticalAlignmentOptions option = VerticalAlignmentOptions.Top;

        switch (alignment)
        {
            case VerticalTextAlign.Top:
                option = VerticalAlignmentOptions.Top;
                break;
            case VerticalTextAlign.Middle:
                option = VerticalAlignmentOptions.Middle;
                break;
            case VerticalTextAlign.Bottom:
                option = VerticalAlignmentOptions.Bottom;
                break;
            default:
                throw new System.Exception($"Vertical alignment not supported: {alignment}");
        }

        textField.verticalAlignment = option;
    }

    /// <summary>
    /// Destroys the info box permanently.
    /// </summary>
    public void DestroyPermanently()
    {
        Destroy(gameObject);
    }

    protected void UpdateTextContents()
    {
        textField.SetText($"{textContent}");
    }
}
