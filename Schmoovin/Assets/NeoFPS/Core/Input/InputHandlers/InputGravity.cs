using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS;
using NeoCC;
using NeoFPS.CharacterMotion;

public class InputGravity : CharacterInputBase
{
    //private INeoCharacterController neoCharacterController;
    private NeoCharacterController neoCharacterController;
    //private MotionController motionController;
    //private UnityAction flip;

    // Start is called before the first frame update
    void Start()
    {
        neoCharacterController = GetComponent<NeoCharacterController>();
        //motionController = GetComponent<MotionController>();
        //flip += ProcessFlip;
    }

    private void Update()
    {
        //motionController.motionGraph.AddEventListener(Animator.StringToHash("flip"), flip);
    }

    protected override void UpdateInput()
    {
        //throw new System.NotImplementedException();
        //Debug.Log("GRAV CHANGED");

        //reverse gravity and up vector
        bool gravityChange = GetButtonDown(FpsInputButton.Inspect);

        if (gravityChange)
        {
            neoCharacterController.characterGravity.gravity *= -1f;
        }

    }

    //void ProcessFlip()
    //{
    //    //update up vector during character flip
    //    if (neoCharacterController.orientUpWithGravity == false)
    //    {
    //        neoCharacterController.up
    //    }
    //}

}
