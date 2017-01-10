using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//Represents the status of the application.
public enum AppStatus 
{
    LOADING,    //Application status during loading.
    MENU,       //Application status while navigating menues.
    SHOP,       //Application status while browsing the shop.
    PLAYING,    //Application status during gameplay.
    EXIT        //Application status when closing.
}
///Game Manager is loaded first and closed last. 
///Holds the user's data and unlocked bonuses.
public class GameManager : MonoBehaviour {

    public static GameManager instance = null;              //Static instance of GameManager.

    //Current Status of our application. Public getter and private setter. Set is done inside the class.
    public AppStatus _appStatus { get; private set; }
    //Current loaded LevelController. Public getter and private setter. Set is done inside the class.
    public LevelController _levelController { get; private set; }
    
    
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    public void Load(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void SaveScore(int score)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Application.persistentDataPath + "/highScore.dat");

        PlayerScore pScore = new PlayerScore();
        pScore.highScore = score;

        bf.Serialize(fs, pScore);
    }

    public int LoadHighScore()
    {
        if (File.Exists(Application.persistentDataPath + "/highScore.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(Application.persistentDataPath + "/highScore.dat", FileMode.Open);

            PlayerScore pScore = (PlayerScore)bf.Deserialize(fs);
            fs.Close();
            return pScore.highScore;
        }
        else
        {
            return 0;
        }
       
    }

   
}
[System.Serializable]
class PlayerScore
{
    public int highScore;
}