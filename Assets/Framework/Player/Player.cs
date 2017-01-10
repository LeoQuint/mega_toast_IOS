//#define ANDROID_DEBUG //enable for debug overlay
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


using NS_Level;

public enum PlayerStatus 
{
    INTRO,
    CHARGING,
    GOINGUP,
    GOINGDOWN,
    OVERHEAD,
    LANDED,
    DEAD
}

public class Player : MonoBehaviour {

    //FOR DEBUG ON DEVICE
    public Text DEBUG_FIELD_DEVICE;

    public static Player instance;

    public GameObject player;

    //Delegate used to controls/movements.
    delegate void mDelegate();
    mDelegate mDel;

    
    public PlayerData pStats; //Player Stats. Holds the modifier and stats of a particular type of bread or player.

    [SerializeField]
    float flipDelay = 0.5f;

    public PlayerStatus playerStatus;

    Quaternion upRotation = new Quaternion(0f, 0f, 0f, 1f);
    Quaternion downRotation = new Quaternion(-1f, 0f, 0f, 0f);
    Quaternion initialRotation = new Quaternion(-0.7f, 0f, 0f, 0.7f);

    public BitArray playerStats = new BitArray(32);


    public float endBoostMinSpeed = 3f;


    public Mesh[] meshes;
    public Material[] materials;


    [SerializeField][Tooltip("The size of the dead zone for tilt control.")]
    float m_TiltDeadZone = 0.01f;
    [SerializeField]
    [Tooltip("The size of the dead zone for swiping.")]
    float m_SwipeDeadZone = 0.01f;
    /// OLD CODE

    GameObject fireLocation;//Pepperfire particle effect.

    public int ptsRequiredPerCoin = 25;

    Quaternion stepStartRotation;
    Quaternion stepEndRotation;

    private float stepStartTime;
    public float flipDuration = 1f;

    public float jumpForce = 500f;
    public float movementSpeed = 20f;
    float movement = 0f;
    float movementZ = 0f;
    public float tiltMovespeed = 2000f;
    public float swipeMovespeed = 3000f;
    public float maxXvelocity = 3f;
    public float maxUp = 30f;
    public float maxDown = -7.5f;
    public float pepperBoostDuration = 1.2f;

    //Landing values
    [SerializeField]
    float m_LandingPerfect = 0.1f;    
    [SerializeField]
    float m_LandingGreat = 0.5f;
    [SerializeField]
    float m_LandingMissed = 10f;

    Rigidbody rb;
    //UI elements
    public Button launchBtn;
    public Slider powerBar;
    public Text speedTracker;
    public Text scoreTracker;
    public Text heightTracker;
    public GameObject multiplierTracker;
    public GameObject landingResult;
    public GameObject scoreResults;
    public GameObject highScoreResults;
    public GameObject GameUI;

    private float lastVelocity;

    int score;

    public GameObject endMenu;

    public Image listDisplay;
    //Table
    public GameObject toaster;
    public GameObject plateAndBread;

    public int condimentCount;

    public GameObject endPoint;

    private List<bool> toppingCollected = new List<bool>();
    private List<bool> condimentCollected = new List<bool>();

    public bool enableTiltControls = false;
    public bool enableSwipeControls = false;

    bool hasStarted = false;

    public int pepperCollected = 0;
    private bool isEndGame = false;
    private bool isMidflight = false;
    bool intialTapHeldDown = false;             //Used to track if player is holding down on screen at game start
    private bool isFlipping = false;

    public static GameObject splatParticlesHolder;
    public List<GameObject> splatList = new List<GameObject>();

