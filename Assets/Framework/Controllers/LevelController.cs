using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using NS_Level;

//Builds the level at start
public class LevelController : MonoBehaviour {

    ///Static//////////////////////////////////////////////////////////////////////
    ///                                                                         ///
    ///                                                                         ///
    ///////////////////////////////////////////////////////////////////////////////   

    //Only one instance of levelController for each levels.
    public static LevelController instance;
    public static GameObject holder_Toppings;     //Gameobject to hold the spawned toppings (keeping the scene manager clean)
    public static GameObject holder_Condiments;  //Gameobject to hold the spawned condiments (keeping the scene manager clean)

    ///Public//////////////////////////////////////////////////////////////////////
    ///                                                                         ///
    ///                                                                         ///
    ///////////////////////////////////////////////////////////////////////////////  
                                                                      
    //Selected Toppings and condiments for the sandwich built in this level.
    public List<Toppings> selectedToppings = new List<Toppings>();
    public List<int> quantityToppings = new List<int>();
    public List<Condiments> selectedCondiments = new List<Condiments>();
    public List<int> quantityCondiments = new List<int>();

    //Rate of spawn for objects and bonuses.
    public float objectSpawnRate = 0.5f;
    public float bonusesSpawnRate = 0.1f;

    public bool isPlaying = false;

    public Transform startPOS;

    public GameObject[] toppings;
    public GameObject[] condiments;
    public GameObject[] bonuses;

    public Image li_1;
    public Image li_2;
    public Image li_3;
    public Sprite[] cSprites;
    public Sprite[] tSprites;

    public Transform targetPlayer;
    public Transform startingPosition;

    public Camera_Follow camScript;
    ///Private/////////////////////////////////////////////////////////////////////
    ///                                                                         ///
    ///                                                                         ///
    ///////////////////////////////////////////////////////////////////////////////   
    
    public bool isGoingDown = false;

    //Lists holding references to each spawned gameobject going up / down.
    List<GameObject> upSpawned = new List<GameObject>();
    List<GameObject> downSpawned = new List<GameObject>();

    float spawnedHeight;
    int downSpawnedHeight;

    void Awake () 
    {
        
        //Setup the singleton
        if (instance != null)
            Destroy(instance);
        instance = this;

        spawnedHeight = 0f;
        isGoingDown = false;

        //Setup the spawned toppings/condiments holders 
        holder_Condiments = new GameObject("SpawnedCondiments");
        holder_Toppings = new GameObject("SpawnedToppings");
        holder_Condiments.transform.SetParent(this.transform);
        holder_Toppings.transform.SetParent(this.transform);


#if UNITY_ANDROID
        if (!GameCenterLoading.instance.isConnected)
        {
            GameCenterLoading.instance.RetryConnection();
        }
#endif
    }


