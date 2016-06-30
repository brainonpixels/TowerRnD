using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	private static GameControl gc;
	
	public static GameControl get() {
		return gc;
	}

	public Transform player;
	public Transform hud;
	public Transform levelGenerator;

	private Transform gameOverScreen;

	private RadialPlayerControl playerControl;
	private LevelGenerator_0_1 levelGen;

	public GameControl() {
		gc = this;
	}

	void Start () {

		gameOverScreen = hud.Find ("GameOverScreen");
		playerControl = player.GetComponent<RadialPlayerControl> ();
		levelGen = levelGenerator.GetComponent<LevelGenerator_0_1> ();

		levelGen.startLevel ();
		playerControl.setSpawnPosition (levelGen.getSpawningLocation());
		playerControl.respawn ();
	}

	void Update () {
	
	}

	public void gameOver() {
		gameOverScreen.gameObject.SetActive (true);
	}

	public void restart() {
		playerControl.respawn ();
		gameOverScreen.gameObject.SetActive (false);
	}
}
