using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flash the icon's alpha back forth.
/// </summary>
public class ContinueIconFlash : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] float frequency = 1f;
    float time = 0f;

    private void OnEnable()
    {
        // Start at 0 alpha.
        //  If start at full alpha; seems too distracting when it pops in.
        time = 0f;

        // Reset image to 0 alpha
        Color color = image.color;
        color.a = 0f;
        image.color = color;
    }

    private void Update()
    {
        // Sin wave + its absolute value = flashing effect over time
        //  Should start at 0 each time its enabled
        float alpha = Mathf.Abs(Mathf.Sin(frequency * time));
        Color color = image.color;
        color.a = alpha;
        image.color = color;

        // Manually keep track of timer so can restart at 0 each time
        time += Time.deltaTime;
    }
}
