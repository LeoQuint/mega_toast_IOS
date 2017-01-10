using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Camera_Follow : MonoBehaviour {

    public Transform target;
    public GameObject ingredientList;

    Rigidbody playerRb;

    Quaternion startingRot = new Quaternion(0f,0f,0f,1f);
    Quaternion overHeadRot = new Quaternion(0f,0.7f,-0.7f,0f);
    Quaternion midOverhead = new Quaternion(0.4f,0.6f,-0.4f, 0.6f);
    Quaternion endRot;
    Vector3 overHeadPos = new Vector3(-0.03f, 23.5f, -7.5f);


    Vector3 endPos;

    public float offsetDown = 5f;
    public float offsetUp = -5f;
    private float camOffset;
    

    float stepStartTime;
    float flipDuration = 0.5f;

    delegate void mDelegate();
    mDelegate mDel;


    public bool overHeadFollow = true;
    bool asFliped = false;

    //public references:
    public Player pScript;

    private Vector3 startingPos;
    private Quaternion initialRotation;

    void Awake()
    {
        startingPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        initialRotation = transform.rotation;
    }

    void Start() 
    {
        Debug.Log(transform.position);

        
        playerRb = target.GetComponent<Rigidbody>();
        camOffset = offsetUp;
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
  
        if (pScript.playerStatus == PlayerStatus.INTRO || pScript.playerStatus == PlayerStatus.CHARGING)
        {
            return;
        }
        if (pScript.playerStatus == PlayerStatus.LANDED || pScript.playerStatus == PlayerStatus.DEAD)
        {
            return;
        }
        if (mDel != null)
        {
            mDel();
        }
        if (target != null)
        {
            if (pScript.playerStatus == PlayerStatus.GOINGDOWN && transform.position.y < 18f)
            {
                pScript.ChangeToOverhead();
               
                if (!asFliped)
                {
                    ingredientList.SetActive(false);
                    asFliped = true;
                    mDel += Flip;
                    LevelController.instance.ClearForLanding();
                    endRot = midOverhead;
                    stepStartTime = Time.time;
                    endPos = overHeadPos;

                    //playerRb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    Debug.LogError("FlipStarts");
                }

                return;
            }
            else if (pScript.playerStatus == PlayerStatus.OVERHEAD || asFliped)
            {
               
                return;
            }


            
            if (playerRb.velocity.y > 0f)
            {
                camOffset = offsetUp;
            }
            else
            {
                camOffset = offsetDown;
            }



            Vector3 adjustedPos = new Vector3(0f, transform.position.y, -12f);
            Vector3 adjustedPosTarget = new Vector3(0f, target.position.y + camOffset, -12f);
            transform.position = Vector3.Lerp(adjustedPos, adjustedPosTarget, Mathf.Abs( playerRb.velocity.y) * Time.deltaTime);
            
        }
        
	}
    bool reachedMidFlip = false;
    void Flip()
    {
        
        float step = (Time.time - stepStartTime) / flipDuration;

        transform.rotation = Quaternion.Lerp(startingRot, endRot, step);

        transform.position = Vector3.Lerp(transform.position, endPos, step);


        if (step >= 1f)
        {
            mDel -= Flip;
            mDel += FlipPartTwo;
            stepStartTime = Time.time;
            endRot = overHeadRot;
            startingRot = transform.rotation;
            
        }
    }
    void FlipPartTwo()
    {
        float step = (Time.time - stepStartTime) / flipDuration;
        transform.rotation = Quaternion.Lerp(startingRot, endRot, step);

        transform.position = Vector3.Lerp(transform.position, endPos, step);
        if (step >= 1f)
        {
            mDel -= FlipPartTwo;
            if (overHeadFollow)
            {
                mDel += OverHeadFollow;

                StartCoroutine(FinalPhaseDelay());
            }
        }
    }
    IEnumerator FinalPhaseDelay()
    {
        yield return new WaitForSeconds(0f);
        Time.timeScale = 0f;
        StartCoroutine(FinalViewDelay());
    }
    IEnumerator FinalViewDelay()
    {
        
        yield return new WaitForSecondsRealtime(0f);
        Physics.gravity = new Vector3(0f,-2.3f, 0f);
        Time.timeScale = 1f;
        pScript.FlipBool(false);
        
    }
    void OverHeadFollow() 
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(-0.03f, target.position.y + 3f, -7.5f), Mathf.Abs(playerRb.velocity.y) * Time.deltaTime);
    }
    public void ResetValues()
    {
        mDel = null;
        reachedMidFlip = false;
        asFliped = false;
        transform.rotation = initialRotation;
        transform.position = startingPos;
        camOffset = offsetUp;
        ingredientList.SetActive(true);
    }

}