    void Awake()
    {
        //Instance gets removed by any new instance created.
        if (instance != null)
        {
            Destroy(instance);
        }
        fireLocation = player.transform.FindChild("fireLocation").gameObject;
        fireLocation.SetActive(false);
       
        //Set our current level to this levelController.
        instance = this;
        score = 0;
        hasStarted = false;
#if ANDROID_DEBUG
        DEBUG_FIELD_DEVICE.gameObject.SetActive(true);
#endif
    }
    void Start()
    {
        condimentCount = 0;
        rb = player.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        pepperCollected = 0;
        Physics.gravity = new Vector3(0f, -1.3f, 0f);

    }
    void Update()
    {
        //STATE: IF NOT YET STARTED
        if (!LevelController.instance.isPlaying || !hasStarted || isFlipping)
        {
            if (playerStatus == PlayerStatus.DEAD)
            {
                if (!isEndGame)
                {
                    isEndGame = true;
                    StartCoroutine(EndGameDelay());
                }
            }
            return;
        }

        //STATES: IF LANDED OR DEAD
        if (playerStatus == PlayerStatus.LANDED || playerStatus == PlayerStatus.DEAD)
        {
            if (!isEndGame)
            {
                isEndGame = true;
                StartCoroutine(EndGameDelay());
            }
            
            return;
        }

        //STATES: IF INTRO
        if (playerStatus == PlayerStatus.INTRO)
        {
            return;
        }


        if (mDel != null)
        {
            mDel();
        }

        //STATES: IF CHARGING 
        if (playerStatus == PlayerStatus.CHARGING)
        {
            //When player first holds down, track it and start the power bar
            if (intialTapHeldDown == false && Input.GetButtonDown("Jump"))
            {
                Debug.Log("PRIMING JUMP");
                powerBar.gameObject.GetComponent<PowerBar>().isRunning = true; 
                intialTapHeldDown = true;
            }
            else if(intialTapHeldDown && Input.GetButtonUp("Jump"))
            {
                Debug.Log("JUMPING");
                Jump();
            }

            return;
        }

        //STATES: PLAYING (OVERHEAD)
        if (playerStatus == PlayerStatus.OVERHEAD)
        {
            if (enableTiltControls)
                TiltControls();

            if (enableSwipeControls)
                SwipeControls();

#if UNITY_EDITOR   
            movement = Input.GetAxis("Horizontal");
            movementZ = Input.GetAxis("Vertical");
            rb.AddForce(new Vector3(movement, 0f, movementZ) * movementSpeed);
            //rb.velocity = new Vector3((movement * movementSpeed), rb.velocity.y, 0f);
#endif

        }
        else 
        {
            if (enableTiltControls)
                TiltControls();
            else if (enableSwipeControls)
                SwipeControls();
#if UNITY_EDITOR
            movement = Input.GetAxis("Horizontal");
            
            rb.AddForce(Vector3.right * movement * movementSpeed);
            //rb.velocity = new Vector3((movement * movementSpeed), rb.velocity.y, 0f);
#endif
        }

        
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxXvelocity, maxXvelocity), Mathf.Clamp(rb.velocity.y, maxDown, maxUp), 0f);

        if (playerStatus == PlayerStatus.OVERHEAD)
        {
            
            Vector3 clampedXZ = new Vector3(Mathf.Clamp(player.transform.position.x, -1f, 1f), player.transform.position.y, Mathf.Clamp(player.transform.position.z, -8.5f, -6.5f));
            player.transform.position = clampedXZ;
        }
        else
        {
            Vector3 clampedX = new Vector3(Mathf.Clamp(player.transform.position.x, -1.2f, 1.2f), player.transform.position.y, player.transform.position.z);
            player.transform.position = clampedX;
        }
        

        if (rb.velocity.y < 1.5f && playerStatus == PlayerStatus.GOINGUP && isMidflight)
        {
            playerStatus = PlayerStatus.GOINGDOWN;
            Physics.gravity = new Vector3(0f, -2.3f, 0f);
            stepStartTime = Time.time;
            stepStartRotation = upRotation;
            stepEndRotation = downRotation;

            foreach (bool b in toppingCollected)
            {
                if (!b)
                {
                    ExplosiveDeath();
                    return;
                }
            }
            

            LevelController.instance.SpawnCondimentsTurn();
            
            mDel += Flip;
            //mDel += MoveList; Moving the list disabled
        }
    }
    public void ChangeModel(string bread) 
    {
        switch(bread)
        {
            case "Breadley":
                player.transform.FindChild("Mesh").GetComponent<MeshFilter>().mesh = meshes[0];
                player.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material = materials[0];
               
                player.transform.FindChild("Mesh").rotation = new Quaternion(-0.7f, 0f, 0f, 0.7f);
                player.transform.FindChild("Mesh").localScale = new Vector3(1f, 1f, 1f);
                //end point
                endPoint.transform.localPosition = new Vector3(0f,0f,0.01f);
                endPoint.transform.localScale = new Vector3(1f, 1f, 1f);
                endPoint.transform.rotation = new Quaternion(0f, -1f, 0f, 0f);
                endPoint.GetComponent<MeshFilter>().mesh = meshes[0];
                endPoint.GetComponent<MeshRenderer>().material = materials[0];

                break;

            case "Bagel":
                player.transform.FindChild("Mesh").GetComponent<MeshFilter>().mesh = meshes[1];
                player.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material = materials[1];

                player.transform.FindChild("Mesh").rotation = new Quaternion(-0.2f, -0.7f, -0.7f, -0.2f);
                player.transform.FindChild("Mesh").localScale = new Vector3(1f, 1f, 1f);

                //end point
                endPoint.transform.localPosition = new Vector3(0f, 0f, 0f);
                endPoint.transform.localScale = new Vector3(1f, 1f, 1f);
                endPoint.transform.rotation = new Quaternion(1f, 0f, 0f, 0f);
                endPoint.GetComponent<MeshFilter>().mesh = meshes[1];
                endPoint.GetComponent<MeshRenderer>().material = materials[1];
                break;
            case "Pumpernickolas":
                player.transform.FindChild("Mesh").GetComponent<MeshFilter>().mesh = meshes[2];
                player.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material = materials[2];

                player.transform.FindChild("Mesh").rotation = new Quaternion(0f, 0.7f, 0.7f, 0f);
                player.transform.FindChild("Mesh").localScale = new Vector3(0.008f,0.008f,0.008f);

                //end point
                endPoint.transform.localPosition = new Vector3(0f, -0.0025f, 0.0025f);
                endPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                endPoint.transform.rotation = new Quaternion(1f, 0f, 0f, 0f);
                endPoint.GetComponent<MeshFilter>().mesh = meshes[2];
                endPoint.GetComponent<MeshRenderer>().material = materials[2];
                break;
            case "Zombread":
                
                player.transform.FindChild("Mesh").GetComponent<MeshFilter>().mesh = meshes[3];
                player.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material = materials[3];

                player.transform.FindChild("Mesh").rotation = new Quaternion(0f, 0.7f, 0.7f, 0f);
                player.transform.FindChild("Mesh").localScale = new Vector3(0.01f, 0.01f, 0.01f);

                //end point
                endPoint.transform.localPosition = new Vector3(0f, 0f, 0f);
                endPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                endPoint.transform.rotation = new Quaternion(1f, 0f, 0f, 0f);

                endPoint.GetComponent<MeshFilter>().mesh = meshes[3];
                endPoint.GetComponent<MeshRenderer>().material = materials[3];
                break;
        }
    }
    public void StartGame() 
    {
        toppingCollected.Clear();
        condimentCollected.Clear();

        for (int i = 0; i < LevelController.instance.selectedToppings.Count; i++)
        {
            toppingCollected.Add(false);
        }
        for (int i = 0; i < LevelController.instance.selectedCondiments.Count; i++)
        {
            condimentCollected.Add(false);
        }



        playerStatus = PlayerStatus.INTRO;
        rb.isKinematic = false;
        UpdateListCount();
        StartCoroutine(DelayBreadLoading());
    }
    void UpdateListCount()
    {
        if (playerStatus != PlayerStatus.GOINGDOWN)
        {
            listDisplay.transform.FindChild("1").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityToppings[0].ToString();
            listDisplay.transform.FindChild("2").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityToppings[1].ToString();
            listDisplay.transform.FindChild("3").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityToppings[2].ToString();
        }
        else
        {
            listDisplay.transform.FindChild("1").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityCondiments[0].ToString();
            listDisplay.transform.FindChild("2").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityCondiments[1].ToString();
            listDisplay.transform.FindChild("3").transform.FindChild("Text").GetComponent<Text>().text = LevelController.instance.quantityCondiments[2].ToString();
        }
    }
    IEnumerator DelayBreadLoading()
    {
        

        yield return new WaitForSeconds(2f);
        playerStatus = PlayerStatus.CHARGING;
        hasStarted = true;
        Physics.gravity = new Vector3(0f, -0.3f, 0f);
        powerBar.gameObject.SetActive(true);                    //Moving this to 'button down' on start
        listDisplay.gameObject.SetActive(true);
        launchBtn.gameObject.SetActive(true);
    }
    public void Jump()
    {
        if (playerStatus != PlayerStatus.CHARGING)
        {
            return;
        }
            launchBtn.gameObject.SetActive(false);
            playerStatus = PlayerStatus.GOINGUP;
            powerBar.GetComponent<PowerBar>().hasLaunched = true;
            StartCoroutine(DisplayPower());
            //speedTracker.gameObject.SetActive(true);
            //heightTracker.gameObject.SetActive(true);
            scoreTracker.gameObject.SetActive(true);
            
            float forceAdded = jumpForce * Mathf.Pow((0.5f), (1f - powerBar.value));
            if (powerBar.value > 0.99f)
            {
                GameCenterLoading.instance.AddProgressToPerfectLaunch();
            }
            rb.AddForce(new Vector3(0f, forceAdded, 0f));
            stepEndRotation = upRotation;
            StartCoroutine(Wait(1f));
        
    }
    void Flip()
    {

        float step = (Time.time - stepStartTime) / flipDuration;

        player.transform.rotation = Quaternion.Lerp(stepStartRotation, stepEndRotation, step);

        if (step >= 1f)
        {
            mDel -= Flip;
        }
    }
    IEnumerator DisplayPower()
    {
        yield return new WaitForSeconds(2f);
        powerBar.gameObject.SetActive(false);
    }
    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
        isMidflight = true;
        stepStartTime = Time.time;
        stepStartRotation = player.transform.rotation;
        //Removes the toaster and adds the plate with bread
        SetTable();
        mDel += Flip;
    }
    IEnumerator EndGameDelay()
    {
        Debug.Log("End game delay");
        yield return new WaitForSeconds(2f);
        endMenu.gameObject.SetActive(true);
        if (playerStatus == PlayerStatus.DEAD)
        {
            scoreResults.SetActive(false);//Your score displayed at the top.
            highScoreResults.transform.FindChild("Text").GetComponent<Text>().text = "Game Over!";
        }
        else
        {
            scoreResults.SetActive(true);//Your score displayed at the top.
            landingResult.gameObject.SetActive(false);//Good landing/Great/etc result text.
        }       
        
        GameUI.gameObject.SetActive(false);
    }
    void SetTable()
    {
        toaster.SetActive(false);
        plateAndBread.SetActive(true);
    }
    /*List movement disabled
    void MoveList()
    {
        float step = (Time.time - stepStartTime) / flipDuration;
        listDisplay.transform.position = new Vector3(listDisplay.transform.position.x, (Screen.height * 0.8f) * step, 0f);
    }*/
    public void ExplosiveDeath() 
    {
        int childCount =  player.transform.FindChild("GatherLocation").childCount;
        List<Transform> childs = new List<Transform>();
        for (int i = 0; i < childCount; i++)
        {
            player.transform.FindChild("GatherLocation").GetChild(i).gameObject.AddComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f)));
            childs.Add(player.transform.FindChild("GatherLocation").GetChild(i));
        }
        foreach (Transform child in childs)
        {
            child.GetComponent<Foods>().Death();
            child.parent = null;
        }
        playerStatus = PlayerStatus.DEAD;
    }
    public void FlipBool(bool flipped)
    {
        isFlipping = flipped;
    }
    public void ChangeToOverhead() 
    {
        LevelController.instance.isGoingDown = false;
        FlipBool(true);
        playerStatus = PlayerStatus.OVERHEAD;
        foreach (bool b in condimentCollected)
        {
            if (!b)
            {
                ExplosiveDeath();
                return;
            }
        }
        player.transform.position = new Vector3(Random.Range(-1f,1f), player.transform.position.y, Random.Range(-8.5f,-6.5f));
        Debug.LogError(player.transform.position);
        lastVelocity = rb.velocity.y;
        rb.velocity = new Vector3(0f, 0f, 0f);
        Physics.gravity = new Vector3(0f, -0.05f, 0f);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    void TiltControls() 
    {
        Vector3 dir = Vector3.zero;

        dir.x = Input.acceleration.x;
        if (playerStatus == PlayerStatus.OVERHEAD)
        {
            dir.x = -Input.acceleration.x;
            dir.z = -Input.acceleration.y;
        }

        if (dir.sqrMagnitude > 1)
        {
            dir.Normalize();
        }
           
        //if the acceleration is small enought we disregard.
        if (Mathf.Abs(dir.x) < m_TiltDeadZone)
        {
            dir.x = 0f;
            //Reduce motion by 5% per frame if no new input.
            rb.velocity = new Vector3(rb.velocity.x *0.95f, rb.velocity.y, rb.velocity.z);
        }
        if (Mathf.Abs(dir.z) < m_TiltDeadZone)
        {
            dir.z = 0f;
            //Reduce motion by 5% per frame if no new input.
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.95f);
        }

        dir *= Time.deltaTime;
        if (Mathf.Sign(dir.x) != Mathf.Sign(rb.velocity.x))
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.85f, rb.velocity.y, rb.velocity.z);
        }
        rb.AddForce(dir * tiltMovespeed);
