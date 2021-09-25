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
    private IMotionController motionController;
    //public bool groundedOnGravChange
    //{
    //    get;
    //    set;
    //}
    public bool gravityChanged
    {
        get;
        set;
    }
    //private MotionController motionController;
    //private UnityAction flip;

    // Start is called before the first frame update
    void Start()
    {
        neoCharacterController = GetComponent<NeoCharacterController>();
        motionController = GetComponent<MotionController>();
        gravityChanged = false;
        //motionController = GetComponent<MotionController>();
        //flip += ProcessFlip;
    }

    //private void Update()
    //{
    //    //motionController.motionGraph.AddEventListener(Animator.StringToHash("flip"), flip);
    //}

    protected override void UpdateInput()
    {
        //Debug.Log("GRAVITY");
        //throw new System.NotImplementedException();
        //Debug.Log("GRAV CHANGED");

        //reverse gravity and up vector
        bool gravityChange = GetButtonDown(FpsInputButton.Inspect) && !neoCharacterController.isGrounded;

        if (gravityChange)
        {
            //Debug.Log("GRAVITY");
            neoCharacterController.characterGravity.gravity *= -1f;
            gravityChanged = true;
            //if (!gravityReversed)
            //    gravityReversed = true;
            //else
            //    gravityReversed = false;
            //check grounded state
            //if (motionController.characterController.isGrounded)
            //    groundedOnGravChange = true;
            //else
            //    groundedOnGravChange = false;
        }
        else
        {
            //gravityReversed = false;
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
