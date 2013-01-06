using UnityEngine;
using System.Collections;

public class Element : MonoBehaviour {
	
	public SpriteManager spriteManager;
	public string[] stringForm;
	public string elementName;
	
	public bool isRotatable;
	public bool isMirrowable;
	private bool isMirrowed = false;
	private int rotatedAngle = 0;
	
	private int width;
	private int height;
	
	private Sprite[] sprites;
	public int SPRITE_WIDTH = 24;
	private int colors = 5;
	private int colorIndex = 0;
	
	// Use this for initialization
	void Awake () {
		GameObject initSpriteManager = GameObject.Find("InitElementSpriteManager");
		spriteManager = initSpriteManager.GetComponent<LinkedSpriteManager>();
		
		colorIndex = Random.Range(0, colors);
		isMirrowed = isMirrowable && Random.Range(0, 1 + 1) > 0 ? true : false;		
		
		int blocks = 0;
		width = 0;
		height = stringForm.Length;
		for(int i = 0; i < stringForm.Length; i++) {
			width = Mathf.Max(width, stringForm[i].Length);
			for(int j = 0; j < stringForm[i].Length; j++) {
				if(stringForm[i][j].CompareTo('1') == 0) 
					blocks ++;
			}
		}
		
		if(isMirrowed) {
			string[] newStringForm = new string[stringForm.Length];
			int j = 0;
			for(int i = stringForm.Length - 1; i >= 0; i--) {
				newStringForm[j++] = stringForm[i];
			}
			stringForm = newStringForm;
		}
		
		sprites = new Sprite[blocks];
		int iSprite = 0;
		for(int i = 0; i < stringForm.Length; i++) {
			for(int j = 0; j < stringForm[i].Length; j++) {
				if(stringForm[i][j].CompareTo('1') == 0) {
					Vector3 moveTo = new Vector3(((float)j-(float)width/2f+0.5f)*SPRITE_WIDTH, (-(float)i+(float)height/2f-0.5f)*SPRITE_WIDTH, 0);
					sprites[iSprite++] = spriteManager.AddSprite(gameObject, SPRITE_WIDTH, SPRITE_WIDTH, 
						new Vector2(1f*colorIndex/colors, 0), 
						new Vector2(1f/colors, 1), 
						moveTo, 
						false);					
				}
			}
		}
		
		if(isRotatable)
			rotatedAngle = 90 * Random.Range(0, 3 + 1);
		if(rotatedAngle > 0)
			Rotate(0);	// Update rotation
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public int color {
		get {
			return colorIndex;
		}
	}
	
	public string[] matrix {
		get { 
			char[,] mat;
			string[] returnmat;
			if(rotatedAngle == 0 || rotatedAngle == 180) {
				mat = new char[height, width];
				returnmat = new string[height];
			}
			else {
				mat = new char[width, height];
				returnmat = new string[width];
			}
			
			for(int y = 0; y < stringForm.Length; y++) {
				for(int x = 0; x < stringForm[y].Length; x++) {
					int mx=0, my=0;
					switch(rotatedAngle) {
					case 0:
						my = y; mx = x; break;
					case 90:
						my = width - x - 1; mx = y;	break;
					case 180:
						my = height - y - 1; mx = width - x - 1;	break;
					case 270:
						my = x; mx = height - y - 1;	break;
					}
					
					mat[my, mx] = (stringForm[y][x].CompareTo('1') == 0) ? '1' : '0';					
				}
			}
			for(int y = 0; y < returnmat.Length; y++) {
				for(int x = 0; x < ((rotatedAngle == 0 || rotatedAngle == 180) ? width : height); x++)
					returnmat[y] += mat[y, x];
			}		
			
			return returnmat;
		}
	}
	
	public string[] getRotatedMatrix(int angle) {
		rotatedAngle += angle;
		while(rotatedAngle < 0)
			rotatedAngle +=360;
		while(rotatedAngle >= 360)
			rotatedAngle -=360;		
		
		string[] mat = matrix;
		
		rotatedAngle -= angle;
		while(rotatedAngle < 0)
			rotatedAngle +=360;
		while(rotatedAngle >= 360)
			rotatedAngle -=360;				
		
		return mat;
	}
	
	public void Rotate(int angle) {
		rotatedAngle += angle;
		while(rotatedAngle < 0)
			rotatedAngle +=360;
		while(rotatedAngle >= 360)
			rotatedAngle -=360;
		
		gameObject.transform.eulerAngles = new Vector3(0, 0, rotatedAngle);
	}
	
	public void MoveLeft() {
		Vector3 pos = gameObject.transform.position;
		gameObject.transform.position = new Vector3(pos.x - SPRITE_WIDTH, pos.y, pos.z);
	}	
	
	public void MoveRight() {
		Vector3 pos = gameObject.transform.position;
		gameObject.transform.position = new Vector3(pos.x + SPRITE_WIDTH, pos.y, pos.z);
	}
	
	public void MoveDown() {
		Vector3 pos = gameObject.transform.position;
		gameObject.transform.position = new Vector3(pos.x, pos.y - SPRITE_WIDTH, pos.z);
	}	
}