#if ANDROID_DEBUG
        DEBUG_FIELD_DEVICE.text = "Rigidbody: " + rb.velocity + "\n" + "Input acceleration: " + Input.acceleration + "\n" + "Added Force: " + (dir/Time.deltaTime);
#endif
        //rb.velocity = new Vector3((dir.x * tiltMovespeed), rb.velocity.y, (dir.z * tiltMovespeed));
    }
    float touchPreviousPos;
    float touchPreviousPosY;
    float touchCurrentPosY;
    float touchCurrentPos;
    public void SwipeControls()
    {
        float debugSwipeValue = 0f;//todo remove
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                touchPreviousPos = t.position.x;
                touchPreviousPosY = t.position.y;
            }
            if (t.phase == TouchPhase.Ended)
            {
                touchPreviousPos = 0f;
                touchPreviousPosY = 0f;

            }

            if (t.phase == TouchPhase.Moved)
            {

                Vector3 dir = Vector3.zero;

                touchCurrentPos = t.position.x;
                touchCurrentPosY = t.position.y;


                if (playerStatus == PlayerStatus.OVERHEAD)
                {
                    dir.x = touchPreviousPos - touchCurrentPos;
                    dir.z = touchPreviousPosY - touchCurrentPosY;
                }
                else
                {
                    dir.x = touchCurrentPos - touchPreviousPos;
                }
                debugSwipeValue = dir.x;
                if (Mathf.Abs(dir.x) < m_SwipeDeadZone)
                {
                    dir.x = 0f;
                    //Reduce motion by 5% per frame if no new input.
                    rb.velocity = new Vector3(rb.velocity.x * 0.95f, rb.velocity.y, rb.velocity.z);
                }
                if (Mathf.Abs(dir.z) < m_SwipeDeadZone)
                {
                    dir.z = 0f;
                    //Reduce motion by 5% per frame if no new input.
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.95f);
                }

                //if (dir.sqrMagnitude > 1)
                //    dir.Normalize();
                //If changing direction, reduce speed further.
                if (Mathf.Sign(dir.x) != Mathf.Sign(rb.velocity.x))
                {
                    rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y, rb.velocity.z);
                }
                //If changing direction, reduce speed further.
                if (Mathf.Sign(dir.z) != Mathf.Sign(rb.velocity.z))
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.5f);
                }

                dir *= Time.deltaTime;
                rb.AddForce(dir * swipeMovespeed);
                //rb.velocity = new Vector3((dir.x * swipeMovespeed), rb.velocity.y, 0f);

                touchPreviousPos = touchCurrentPos;
                touchPreviousPosY = touchCurrentPos;



            }