    void Start() 
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Gather all resources needed in this level.
        BuildLevel();
    }


    void Update() 
    {
        SpawnAdditionals();
    }

    void BuildLevel() 
    {
        CreateSandwich(true);
        SpawnToppings(50);
    }

    //Determines if more condiments / toppings need to be spawned
    void SpawnAdditionals()
    {
        //If the player has come close to the top height of spawned toppings, spawn more
        if (targetPlayer.position.y > spawnedHeight - 10f)
            SpawnToppings(20f, false);
        
        //If the player has come close to the bottom height of the spawned condiments, spawn more
        if (isGoingDown && targetPlayer.position.y < downSpawnedHeight + 5f)
            SpawnCondiments(20);
    }



    //Creates the sandwich the player is trying to build. If isRandom = true we create a random sandwich.
    //Size = the number of condiments and toppings to be randomed.
    public void ClearForLanding()
    {
        foreach (GameObject g in downSpawned)
        {
            Destroy(g);
        }
    }

    //Creates random requirements for the sandwich for this level
    void CreateSandwich(bool isRandom = false, int size = 3) 
    {
        //Clear old values 
        selectedToppings.Clear();
        quantityToppings.Clear();
        selectedCondiments.Clear();
        quantityCondiments.Clear();

        //Random Sandwich: Build a random list of wanted toppings & condiments 
        if (isRandom)
        {
            for (int index = 0; index < 3; index++)
            {
                bool duplicated = false;
                int rngTop = 0;
                int rngCond = 0;
               
                //Find & add a unique Topping:
                do
                {
                    duplicated = false;
                    rngTop = Random.Range(0, (int)Toppings.COUNT);
                    for (int i = 0; i < selectedToppings.Count; i++)
                    {
                        if (selectedToppings[i] == (Toppings)rngTop)
                        {
                            duplicated = true;
                            break;
                        }
                    }
                } while (duplicated);

                selectedToppings.Add((Toppings)rngTop);

                //Find & add a unique condiment
                do
                {
                    duplicated = false;
                    rngCond = Random.Range(0, (int)Condiments.COUNT);
                   
                    for (int j = 0; j < selectedCondiments.Count; j++)
                    {
                        if (selectedCondiments[j] == (Condiments)rngCond)
                        {
                            duplicated = true;
                            break;
                        }
                    }
                } while (duplicated);

                selectedCondiments.Add((Condiments)rngCond);
            }
        }

        //Pick random quantity requirements
        for (int j = 0 ; j < 3; j++)
        {
            quantityToppings.Add(Random.Range(1,4));
            quantityCondiments.Add(Random.Range(1, 4));
        }

        //Set the sprites & text for the required toppings & condiments in the UI
        li_1.sprite = tSprites[(int)selectedToppings[0]];
        li_2.sprite = tSprites[(int)selectedToppings[1]];
        li_3.sprite = tSprites[(int)selectedToppings[2]];

        li_1.transform.FindChild("Text").GetComponent<Text>().text = quantityToppings[0].ToString();
        li_2.transform.FindChild("Text").GetComponent<Text>().text = quantityToppings[1].ToString();
        li_3.transform.FindChild("Text").GetComponent<Text>().text = quantityToppings[2].ToString();

    }

    //Spawn toppings up the vertical stretch
    void SpawnToppings(float amount, bool initial = true) 
    {
        //Destroy all the way-down-condiments
        foreach (GameObject g in downSpawned)
            Destroy(g);    
        

        //SPECIAL CASE: Initial height
        if (initial)
        {
            //Remove all the old upspawned
            foreach (GameObject g in upSpawned)
                Destroy(g);
            upSpawned.Clear();

            //Set initial spawning height
            spawnedHeight = startPOS.position.y + 5f;
        }


        //Spawn desired amount of toppings up the stretch, spaced out
        float yPOS; //Y Position of the spawn, increments with the index of the spawn
        for (int i = 0; i < amount; i++)
        {
            float rand = Random.Range(0f, 1f);

            //Determine position of new spawned food
            float xPOS = Random.Range(-1.2f, 1.2f) ; 
            yPOS = i + spawnedHeight;

           //Spawn a new topping
            GameObject newlySpawned = null;
            if (rand < bonusesSpawnRate)        //Check if we're spawning bonus
                newlySpawned = Instantiate(bonuses[0], new Vector3(xPOS, yPOS, -7.497f), Quaternion.identity) as GameObject;
            else if (rand < objectSpawnRate)    //Check if we're spawning regular
                newlySpawned = Instantiate(toppings[Random.Range(0, (int)Toppings.COUNT)], new Vector3(xPOS, yPOS, -7.497f), Quaternion.identity) as GameObject;

            //If a new topping was properly spawned, add it to our toppings list/holder
            if (newlySpawned != null)
            {
                upSpawned.Add(newlySpawned);
                newlySpawned.transform.SetParent(holder_Toppings.transform);
            }
        }

        spawnedHeight += amount;
    }


    //Used to spawn confimdents on teh way down
    public void SpawnCondimentsTurn() 
    {
        //Display required condiments in UI 
        li_1.sprite = cSprites[(int)selectedCondiments[0]];
        li_2.sprite = cSprites[(int)selectedCondiments[1]];
        li_3.sprite = cSprites[(int)selectedCondiments[2]];

        li_1.transform.FindChild("Text").GetComponent<Text>().text = quantityCondiments[0].ToString();
        li_2.transform.FindChild("Text").GetComponent<Text>().text = quantityCondiments[1].ToString();
        li_3.transform.FindChild("Text").GetComponent<Text>().text = quantityCondiments[2].ToString();


        SetCheckMarks(999);

        //Destroy all the toppings (way up spawns)
        foreach (Transform leftoverToppings in holder_Toppings.transform)
            Destroy(leftoverToppings.gameObject);

        //Spawn 15 condiments 
        int height = ((int)targetPlayer.position.y) - 3;
        float yPOS;
        for (int i = 0; i < 15; i++)
        {
            if (Random.Range(0f, 1f) < objectSpawnRate)
            {
                //Determine position 
                float xPOS = Random.Range(-1.2f, 1.2f);
                yPOS = height - i;

                //Spawn the new condiment
                GameObject newlySpawned = Instantiate(condiments[Random.Range(0, (int)Condiments.COUNT)], new Vector3(xPOS, yPOS, -7.497f), Quaternion.identity) as GameObject;
                downSpawned.Add(newlySpawned);
                newlySpawned.transform.SetParent(holder_Condiments.transform);
            }
        }

        //Set us up for going down, and drop the height
        isGoingDown = true;
        downSpawnedHeight = height - 15;
        Debug.Log("Height:" + downSpawnedHeight);
    }

    //Spawn more condiments
    void SpawnCondiments(int num)
    {
        Debug.Log("Height:"  + downSpawnedHeight);
        float xPOS;
        float yPOS;

        int height = downSpawnedHeight -1;

        for (int i = 0; i < num; i++)
        {
            if (Random.Range(0f, 1f) < objectSpawnRate)
            {
                //Determine new position
                xPOS = Random.Range(-1.2f, 1.2f);
                yPOS = height - i;

                //Spawn the new condiment
                GameObject newlySpawned = Instantiate(condiments[Random.Range(0, (int)Condiments.COUNT)], new Vector3(xPOS, yPOS, -7.497f), Quaternion.identity) as GameObject;
                downSpawned.Add(newlySpawned);
                newlySpawned.transform.SetParent(holder_Condiments.transform);
            }
        }

        downSpawnedHeight -= num;
    }


    public void SetCheckMarks(int c)
    {
        SoundController.instance.PlayClip(Random.Range(5,7));
        switch (c)
        {
            case 1:
                li_1.transform.FindChild("Image").gameObject.SetActive(true);
                li_1.transform.FindChild("Text").gameObject.SetActive(false);
                break;
            case 2:
                li_2.transform.FindChild("Image").gameObject.SetActive(true);
                li_2.transform.FindChild("Text").gameObject.SetActive(false);
                break;
            case 3:
                li_3.transform.FindChild("Image").gameObject.SetActive(true);
                li_3.transform.FindChild("Text").gameObject.SetActive(false);
                break;
            case 999:
                li_1.transform.FindChild("Image").gameObject.SetActive(false);
                li_2.transform.FindChild("Image").gameObject.SetActive(false);
                li_3.transform.FindChild("Image").gameObject.SetActive(false);
                li_1.transform.FindChild("Text").gameObject.SetActive(true);
                li_2.transform.FindChild("Text").gameObject.SetActive(true);
                li_3.transform.FindChild("Text").gameObject.SetActive(true);
                break;
        }
    }

    //Sets everything up for replay (NEVER CALLED YET)
    public void Replay()
    {
        //SceneManager.LoadScene(1);
        targetPlayer.position = startingPosition.position;
        targetPlayer.rotation = startingPosition.rotation;
        

        //Clear all collected toppings and condiments 
        foreach (Transform child in targetPlayer.transform.FindChild("GatherLocation"))
            GameObject.Destroy(child.gameObject);

        //Reset all values
        isGoingDown = false;
        downSpawnedHeight = 0;
        BuildLevel();
        SetCheckMarks(999);
        Player.instance.ResetValues();
        camScript.ResetValues();
    }


    public void LoadHome()
    {
        GameManager.instance.Load(1);
    }
}
