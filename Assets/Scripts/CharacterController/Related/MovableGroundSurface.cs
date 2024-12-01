using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To tell <see cref="MyProject.CharacterController"/> that this ground surface can potentially move, so that movement velocity can be supplied to the CharacterController.Move().
/// 
/// <para>Because CharacterController.Move() doesn't support moving child with parent transform if it's called that frame. So have to manually figure out platform's velocity here, and add with player's current velocity.</para>
/// 
/// Whatever this object is, should be considered ground by <see cref="MyProject.CharacterController.IsGrounded"/>. Otherwise this will not work as expected.
/// 
/// <para>Also if this object is going to move, then it should have a <see cref="MyProject.CharacterController"/> component on it, unless some other way to implement relative velocity for character controller.</para>
/// </summary>
public class MovableGroundSurface : MonoBehaviour
{
    MyProject.CharacterController movingObject;
    CharacterController charController;
    public Vector3 velocity
    {
        get
        {
            if(!charController)
            {
                throw new System.Exception($"No character controller found for this {this.GetType().Name}.");
            }

            return charController.velocity;
        }
    }

    private void Start()
    {
        movingObject = GetComponent<MyProject.CharacterController>();
        charController = movingObject.charController;
    }
}
