/**
 * 
 * Tracks player attributes and handles behaviours
 * 
 **/

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Faction to determine friendly fire and other mechanics
	public string faction = "Player";
	int id = 0;

	const float HEAL_WAIT = 5.0f;
	const float MAX_HEALTH = 100;
	const float REGEN_INC = 0.7f;
	const float FIRE_RATE = 0.2f;

	public Camera playerCam;
	public Crosshair crossScript;
	public GameObject ammunition;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;

	float health = MAX_HEALTH;
	float crossJitter = 0;
	float healTimer = 0;
	float fireTimer = 0;

	// Use this for initialization
	void Start () {
		Initialize("Player", 1);
	}
	
	// Update is called once per frame
	void Update () {
		tryRegen();
	}

	public void Initialize(string newFaction, int playerId){
		id = playerId;
		faction = newFaction;
		playerController.setController(playerId);
		playerRenderer.setWindow(0, 0.5f, 0.5f, 1);
	}

	// Regenerates if healing timer is depleted and health is below maximum
	void tryRegen(){
		if (health < MAX_HEALTH){
			if (healTimer <= 0){
				health = Mathf.Min(MAX_HEALTH, health + REGEN_INC * Time.deltaTime);
				print (health);
			}
			else{
				healTimer -= Time.deltaTime;
			}
		}
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage){
		print (damage);
		health -= damage;
		healTimer = HEAL_WAIT;
		print (health);
	}

	// Attempts to fire bullet
	public void tryFire(Vector3 fireDir, Vector3 bulletPos){
		if (fireTimer <= 0){
			GameObject bullet = Instantiate(ammunition, bulletPos, Quaternion.identity) as GameObject;
			Bullet bulletScript = bullet.GetComponent<Bullet>();
			bulletScript.setProperties(15, faction, fireDir, 40);
			
			fireTimer = FIRE_RATE;
		}
		else{
			fireTimer -= Time.deltaTime;
		}
	}
}
