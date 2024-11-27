using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will be parented as a child game object of a trebuchet.
/// </summary>
public class TrebuchetRotatorInteractable : Interactable
{
    Trebuchet trebuchet;

    [SerializeField] protected bool RotatesLeft;
    [SerializeField] protected bool RotatesRight;

    [SerializeField] protected bool _isRotating;

    [SerializeField] TrebuchetRotatorInteractable otherRotateInteractable;

    public bool isRotating
    {
        get
        {
            return _isRotating;
        }
        set
        {
            // Directly update other button
            otherRotateInteractable._isRotating = value;
            otherRotateInteractable.UpdateInteractText(value);

            // Update interact text back appropriately
            UpdateInteractText(value);

            _isRotating = value;
        }
    }

    private void UpdateInteractText(bool isRotating)
    {
        if (RotatesLeft)
            interactText = "Rotate left.";
        if (RotatesRight)
            interactText = "Rotate right.";

        // If being rotated, say unavailable.
        if (isRotating)
            interactText += "\n(Unavailable).";
    }

    protected override void Start()
    {
        // Get parent trebuchet
        trebuchet = GetComponentInParent<Trebuchet>();

        base.Start();
    }

    private void OnValidate()
    {
        interactText = "Doesn't do anything.";

        // Can't have both
        if (RotatesLeft && RotatesRight)
            RotatesRight = false;

        UpdateInteractText(isRotating);
    }

    public override void InteractWith()
    {
        // Disallow interaction if currently rotating.
        if (isRotating)
            return;

        float rotateYAngle = 0f;

        if(RotatesLeft)
        {
            rotateYAngle = -15f;
        }
        if(RotatesRight)
        {
            rotateYAngle = 15f;
        }

        StartCoroutine(RotateTrebuchetCoroutine(rotateYAngle, 1f));
    }

    /// <summary>
    /// Slerp rotate the trebuchet around local y axis only.
    /// </summary>
    /// <param name="angle">Angle (degrees) to rotate. Positive = right. Negative = left.</param>
    /// <param name="duration">Must be within 0 and 10 seconds.</param>
    /// <returns></returns>
    IEnumerator RotateTrebuchetCoroutine(float angle, float duration)
    {
        isRotating = true;

        //trebuchet.transform.localRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, angle, 0f));
        //yield break;

        duration = Mathf.Clamp(duration, 0f, 10f);

        Quaternion start = trebuchet.transform.localRotation;
        Quaternion end = Quaternion.Euler(trebuchet.transform.localRotation.eulerAngles + new Vector3(0f, angle, 0f));

        float timer = 0f;
        while(timer < duration)
        {
            float pct = timer / duration;
            trebuchet.transform.localRotation = Quaternion.Slerp(start, end, pct);
            timer += Time.deltaTime;
            yield return null;
        }

        trebuchet.transform.localRotation = end;

        isRotating = false;
    }
}
