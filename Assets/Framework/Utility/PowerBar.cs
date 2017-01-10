using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerBar : MonoBehaviour {

    public float sliderSpeed = 2f;
    Slider slider;

    public bool isRunning = false;         
    public bool hasLaunched = false;


    float startTime;
	// Use this for initialization
	void Start () {
        startTime = Time.time;
        slider = GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //Ignore if not playing, has launched, or if not active yet 
       if (!LevelController.instance.isPlaying | isRunning == false | hasLaunched)
            return;

        //Progress the slider bar 
        slider.value = Time.time - startTime;
        //Cap value 
        if (slider.value >= 1f)
            startTime = Time.time;
	}
}
