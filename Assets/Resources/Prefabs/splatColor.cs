using UnityEngine;
using System.Collections;

public class splatColor : MonoBehaviour {


    public Color color;
	// Use this for initialization
	void Start () {
        GetComponent<ParticleSystem>().startColor = color;

    }

	
	
}
