using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public float levelStartDelay = 2f;
    public float turnDelay = 0.1f;                          //Delay between each Player turn.

    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFood = 100;
    [HideInInspector] public bool playersTurn = true;


	private Text levelText;                                 //Text to display current level number.
	private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.
    private int level = 0;
    private List<Enemy> enemies;                          //List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;                             //Boolean to check if enemies are moving.
	private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.


    // Use this for initialization
    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        //Assign enemies to a new List of Enemy objects.
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index)
	{
		//Add one to our level number.
		level++;
		//Call InitGame to initialize our level.
		InitGame();
	}

    private void InitGame()
    {

		//While doingSetup is true the player can't move, prevent player from moving while title card is up.
		doingSetup = true;
		//Get a reference to our image LevelImage by finding it by name.
		levelImage = GameObject.Find("LevelImage");
		//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		//Set the text of levelText to the string "Day" and append the current level number.
		levelText.text = "Day " + level;
		//Set levelImage to active blocking player's view of the game board during setup.
		levelImage.SetActive(true);

		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }


	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);

		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}

    public void GameOver()
	{
		//Set levelText to display number of levels passed and game over message
		levelText.text = "After " + level + " days, you starved.";

		//Enable black background image gameObject.
		levelImage.SetActive(true);

        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if (playersTurn || enemiesMoving || doingSetup)
        {
            return;
        }

        //Start moving enemies.
        StartCoroutine(MoveEnemies());
    }

    //Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy script)
    {
        //Add Enemy to List enemies.
        enemies.Add(script);
    }
    //Coroutine to move enemies in sequence.
    IEnumerator MoveEnemies()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        //Wait for turnDelay seconds, defaults to .1 (100 ms).
        yield return new WaitForSeconds(turnDelay);

        //If there are no enemies spawned (IE in first level):
        if (enemies.Count == 0)
        {
            //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
            yield return new WaitForSeconds(turnDelay);
        }

        //Loop through List of Enemy objects.
        for (int i = 0; i < enemies.Count; i++)
        {
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemies[i].MoveEnemy();

            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        //Once Enemies are done moving, set playersTurn to true so player can move.
        playersTurn = true;

        //Enemies are done moving, set enemiesMoving to false.
        enemiesMoving = false;
    }


}
