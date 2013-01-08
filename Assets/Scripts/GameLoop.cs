using UnityEngine;
using System.Collections;

using AssemblyCSharp;

public class GameLoop : MonoBehaviour {
	public float startSpeed = 0.8f;
	public int level = 1;
	public int FPS = 60;
	
	public GUIText textLevel;
	public GUIText textLines;
	public GUIText textScore;
	public GUIText textGameOver;
	
	private const int defaultStep = 1;
	private const int defaultRotateAngle = 90;
	
	private Glass glass;

	private float nextDownTime;
	
	private bool isGameOver = false;	
	private int lines = 0;
	private int score = 0;
	
	void Awake() 
	{
		Application.targetFrameRate = FPS;
	}
	
	// Use this for initialization
	void Start () 
	{
		// Init texts
		updateGUI();
		
		// Init glass
		glass = GameObject.Find("Glass").GetComponent<Glass>();
		
		// Generate first element && throw it
		glass.CreateNextElement();
		glass.ThrowNextElement();
		
		nextDownTime = Time.time + startSpeed;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(isGameOver)
			return;
		
		if(Time.time >= nextDownTime) {
			bool retval = glass.MoveElementDown(defaultStep);
			if(!retval)
				NextElement();
			
			nextDownTime = Time.time + startSpeed;
		}
	}
	
	void NextElement()
	{
		bool retval = glass.FinishElement();
		if(retval == false) {
			isGameOver = true;
			textGameOver.enabled = true;
		}
	}
	
	void OnGUI() 
	{
		if(isGameOver)
			return;
		
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown) {
			if(e.keyCode == KeyCode.UpArrow) {
				glass.RotateElement(defaultRotateAngle);
			}
			else if(e.keyCode == KeyCode.DownArrow) {
				bool retval = glass.MoveElementDown(defaultStep);
				if(!retval)
					NextElement();
			}
			else if(e.keyCode == KeyCode.LeftArrow) {
				glass.MoveElementLeft(defaultStep);
			}
			else if(e.keyCode == KeyCode.RightArrow) {
				glass.MoveElementRight(defaultStep);
			}
			else if(e.keyCode == KeyCode.Space) {
				while(glass.MoveElementDown(defaultStep)) {
				}
			}			
		}        
    }
	
	public void AddDeletedLines(int deletedLines)
	{
		lines += deletedLines;
		switch(deletedLines) {
			case 1:
				score += 100;
				break;
			case 2:
				score += 300;
				break;
			case 3:
				score += 800;
				break;
			case 4:
				score += 1500;
				break;
			case 5:
				score += 2000;
				break;
		}
		updateGUI();
	}
	
	void updateGUI()
	{
		textLevel.text = "" + level;
		textLines.text = "" + lines;
		textScore.text = "" + score;
	}
}
