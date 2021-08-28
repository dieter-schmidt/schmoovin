using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS;
using NeoCC;

public class InputGravity : CharacterInputBase
{
    private INeoCharacterController neoCharacterController;


    // Start is called before the first frame update
    void Start()
    {
        neoCharacterController = GetComponent<INeoCharacterController>();
    }

    protected override void UpdateInput()
    {
        //throw new System.NotImplementedException();
        //Debug.Log("GRAV CHANGED");
        bool gravityChange = GetButtonDown(FpsInputButton.Inspect);

        if (gravityChange)
        {
            neoCharacterController.characterGravity.gravity *= -1f;
        }
    }

}
