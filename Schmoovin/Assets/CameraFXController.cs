using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS;

public class CameraFXController : MonoBehaviour
{

    CameraFilterPack_Drawing_Manga_Flash flash;
    CameraFilterPack_Blur_Radial radialBlur;
    CameraFilterPack_Color_GrayScale grayScale;
    CameraFilterPack_Blur_GaussianBlur gaussianBlur;
    CameraFilterPack_Color_BrightContrastSaturation brightConSat;

    [SerializeField, Tooltip("The blur intensity at the character's max speed")]
    private float maxRadialBlurIntensity = -0.032f;
    [SerializeField, Tooltip("The character speed at which the radial blur maxes out")]
    private float maxRadialBlurSpeed = 30f;
    [SerializeField, Tooltip("The character speed at which the radial blur maxes out")]
    private float radialBlurClampSpeed = 20f;

    //effect specific variables
    float brightnessMin = 0.75f;
    float brightnessMax = 2.2f;

    float saturationMin = 0.35f;
    float saturationMax = 1.5f;

    float contrastMin = 0.9f;
    float contrastMax = 1.05f;

    float defaultBrightConSat = 1f;
    float defaultTimeScale = 1f;

    //timescale variables
    [SerializeField, Tooltip("The blur intensity at the character's max speed")]
    private InputCharacterTimeDS inputTime = null;

    // Start is called before the first frame update
    void Start()
    {
        //initialize FX variables
        flash = GetComponent<CameraFilterPack_Drawing_Manga_Flash>();
        radialBlur = GetComponent<CameraFilterPack_Blur_Radial>();
        grayScale = GetComponent<CameraFilterPack_Color_GrayScale>();
        gaussianBlur = GetComponent<CameraFilterPack_Blur_GaussianBlur>();
        brightConSat = GetComponent<CameraFilterPack_Color_BrightContrastSaturation>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void timedFX(string action, float length)
    {
        StartCoroutine(TimedFXCoroutine(action, length));
    }

    IEnumerator TimedFXCoroutine(string action, float length)
    {
        switch (action)
        {
            case "airdash":
                flash.enabled = true;
                radialBlur.enabled = true;
                break;
            case "gpland":
                gaussianBlur.enabled = true;
                break;
            default:
                break;
        }

        //play effect for set length
        yield return new WaitForSeconds(length);

        switch (action)
        {
            case "airdash":
                flash.enabled = false;
                radialBlur.enabled = false;
                break;
            case "gpland":
                gaussianBlur.enabled = false;
                break;
            default:
                break;
        }
    }

    public void startFX(string effect)
    {
        switch (effect)
        {
            case "grayscale":
                grayScale.enabled = true;
                break;
            case "timescale":
                brightConSat.enabled = true;
                break;
            case "grounddash":
            case "glide":
                radialBlur.enabled = true;
                break;
            default:
                break;
        }
    }

    public void stopFX(string effect)
    {
        switch (effect)
        {
            case "grayscale":
                grayScale.enabled = false;
                break;
            case "timescale":
                grayScale.enabled = false;
                break;
            case "grounddash":
            case "glide":
                radialBlur.enabled = false;
                break;
            default:
                break;
        }
    }

    public void toggleFX(string effect)
    {
        switch (effect)
        {
            case "grounddash":
                if (radialBlur.enabled)
                {
                    radialBlur.enabled = false;
                }
                else
                {
                    radialBlur.enabled = true;
                }
                break;
            default:
                break;
        }
    }

    public void updateFX(string effect, float brightness = 0f, float saturation = 0f, float contrast = 0f)
    {

    }

    public void scaleFXWithTime()
    {
        //Debug.Log(NeoFpsTimeScale.timeScale);
        //brightConSat.Brightness = Time.timeScale;
        //brightConSat.Saturation = NeoFpsTimeScale.timeScale;
        //if (Mathf.Abs(NeoFpsTimeScale.timeScale - 1f) < 0.05f)
        //{
        //    brightConSat.Brightness = 1f;
        //    brightConSat.Saturation = 1f;
        //    brightConSat.Contrast = 1f;
        //}
        //else
        //{
        if (NeoFpsTimeScale.timeScale > defaultTimeScale)// if (NeoFpsTimeScale.timeScale - 1f > 0.01f)
        {
            brightConSat.Brightness = defaultBrightConSat + ((brightnessMax - defaultBrightConSat) / (inputTime.M_FastTimeScale - defaultTimeScale)) * (NeoFpsTimeScale.timeScale - defaultTimeScale);
            brightConSat.Saturation = defaultBrightConSat + ((saturationMax - defaultBrightConSat) / (inputTime.M_FastTimeScale - defaultTimeScale)) * (NeoFpsTimeScale.timeScale - defaultTimeScale);
            brightConSat.Contrast = contrastMin + ((contrastMax - defaultBrightConSat) / (inputTime.M_FastTimeScale - defaultTimeScale)) * (NeoFpsTimeScale.timeScale - defaultTimeScale);
        }
        else// if (NeoFpsTimeScale.timeScale - 1f < -0.01f)
        {
            brightConSat.Brightness = brightnessMin + ((defaultBrightConSat - brightnessMin) / (defaultTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);
            brightConSat.Saturation = saturationMin + ((defaultBrightConSat - saturationMin) / (defaultTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);
            brightConSat.Contrast = contrastMin + ((defaultBrightConSat - contrastMin) / (defaultTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);
        }
        //else
        //{
        //    brightConSat.Brightness = 1f;
        //    brightConSat.Saturation = 1f;
        //    brightConSat.Contrast = 1f;
        //}
        //brightConSat.Brightness = brightnessMin + ((brightnessMax - brightnessMin) / (inputTime.M_FastTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);
        //brightConSat.Saturation = saturationMin + ((saturationMax - saturationMin) / (inputTime.M_FastTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);
        //brightConSat.Contrast = contrastMin + ((contrastMax - contrastMin) / (inputTime.M_FastTimeScale - inputTime.M_SlowTimeScale)) * (NeoFpsTimeScale.timeScale - inputTime.M_SlowTimeScale);

        //}

    }

    public void scaleFXWithVelocity(float speed)
    {
        //Debug.Log(speed);
        float adjustedSpeed = Mathf.Clamp(maxRadialBlurSpeed - speed, 0f, 10f);
        radialBlur.Intensity = (10f - adjustedSpeed) * maxRadialBlurIntensity / 10f;
        //radialBlur.Intensity = Mathf.Min(maxRadialBlurSpeed, speed) * maxRadialBlurIntensity / maxRadialBlurSpeed;
        //0 - 0
        //30 - -0.032
    }
}