#if ANDROID_DEBUG
            DEBUG_FIELD_DEVICE.text = "Rigidbody: " + rb.velocity + System.Environment.NewLine
            + "Swipe values: " + debugSwipeValue;
#endif

        }
        else //if no touch is occuring, lose 5% each frame.
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.95f, rb.velocity.y, rb.velocity.z * 0.95f);
        }

    }
    public void SetControls(string scheme)
    {
        if (scheme == "swipe")
        {
            enableSwipeControls = true;
            enableTiltControls = false;
        }
        else if (scheme == "tilt")
        {
            enableSwipeControls = false;
            enableTiltControls = true;
        }
    }
    private float upwardVelocity = 0f;
    private float pepperBonusStartTime = 0f;
    private bool pepperActive = false;
    public void PepperBonus() 
    {
        
        if (!pepperActive)
        {
            pepperActive = true;
            fireLocation.SetActive(true);
            upwardVelocity = rb.velocity.y;
            mDel += PepperActive;
        }
        pepperBonusStartTime = Time.time;
        
        pepperCollected++;
        if (pepperCollected == 10)
        {
            //GameCenterLoading.instance.UnlockAchievement("CgkI09G1lLUQEAIQHQ");
        }
        
        
    }
    void PepperActive()
    {
        
        Vector3 currentV = rb.velocity;
        
        if ( Time.time >= pepperBonusStartTime + pepperBoostDuration)
        {
            fireLocation.SetActive(false);
            mDel -= PepperActive;
            if (upwardVelocity < endBoostMinSpeed)
            {
                upwardVelocity = endBoostMinSpeed;
            }
            rb.velocity = new Vector3(currentV.x, upwardVelocity, 0f);
            pepperActive = false;
            return;
        }
        
        rb.velocity = new Vector3(currentV.x, maxUp, 0f);

    }
    private int bonusMultiplier = 1;
    public void AddScore(int amount, bool multiplier, Toppings top, Condiments con) 
    {
        if (multiplier)
        {
            if (top != Toppings.COUNT)
            {
                for (int i = 0; i < LevelController.instance.selectedToppings.Count; i++)
                {
                    if (LevelController.instance.selectedToppings[i] == top)
                    {

                        LevelController.instance.quantityToppings[i]--;
                        if(LevelController.instance.quantityToppings[i] <= 0)
                        {
                            toppingCollected[i] = true;
                        }
                        
                        break;
                    }
                }
            }
            if (con != Condiments.COUNT)
            {
                for (int j = 0; j < LevelController.instance.selectedCondiments.Count; j++)
                {
                    if (LevelController.instance.selectedCondiments[j] == con)
                    {
                        LevelController.instance.quantityCondiments[j]--;
                        if (LevelController.instance.quantityCondiments[j] <= 0)
                        {
                            condimentCollected[j] = true;
                        }
                        
                        break;
                    }
                }
            }

         
        }
    


       
       score++;
       scoreTracker.text = score.ToString();
       CheckScoreAchievement();
       UpdateListCount();
    }  
    public void EndGame(float distance) //also end point
    {
        int ingrediantsCollected = player.transform.FindChild("GatherLocation").childCount;

        Vector3 gatherPOS = player.transform.FindChild("GatherLocation").position;




        for (int i = 0; i < (int)(ingrediantsCollected/2); i++)
        {
            splatList[i].transform.position = new Vector3(gatherPOS.x, gatherPOS.y-(i/ingrediantsCollected), gatherPOS.z);
            splatList[i].SetActive(true);
           
        }

        if (distance > m_LandingMissed)
        {
            landingResult.GetComponent<Text>().text = "Missed!";
            //score -= 10;
        }
        else
        {
            GameCenterLoading.instance.PostToLeaderboard(score);
            
            int accuracy = (100 - (int)(distance * 100f));

            if (distance < m_LandingPerfect)
            {
                //score += 50;
                GameCenterLoading.instance.AddProgressToPerfectLanding();
                landingResult.GetComponent<Text>().text = "Perfect Landing!";
            }
            else if (distance < m_LandingGreat)
            {
                landingResult.GetComponent<Text>().text = "Great Landing!";
                //score += 25;
            }
            else
            {
                landingResult.GetComponent<Text>().text = "Good Landing!";
                //score += 10;
            }

           
            scoreTracker.text = score.ToString();
            //CheckScoreAchievement();
            GameCenterLoading.instance.AddProgressToCompletedSand();
            //GameCenterLoading.instance.AddCoins(score);
            
        }
        int highScore = GameManager.instance.LoadHighScore();
        if (highScore >= score)
        {
            highScoreResults.transform.FindChild("Text").GetComponent<Text>().text = "high score " + highScore;
            scoreResults.transform.FindChild("Text").GetComponent<Text>().text = "Score " + score;
        }
        else
        {
            highScoreResults.transform.FindChild("Text").GetComponent<Text>().text = "New personal best " + score;
            scoreResults.transform.FindChild("Text").GetComponent<Text>().text = "Score " + score;
            GameManager.instance.SaveScore(score);
        }
        Debug.Log("End game");
        landingResult.gameObject.SetActive(true);
    }
    int CalculateEarnedCoins(int score)
    {
        return (score / ptsRequiredPerCoin);
    }
    public void CheckScoreAchievement()
    {
     
    }
    public void CheckToppingAchievement(int count)
    {
        if (count >= 175)
        {
            GameCenterLoading.instance.UnlockAchievement("CgkIm8DKqdILEAIQEA");
            return;
        }
        if (count >= 150)
        {
            GameCenterLoading.instance.UnlockAchievement("CgkIm8DKqdILEAIQDw");
            return;
        }
        if (count >= 125)
        {
            GameCenterLoading.instance.UnlockAchievement("CgkIm8DKqdILEAIQEQ");
            return;
        }
        if (count >= 100)
        {
            GameCenterLoading.instance.UnlockAchievement("CgkIm8DKqdILEAIQDg");
            return;
        }
        if (count >= 75)
        {
            GameCenterLoading.instance.UnlockAchievement("CgkIm8DKqdILEAIQDQ");
        }        
    }
    public void ResetValues()
    {
        this.StopAllCoroutines();
        condimentCount = 0;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        Physics.gravity = new Vector3(0f,-0.3f, 0f);
        score = 0;
        isMidflight = false;
        isFlipping = false;
        scoreTracker.text = score.ToString();
        bonusMultiplier = 1;
        playerStatus = PlayerStatus.INTRO;
        powerBar.GetComponent<PowerBar>().hasLaunched = false;
        plateAndBread.SetActive(false);
        toaster.SetActive(true);
        isEndGame = false;
        hasStarted = false;
        GameUI.SetActive(true);
        endMenu.SetActive(false);
        player.gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, 0.003f, 0f);
        player.gameObject.GetComponent<BoxCollider>().size = new Vector3(0.06457424f, 0.006f, 0.05993531f);
        //player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;
        player.GetComponent<Rigidbody>().velocity = new Vector3(0f,0f,0f);
        StartGame();
    }
}


