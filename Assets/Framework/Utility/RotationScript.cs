using UnityEngine;
using System.Collections;

public class RotationScript : MonoBehaviour {

    public float rotationSpeed;

    public bool isUpright = false;

    float sideSpin = 0f;

	// Use this for initialization
	void Start () {
        if (isUpright)
        {
            transform.rotation = new Quaternion(-0.7f,0.1f,-0.1f,0.7f);
            sideSpin = Random.Range(0f,150f);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isUpright)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right * sideSpin * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        }
	}
}
