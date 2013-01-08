using UnityEngine;
using System.Collections;

public class MenuLoop : MonoBehaviour {
	
	public GameObject playGame;
	public GameObject quitGame;
	
	private enum menu : int { Play, Quit };
	private int currentOption = 0;

	// Use this for initialization
	void Start () {
		UpdateGUI();		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI() {
		
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown) {
			if(e.keyCode == KeyCode.UpArrow) {
				currentOption = (int)menu.Play;
			}
			else if(e.keyCode == KeyCode.DownArrow) {
				currentOption = (int)menu.Quit;
			}
			else if(e.keyCode == KeyCode.Return) {
				if(currentOption == (int)menu.Play) {
					Application.LoadLevel(1);
				}
				else {
					Application.Quit();
				}					
			}
			else if(e.keyCode == KeyCode.Escape) {
				Application.Quit();
			}
			
			UpdateGUI();
		}        
		
	}
	
	void UpdateGUI() {
		playGame.guiText.fontStyle = currentOption == (int)menu.Play ? FontStyle.Bold : FontStyle.Normal;
		quitGame.guiText.fontStyle = currentOption == (int)menu.Quit ? FontStyle.Bold : FontStyle.Normal;
	}
}
