using UnityEngine;
using System.Collections;

//Used to determine how close the toast fell to the center plate
public class EndGameToast : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Stop the toast 
            other.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY;

            //Move sandwich so it's height is offset by the number of toppings 
            float offset = ((float)(other.gameObject.transform.FindChild("GatherLocation").childCount - Player.instance.condimentCount)) / 60f;
            other.transform.position = new Vector3(other.transform.position.x, 0.7f + offset, other.transform.position.z);

            //Update player status
            Player.instance.playerStatus = PlayerStatus.LANDED;

            //Determine distance from plate center
            float distance = Vector3.Distance(new Vector3(this.transform.position.x, 0f, this.transform.position.z), new Vector3(other.transform.position.x, 0f, other.transform.position.z));
            distance = Mathf.Abs(distance);

            //Send final distance to the end game
            Player.instance.EndGame(distance);
        }
    }
}
