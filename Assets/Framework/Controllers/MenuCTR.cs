using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuCTR : MonoBehaviour {

    public Sprite soundOn;
    public Sprite soundOff;

    public Button soundBtn;

    public GameObject mainMenuUI;
    public GameObject gameUI;
    public GameObject titleUI;
    public GameObject charSelectUI;
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public Text txt_Char_Name;

    bool isSoundOn = true;

    AudioSource aud;

    bool movingUI = false;
    bool isToasty = true;

    public GameObject player;
    public GameObject plate;

    public Sprite toast;
    public Sprite bagel;

    public GameObject btn_Toast;

    public GameObject mainBg;
    public GameObject mainScene;

    public Transform playerCenter;
    public string[] selectedChar;
    public GameObject[] selectedChar_prefab;
    private int currentCharacterIndex = 0;

    public Camera charSelectCam;
    private GameObject currentCharacter;

    public GameObject mutedOverlay;
    public GameObject tiltControlSelected;
    public GameObject swipeControlSelected;
    public GameObject titleLogo;
    public GameObject settingsMenu;


    void Awake() 
    {
        aud = GetComponent<AudioSource>();
    }
	// Use this for initialization
	void Start () {
        currentCharacter = Instantiate(selectedChar_prefab[currentCharacterIndex], playerCenter.position, Quaternion.identity) as GameObject;
        charSelectCam.depth = 0;
    }
    public void ToggleCharSelect(bool t)
    {
        titleLogo.SetActive(!t);
        mainMenuUI.SetActive(!t);
        charSelectUI.SetActive(t);
      
    }
   

    //scroll up or down the list of characters.
    public void Scroll(int s)
    {
        PlayButton();
        if (currentCharacter)
        {
            Destroy(currentCharacter);
        }
        if (s == 1)
        {
            currentCharacterIndex++;
            if (currentCharacterIndex == selectedChar.Length)
            {
                currentCharacterIndex = 0;
            }
        }
        else if (s == -1)
        {
            currentCharacterIndex--;
            if (currentCharacterIndex < 0)
            {
                currentCharacterIndex = selectedChar.Length - 1;
            }
        }
        currentCharacter = Instantiate(selectedChar_prefab[currentCharacterIndex], playerCenter.position, Quaternion.identity) as GameObject;
        CharSelect(selectedChar[currentCharacterIndex]);
        txt_Char_Name.text = selectedChar[currentCharacterIndex];
    }

    public void CharSelect(string sel) 
    {
        PlayButton();
        player.GetComponent<Player>().ChangeModel(sel);
    }

    public void ToogleSetting()
    {
        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
        }
        else
        {
            settingsMenu.SetActive(true);
        }
    }

    public void StartGame() 
    {
        PlayButton();
        charSelectCam.depth = -2;
        mainBg.SetActive(false);
        mainMenuUI.SetActive(false);
        gameUI.SetActive(true);

        LevelController.instance.isPlaying = true;

        player.GetComponent<Player>().StartGame();
    }

    public void ShowAchievements()
    {
        PlayButton();
        //GameCenterLoading.instance.ShowAchievements();
    }
    public void ShowLeaderboard()
    {
        PlayButton();
        //GameCenterLoading.instance.ShowLeaderboard();
    }

    public void TogglePauseMenu()
    {        
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        pauseButton.SetActive(!pauseButton.activeSelf);
        Time.timeScale = pauseMenu.activeSelf ? 0f : 1f;
        float pausedVolume = pauseMenu.activeSelf ? 0.5f : 1f;
        SoundController.instance.SetVolume(pausedVolume);
    }

    public void BackToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    #region Settings
    //toggle sound on/off
    public void SoundToggle() 
    {
        isSoundOn = !isSoundOn;
        if (isSoundOn)
        {
            SoundController.instance.UnMute();
            mutedOverlay.SetActive(false);
        }
        else
        {
            SoundController.instance.Mute();
            mutedOverlay.SetActive(true);
        }
        
    }
    public void SelectControls(string selected)
    {
        PlayBlock();
        if (selected == "tilt")
        {
            tiltControlSelected.SetActive(true);
            swipeControlSelected.SetActive(false);
            Player.instance.SetControls(selected);
        }
        if (selected == "swipe")
        {
            tiltControlSelected.SetActive(false);
            swipeControlSelected.SetActive(true);
            Player.instance.SetControls(selected);
        }
    }
    public void FacebookLink()
    {
        PlayBlock();
        Application.OpenURL("https://www.facebook.com/brian.gogarty.52");
    }
    public void TwitterLink()
    {
        PlayBlock();
        Application.OpenURL("https://twitter.com/sandwichheroapp");
    }

    #endregion
    public void PlayButton()
    {
        SoundController.instance.PlayClip(Random.Range(2, 5));
    }

    public void PlayBlock()
    {
        SoundController.instance.PlayClip(Random.Range(0, 2));
    }
}
