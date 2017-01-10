using UnityEngine;
using System.Collections;

public class SocialController : MonoBehaviour {

    public static SocialController instance; 

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(tempLoading());
    }

    IEnumerator tempLoading()
    {
        yield return new WaitForSeconds(2f);

        GameManager.instance.Load(1);
    }

}
