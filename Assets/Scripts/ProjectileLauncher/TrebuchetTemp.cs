using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetTemp : MonoBehaviour
{
    public Rigidbody weight;
    public GameObject projectile;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Release weight
            weight.isKinematic = false;
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            // Launch projectile
            HingeJoint projectileToArmHinge = projectile.GetComponent<HingeJoint>();
            Destroy(projectileToArmHinge);
        }
    }
}
