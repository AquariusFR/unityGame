using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

      //Allows us to use SceneManager
using UnityEngine.UI;

public class Player : MovingObject
{

	public int damageToWall = 1;
	public float restartLevelDelay = 1f;
	//Delay time in seconds to restart level.
	public int pointsPerFood = 10;
	//Number of points to add to player food points when picking up a food object.
	public int pointsPerSoda = 20;
	//Number of points to add to player food points when picking up a soda object.

	public Text footText;

	private Animator animator;
	//Used to store a reference to the Player's animator component.
	private int food;
	//Used to store player food points total during level.

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;



	// Use this for initialization
	protected override void Start ()
	{
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator> ();

		//Get the current food point total stored in GameManager.instance between levels.
		food = GameManager.instance.playerFood;
		UpdateFoodText ();

		base.Start ();
	}

	private void UpdateFoodText ()
	{
		footText.text = "Food: " + food;
	}

	private void decrementFood (int footLoosed)
	{
		food -= footLoosed;
		footText.text = "-" + footLoosed + "Food: " + food;
	}

	private void incrementFood (int footAdded)
	{
		food += footAdded;
		footText.text = "+" + footAdded + "Food: " + food;
	}

	protected override void AttemptMove<T> (int xDir, int yDir)
	{
		food--;
		UpdateFoodText ();

		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		base.AttemptMove<T> (xDir, yDir);

		//Hit allows us to reference the result of the Linecast done in Move.
		RaycastHit2D hit;

		//If Move returns true, meaning Player was able to move into an empty space.
		if (Move (xDir, yDir, out hit)) {

			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);


			if (yDir == 0) {
				if (xDir > 0) {
					animator.SetTrigger ("PlayerRight");
				} else {
					animator.SetTrigger ("PlayerLeft");
				}
			} else {
				if (yDir > 0) {
					animator.SetTrigger ("PlayerUp");
				} else {
					animator.SetTrigger ("PlayerDown");
				}
			}

			//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
		}
		//Since the player has moved and lost food points, check if the game has ended.
		CheckIfGameOver ();
		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameManager.instance.playersTurn = false;
	}

	private void OnDisable ()
	{
		//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
		GameManager.instance.playerFood = food;
	}

	// Update is called once per frame
	void Update ()
	{
		//If it's not the player's turn, exit the function.
		if (!GameManager.instance.playersTurn) {
			return;
		}

		int horizontal = 0;     //Used to store the horizontal move direction.
		int vertical = 0;       //Used to store the vertical move direction.

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		horizontal = (int)(Input.GetAxisRaw ("Horizontal"));

		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int)(Input.GetAxisRaw ("Vertical"));


		//Check if moving horizontally, if so set vertical to zero.
		if (horizontal != 0) {
			vertical = 0;
		}
		//Check if we have a non-zero value for horizontal or vertical
		if (horizontal != 0 || vertical != 0) {
			//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
			//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
			AttemptMove<Wall> (horizontal, vertical);
		}
	}

	//OnCantMove overrides the abstract function OnCantMove in MovingObject.
	//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
	protected override void OnCantMove<T> (T _wall)
	{

		Wall hitWall = _wall as Wall;
		//Call the DamageWall function of the Wall we are hitting.
		hitWall.DamageWall (damageToWall);

		//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
		animator.SetTrigger ("PlayerLeft");
	}


	//Restart reloads the scene when called.
	private void Restart ()
	{
		footText.text = "EXIT !!!!";
		//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
		//and not load all the scene object in the current scene.
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}

	//LoseFood is called when an enemy attacks the player.
	//It takes a parameter loss which specifies how many points to lose.
	public void TakeDamage (int loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("PlayerUp");

		//Subtract lost food points from the players total.
		decrementFood (loss);

		//Check to see if game has ended.
		CheckIfGameOver ();
	}


	//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
	private void OnTriggerEnter2D (Collider2D other)
	{
		//Check if the tag of the trigger collided with is Exit.
		if (other.tag == "Exit") {
			//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
			Invoke ("Restart", restartLevelDelay);

			//Disable the player object since level is over.
			enabled = false;
		}

        //Check if the tag of the trigger collided with is Food.
		else if (other.tag == "Food") {
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			//Add pointsPerFood to the players current food total.
			incrementFood (pointsPerFood);

			//Disable the food object the player collided with.
			other.gameObject.SetActive (false);
		}

        //Check if the tag of the trigger collided with is Soda.
		else if (other.tag == "Soda") {
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
			//Add pointsPerSoda to players food points total
			incrementFood (pointsPerSoda);

			//Disable the soda object the player collided with.
			other.gameObject.SetActive (false);
		}
	}





	//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
	private void CheckIfGameOver ()
	{
		//Check if food point total is less than or equal to zero.
		if (food <= 0) {

			SoundManager.instance.RandomizeSfx (gameOverSound);
			SoundManager.instance.musicSource.Stop();
			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver ();
		}
	}
}
