using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterScript : MonoBehaviour {

	public GameObject projectile;
	public float timeToReload;
	public float fireSpeed;

	float reloadTimer = 0;

	List<GameObject> playerList;

	void Awake() {
		playerList = new List<GameObject>();
	}


	void Update() {
		// check if shooter is reloaded
		if (reloadTimer > timeToReload) {
			// check if there are any targets within range
			if (playerList.Count > 0) {
				reloadTimer = 0;
				//shoot at each player in range
				foreach (GameObject obj in playerList) {
					//calculate the trajectory vector 
					float x = obj.transform.position.x - transform.position.x;
					float y = obj.transform.position.y - transform.position.y;
					Vector2 FireDirection = new Vector2 (x, y);
					FireDirection = FireDirection.normalized * fireSpeed;

					// Create the projectile and Access its Rigidbody to add force
					GameObject newProjectile = (GameObject) Instantiate (projectile, transform.position, transform.rotation);
					newProjectile.GetComponent<Rigidbody2D> ().velocity = FireDirection;
					//If the projectile exists after 5 seconds, destroy it
					Destroy (newProjectile, 5);
				}
			}
		} else {
			reloadTimer += Time.deltaTime;
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
        //TASK 3
        //HINT: Use coll.gameObject to get a reference to coll's GameObject
        if (coll.gameObject.CompareTag("Player"))
        {
            playerList.Add(coll.gameObject);
        }
	}

	void OnTriggerExit2D(Collider2D coll) {
        //TASK 3
        if (coll.gameObject.CompareTag("Player"))
        {
            playerList.Remove(coll.gameObject);
        }
    }

	//Returns whether or not the Game Object is the Player Character
	bool isPlayer(GameObject obj) {
		return obj.CompareTag ("Player");
	}
}
