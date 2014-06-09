using UnityEngine;
using System.Collections;
using System;
using KiiCorp.Cloud.Analytics;
using System.Collections.Generic;
using KiiCorp.Cloud.Storage;

public class PlayerHealth : MonoBehaviour
{	
	public float health = 100f;					// The player's health.
	public float repeatDamagePeriod = 2f;		// How frequently the player can be damaged.
	public AudioClip[] ouchClips;				// Array of clips to play when the player is damaged.
	public float hurtForce = 10f;				// The force with which the player is pushed when hurt.
	public float damageAmount = 10f;			// The amount of damage to take when enemies touch the player

	private SpriteRenderer healthBar;			// Reference to the sprite renderer of the health bar.
	private float lastHitTime;					// The time at which the player was last hit.
	private Vector3 healthScale;				// The local scale of the health bar initially (with full health).
	private PlayerControl playerControl;		// Reference to the PlayerControl script.
	private Animator anim;						// Reference to the Animator on the player

	void Awake ()
	{
		// Setting up references.
		playerControl = GetComponent<PlayerControl>();
		healthBar = GameObject.Find("HealthBar").GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();

		// Getting the intial scale of the healthbar (whilst the player has full health).
		healthScale = healthBar.transform.localScale;
	}

	void Start ()
	{
		if(GameConfig.ENABLE_ANALYTICS)
			FetchAvgDeathTime ();
	}


	void OnCollisionEnter2D (Collision2D col)
	{
		// If the colliding gameobject is an Enemy...
		if(col.gameObject.tag == "Enemy" || col.gameObject.tag == "Enemy2")
		{
			// ... and if the time exceeds the time of the last hit plus the time between hits...
			if (Time.time > lastHitTime + repeatDamagePeriod) 
			{
				// ... and if the player still has health...
				if(health > 0f)
				{
					// ... take damage and reset the lastHitTime.
					TakeDamage(col.transform); 
					lastHitTime = Time.time; 
				}
				// If the player doesn't have health, do some stuff, let him fall into the river to reload the level.
				else
				{
					// Find all of the colliders on the gameobject and set them all to be triggers.
					Collider2D[] cols = GetComponents<Collider2D>();
					foreach(Collider2D c in cols)
					{
						c.isTrigger = true;
					}

					// Move all sprite parts of the player to the front
					SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
					foreach(SpriteRenderer s in spr)
					{
						s.sortingLayerName = "UI";
					}

					// ... disable user Player Control script
					GetComponent<PlayerControl>().enabled = false;

					// ... disable the Gun script to stop a dead guy shooting a nonexistant bazooka
					GetComponentInChildren<Gun>().enabled = false;

					// ... Trigger the 'Die' animation state
					anim.SetTrigger("Die");

					Score.LoadHighScore();
					if(GameConfig.ENABLE_ANALYTICS)
					{
						SendDeathEvent ();
						FetchAvgDeathTime();
				    }
				}
			}
		}
	}


	void TakeDamage (Transform enemy)
	{
		// Make sure the player can't jump.
		playerControl.jump = false;

		// Create a vector that's from the enemy to the player with an upwards boost.
		Vector3 hurtVector = transform.position - enemy.position + Vector3.up * 5f;

		// Add a force to the player in the direction of the vector and multiply by the hurtForce.
		rigidbody2D.AddForce(hurtVector * hurtForce);

		// Reduce the player's health by 10.
		health -= damageAmount;

		// Update what the health bar looks like.
		UpdateHealthBar();

		// Play a random clip of the player getting hurt.
		int i = UnityEngine.Random.Range (0, ouchClips.Length);
		AudioSource.PlayClipAtPoint(ouchClips[i], transform.position);
	}


	public void UpdateHealthBar ()
	{
		// Set the health bar's colour to proportion of the way between green and red based on the player's health.
		healthBar.material.color = Color.Lerp(Color.green, Color.red, 1 - health * 0.01f);

		// Set the scale of the health bar to be proportional to the player's health.
		healthBar.transform.localScale = new Vector3(healthScale.x * health * 0.01f, 1, 1);
	}

	void SendDeathEvent ()
	{
		try {
			// Sending Kii Analytics event for game over stats
			KiiEvent ev = KiiAnalytics.NewEvent("PlayerDeath");
			
			// Set key-value pairs
			ev ["time"] = Time.time;
			ev ["level"] = 1;

			// Upload Event Data to Kii Cloud
			KiiAnalytics.Upload((Exception e) => {
				if (e == null)
				{
					Debug.Log ("Analytics event upload successful");
				} else {
					Debug.Log ("Analytics event upload error: " + e.ToString());
				}
				
			}, ev);
			
		} catch (CloudException e) {
			Debug.Log ("Analytics event upload error: " + e.ToString());
		}
	}

	void FetchAvgDeathTime ()
	{
		if(GameConfig.ANALYTICS_RULE_ID == 0){
			Debug.Log("See Assets/Readme.txt for instructions on how to replace the rule ID and use analytics");
			return;
		}
		Debug.Log("Getting analytics snapshots");

		// Define filters
		ResultCondition condition = new ResultCondition();
		//condition.AddFilter("AppVersion", "9");
		//condition.AddFilter("location", "UK");
		//condition.GroupingKey = "gender";
		//condition.GroupingKey = "UserLevel";
		condition.DateRange = new DateRange(new DateTime(2014, 2, 2), DateTime.Now);
		
		try
		{	// My id is 147, but the ID must match the analytic rule you should create on developer.kii.com this way:
			// Go to developer.kii.com, go to your app's console
			// Click on Analytics on the left side bar, then lick on "Create a new rule"
			// Name your rule AvgDeathTime or whichever name you like, select "Event Data"
			// In the function dropdown select "Avg" and in the field enter the word "time"
			// In the type combo select "float"
			// In the dimensions fields enter "time", "time" and "float"
			// Click on Save and activate the rule
			// Once active copy the ID assigned to the rule and replace the 147 below with that

			KiiAnalytics.GetResult(GameConfig.ANALYTICS_RULE_ID.ToString(), condition, (string ruleId, ResultCondition condition2, GroupedResult result2, Exception e) => {
				if (e == null)
				{
					Debug.Log ("Analytics event upload successful");
					IList<GroupedSnapShot> snapshots = result2.SnapShots;
					Debug.Log("Cycling through analytics snapshots");
					foreach (GroupedSnapShot snapshot in snapshots)
					{
						Debug.Log ("Found a snapshot: " + snapshot.Data);
						JsonOrg.JsonArray array = snapshot.Data;
						int j = 0;
						Score.avgDeath = 0;
						for(int i = array.Length(); i > 0 ; i--){
							if(array.Get(i - 1).GetType() == typeof(JsonOrg.JsonNull))
								j++;
							else
								Score.avgDeath += (float)array.GetDouble(i - 1);
							
						}
						Score.avgDeath /= (array.Length() - j);
					}
				} else {
					Debug.Log ("Analytics snapshot fetch error: " + e.ToString());
				}
				
			});
		}
		catch (Exception e)
		{
			Debug.Log ("Analytics snapshot fetch error: " + e.ToString());
		}
	}
}
