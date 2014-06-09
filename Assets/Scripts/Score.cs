using UnityEngine;
using System.Collections;
using KiiCorp.Cloud.Storage;
using System;

public class Score : MonoBehaviour
{
	public int score = 0;					// The player's score.
	public static int highscore = -1;
	public static float avgDeath = 0;

	private PlayerControl playerControl;	// Reference to the player control script.
	private int previousScore = 0;			// The score in the previous frame.


	void Awake ()
	{
		// Setting up the reference.
		playerControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
	}


	void Update ()
	{
		// Set the score and user text.
		if (KiiUser.CurrentUser != null)
		{
			if(highscore < 0) {
				highscore = 0;
				LoadHighScore ();
			}
			string username = KiiUser.CurrentUser.Username;
			if(GameConfig.ENABLE_ANALYTICS)
				guiText.text = "Score: " + score + "  Highscore: " + highscore + "\nUser: " + username + " Avg death: " + avgDeath.ToString("n2") + " s";
			else
				guiText.text = "Score: " + score + "  Highscore: " + highscore + "\nUser: " + username;
		}
		else
			guiText.text = "Score: " + score;

		// If the score has changed...
		if(previousScore != score){
			// ... play a taunt.
			if(playerControl != null)
				playerControl.StartCoroutine(playerControl.Taunt());
			SaveScore (score);
		}

		// Set the previous score to this frame's score.
		previousScore = score;
	}

	void SaveScore (int score)
	{
		KiiUser user = KiiUser.CurrentUser;
		if (user == null) {
			return;
		}
		KiiBucket bucket = user.Bucket ("scores");
		KiiObject kiiObj = bucket.NewKiiObject ();
		kiiObj ["score"] = score;
		kiiObj ["time"] = Time.time;
		kiiObj ["level"] = 1;
		Debug.Log ("Saving score...");
		kiiObj.Save((KiiObject obj, Exception e) => {
			if (e != null)
				Debug.LogError(e.ToString());
			else
				Debug.Log("Score sent: " + score.ToString());
		});
	}

	public static void LoadHighScore () {
		if (KiiUser.CurrentUser == null) {
			return;
		}
		
		KiiUser user = KiiUser.CurrentUser;
		KiiBucket bucket = user.Bucket ("scores");
		KiiQuery query = new KiiQuery ();
		query.SortByDesc ("score");
		query.Limit = 1;

		bucket.Query(query, (KiiQueryResult<KiiObject> list, Exception e) =>{
			if (e != null)
			{
				Debug.LogError ("Failed to load high score " + e.ToString());
			} else {
				foreach (KiiObject obj in list) {
					highscore = obj.GetInt ("score", 0);
					Debug.Log ("High score loaded: " + highscore.ToString());
					return;
				}
			}
		});

	}
}
