using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairColorAdjuster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Color32 colorRed = Color.red;
        Color32 colorBlack = Color.black;

        Transform outlineTransform = transform.Find("Outline");
        Transform centerTransform = transform.Find("Center");

        outlineTransform.GetComponent<Image>().color = colorBlack;
        centerTransform.GetComponent<Image>().color = colorRed;
    }

}
