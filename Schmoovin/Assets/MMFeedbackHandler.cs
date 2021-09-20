using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMFeedbackHandler : MonoBehaviour
{
    public MMFeedbacks feedbacks;

    // Start is called before the first frame update
    void Start()
    {
        //if (GetComponent<MMFeedbacks>() != null)
        //{
        //    feedbacks = GetComponent<MMFeedbacks>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void triggerMMFeedbacks()
    {
        Debug.Log("trigger feedbacks");
        if (feedbacks != null)
            feedbacks.PlayFeedbacks();
    }
}
