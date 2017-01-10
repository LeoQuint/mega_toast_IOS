using UnityEngine;
using System.Collections;
using NS_Level;
public class Foods : MonoBehaviour {

    public bool pepper;
    public Toppings typeTopping;
    public Condiments typeCondiment;

    public int pointValue = 1;

    public int[] clipPositions;

    public GameObject condimentSplash;
    
    void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag == "Player")
        {
            Transform parentT = other.gameObject.transform.FindChild("GatherLocation");
            Destroy(GetComponent<RotationScript>());
            Destroy(GetComponent<BoxCollider>());


            if (typeCondiment == Condiments.COUNT)
            {
                gameObject.transform.SetParent(parentT);
                gameObject.transform.rotation = transform.parent.rotation;
                int childCount = parentT.childCount;
                float offset = ((float)(childCount)) / 600f;

                gameObject.transform.localPosition = new Vector3(0f, offset, 0f);

                if (typeTopping == Toppings.Egg)
                {
                    gameObject.transform.localRotation = Quaternion.Euler(180f, Random.Range(0f, 355f), 0f);

                }
                else
                {
                    gameObject.transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 355f));
                }

                Vector3 center = other.gameObject.GetComponent<BoxCollider>().center;
                Vector3 colliderSize = other.gameObject.GetComponent<BoxCollider>().size;
                other.gameObject.GetComponent<BoxCollider>().center = new Vector3(center.x, center.y + 0.001f, center.z);
                other.gameObject.GetComponent<BoxCollider>().size = new Vector3(colliderSize.x, colliderSize.y + 0.001f, colliderSize.z);

                Player.instance.CheckToppingAchievement(childCount);
                
                SoundController.instance.PlayClip(clipPositions[Random.Range(0,clipPositions.Length)]);
            }
            else if(!pepper)
            {
                if (condimentSplash != null)
                {
                    
                    GameObject cs = Instantiate(condimentSplash, parentT.position, Quaternion.identity) as GameObject;

                    cs.transform.SetParent(parentT);
                    //cs.transform.rotation = transform.parent.rotation;
                    
                    float offset = ((float)(Player.instance.condimentCount + 3)) / 600f;

                    Player.instance.condimentCount++;
                    cs.transform.localPosition = new Vector3(0f, offset, 0f);

                    cs.transform.localRotation = Quaternion.Euler((Vector3.up * Random.Range(0f, 355f)) + new Vector3(90f, 0f, 0f));
                    //Vector3 center = other.gameObject.GetComponent<BoxCollider>().center;
                    //Vector3 colliderSize = other.gameObject.GetComponent<BoxCollider>().size;
                    //other.gameObject.GetComponent<BoxCollider>().center = new Vector3(center.x, center.y + 0.001f, center.z);
                    //other.gameObject.GetComponent<BoxCollider>().size = new Vector3(colliderSize.x, colliderSize.y + 0.001f, colliderSize.z);
                    int childCount = parentT.childCount;
                    Player.instance.CheckToppingAchievement(childCount);
                }
                Destroy(gameObject);
            }
        
            if (pepper &&  Player.instance.playerStatus == PlayerStatus.GOINGUP)
            {

                Player.instance.PepperBonus();
            }
            SoundController.instance.PlayClip(clipPositions[Random.Range(0, clipPositions.Length)]);
            bool correctTopping = false;
        
            for (int i = 0; i < LevelController.instance.selectedToppings.Count; i++)
            {
                if (LevelController.instance.selectedToppings[i] == typeTopping)
                {
                    correctTopping = true;
                    if (LevelController.instance.quantityToppings[i] == 1)
                    {
                        LevelController.instance.SetCheckMarks(i + 1);
                    }
                }
            }
            for (int j = 0; j < LevelController.instance.selectedCondiments.Count; j++)
            {
                if (LevelController.instance.selectedCondiments[j] == typeCondiment)
                {
                    correctTopping = true;
                    if (LevelController.instance.quantityCondiments[j] == 1)
                    {
                        LevelController.instance.SetCheckMarks(j + 1);
                    }
                    
                }
            }
            if (correctTopping)
            {
               Player.instance.AddScore(pointValue, true, typeTopping, typeCondiment);
            }
            else 
            {
                Player.instance.AddScore(pointValue, false, typeTopping, typeCondiment);
            }
        }


    }


    public void Death()
    {
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

}
