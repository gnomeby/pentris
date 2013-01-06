using UnityEngine;
using System.Collections;

using AssemblyCSharp;

public class GameLoop : MonoBehaviour {
	public float speed = 1;
	public int FPS = 1;
	
	private const int defaultStep = 1;
	private const int defaultRotateAngle = 90;
	
	private Glass glass;

	private float nextDownTime;
	
	private bool isGameOver = false;	
	
	void Awake() 
	{
		Application.targetFrameRate = FPS;
	}
	
	// Use this for initialization
	void Start () 
	{
		// Init glass
		glass = GameObject.Find("Glass").GetComponent<Glass>();
		
		// Generate first element && throw it
		glass.CreateNextElement();
		glass.ThrowNextElement();
		
		nextDownTime = Time.time + speed;
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
			
			nextDownTime = Time.time + speed;
		}
	}
	
	void NextElement()
	{
		bool retval = glass.FinishElement();
		if(retval == false)
			isGameOver = true;		
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
}
