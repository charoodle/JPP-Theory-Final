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
///     (tested) x Window sizing
///     (tested) x Anchored position (basic x/y functionality - 0,0 represents center?)
///     (tested) x Box active state
///     (tested) x Box destroyable
/// 
/// </summary>
public class InfoBox : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI textField;

    /// <summary> Rect transform that is considered the entire info box window itself. </summary>
    protected RectTransform window;

    /// <summary>
    /// What info box holds inside it.
    /// </summary>
    protected string textContent;

    const float WINDOW_DEFAULT_WIDTH = 430f;
    const float WINDOW_DEFAULT_HEIGHT = 330f;

    /// TODO: Use these to normalize x/y to a 0-1 range in <see cref="SetWindowAnchorPosition(float,float)"/> ?
    float canvasWidth;
    float canvasHeight;

    private void Awake()
    {
        window = GetComponent<RectTransform>();

        // Get pixel size of canvas, to be able to normalize a value between 0-1?
        /// Not sure if this is what I want to use to normalize x/y in <see cref="SetWindowAnchorPosition(float, float)".
        Canvas windowCanvas = window.GetComponentInParent<Canvas>();
        canvasWidth = windowCanvas.pixelRect.width;
        canvasHeight = windowCanvas.pixelRect.height;

        // Default size of window.
        SetWindowSize(WINDOW_DEFAULT_WIDTH, WINDOW_DEFAULT_HEIGHT);
    }

    /// <param name="horizontal">How wide window should be.</param>
    /// <param name="vertical">How high window should be.</param>
    public void SetWindowSize(float horizontal, float vertical)
    {
        window.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, horizontal);
        window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vertical);
    }

    /// <summary>
    /// 0,0 represents center of current canvas? Depends? Not 100% sure. Currently anchored to middle of screen.
    /// </summary>
    /// <param name="position"></param>
    public void SetWindowAnchorPosition(Vector2 position)
    {
        window.anchoredPosition = position;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetWindowAnchorPosition(float x, float y)
    {
        // I believe x/y is based off the parented Canvas Scaler's reference resolution, where 0,0 represents the center.
        SetWindowAnchorPosition(new Vector2(x, y));
    }

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

    #region Alignment Enums
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
    #endregion
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
