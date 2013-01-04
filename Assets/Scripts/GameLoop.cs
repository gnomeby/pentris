using UnityEngine;
using System.Collections;

public class GameLoop : MonoBehaviour {
	
	private bool DEBUG_MODE = false;
	private int CONFLICT_ELEMENT = 1;
	private int CONFLICT_GLASS = 2;
	private int CONFLICT_GLASS_OUTOFRANGE = 4;
	
	public GameObject[] elements;
	public int horizontalBlocks;
	public int verticalBlocks;
	public float speed;
	
	private string[] glass;
	
	private GameObject currentGO;
	private Element currentElement;
	private int curEl_width, curEl_height, curElGlass_x, curElGlass_y;
	
	private Vector3 leftTopScreenGlassCorner = new Vector3(89, 472, 0);
	
	private GameObject nextElement;
	private Vector3 nextElementCenter = new Vector3(455, 363, 0);
	
	private float nextDownTime;
	
	private bool nextItem = false;
	private bool isGameOver = false;
	
	private int defaultRotateAngle = 90;
	
	void Awake() {
		Application.targetFrameRate = 60;
	}
	
	// Use this for initialization
	void Start () {
		// Generate first element		
		int index = Random.Range(0, elements.Length);
		if(DEBUG_MODE)
			index = 1;
		nextElement = (GameObject)Instantiate(elements[index], nextElementCenter, gameObject.transform.rotation);
		nextItem = true;
	
		// Define next down time		
		nextDownTime = Time.time + speed;
		
		// Init glass
		glass = new string[verticalBlocks+2];
		for(int y = 0; y < verticalBlocks+2; y++) {
			for(int x = 0; x < horizontalBlocks+2; x++) {
				glass[y] += (x == 0 || y == 0 || y + 1 == verticalBlocks+2 || x + 1 == horizontalBlocks+2) ? '2' : '0';
			}
		}			
	}
	
	// Update is called once per frame
	void Update () {
		if(isGameOver)
			return;
		
		if(nextItem) {
			prepareNextItem();
			
			nextItem = false;
			nextDownTime = Time.time + speed;
		}
		else if(Time.time >= nextDownTime) {
			MoveDown(1);
			nextDownTime = Time.time + speed;
		}
	}
	
	void prepareNextItem() {
		// Prepare current element
		currentGO = nextElement;
		currentElement = currentGO.GetComponent<Element>();
		string[] matrix = currentElement.matrix;
		curEl_height = matrix.Length;
		curEl_width = matrix[0].Length;		
		curElGlass_x = 0;
		curElGlass_y = 0;
				
		// Generate next element
		int index = Random.Range(0, elements.Length);
		nextElement = (GameObject)Instantiate(elements[index], currentGO.transform.position, gameObject.transform.rotation);
			
		// Move current into the glass to the left line
		Vector3 newPosition = new Vector3(
			leftTopScreenGlassCorner.x + currentElement.SPRITE_WIDTH * curEl_width / 2, 
			leftTopScreenGlassCorner.y - currentElement.SPRITE_WIDTH * curEl_height / 2, 
			leftTopScreenGlassCorner.z);
		if(matrix[0].Trim('0'.ToString().ToCharArray()).Length == 0) {
			newPosition.y += currentElement.SPRITE_WIDTH;
			curElGlass_y--;
		}
		currentGO.transform.position = newPosition;
		
		// Move to the center
		curElGlass_x = horizontalBlocks / 2 - curEl_width / 2;
		int LeftLineBlocksCount = 0;
		for(int y = 0; y < matrix.Length; y++) {
			if(matrix[y][0] == '1')
				LeftLineBlocksCount++;			
		}
		if(LeftLineBlocksCount == 0) {
			curElGlass_x--;
		}
		currentElement.MoveMultipleRight(curElGlass_x);
		if(PredictMovementCollision(0, 0) != 0)
			isGameOver = true;
	}
	
	void finishCurrentItem() {
		string[] newGlass = new string[glass.Length];
		string[] matrix = currentElement.matrix;		
		
		for(int gy = 0; gy < glass.Length; gy++) {
			for(int gx = 0; gx < glass[gy].Length; gx++) {
				// gy = curElGlass_y + 1 + y + 0
				// gx = curElGlass_x + 1 + x + 0
				int x = gx - (curElGlass_x + 1);
				int y = gy - (curElGlass_y + 1);
				if(x >=0 && y >=0 && x < matrix[0].Length && y < matrix.Length)
					newGlass[gy] += matrix[y][x];
				else
					newGlass[gy] += glass[gy][gx];
			}
		}
		glass = newGlass;
	}
	
	void OnGUI() {
		if(!currentElement)
			return;
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown) {
			if(e.keyCode == KeyCode.UpArrow) {
				Rotate();
			}
			else if(e.keyCode == KeyCode.DownArrow) {
				MoveDown(1);
			}
			else if(e.keyCode == KeyCode.LeftArrow) {
				MoveLeft(1);
			}
			else if(e.keyCode == KeyCode.RightArrow) {
				MoveRight(1);
			}
		}        
    }
	
	void Rotate() {
		if(DEBUG_MODE)
			currentElement.isRotatable = true;
		if(currentElement.isRotatable == false)
			return;
				
		string[] matrix = currentElement.getRotatedMatrix(defaultRotateAngle);
		
		// Correct glass coordinates for element
		int swap = curEl_height;
		curEl_height = curEl_width;
		curEl_width = swap;
		int diff = Mathf.Abs(curEl_height - curEl_width) / 2;
		curElGlass_x += (curEl_height > curEl_width ? 1 : -1) * diff;
		curElGlass_y += (curEl_height > curEl_width ? -1 : 1) * diff;

		// Normal rotation
		int conflictMask = PredictMovementCollision(0, 0, matrix);
		if(conflictMask == 0) {
			currentElement.SendMessage("Rotate", defaultRotateAngle, SendMessageOptions.DontRequireReceiver);
			return;
		}
		if((conflictMask & CONFLICT_ELEMENT) == CONFLICT_ELEMENT)
			return;
		
		// Spring aside logic
		int[] xopts = {0, -1, 1, -2, 2};
		for(int x = 0; x < xopts.Length; x++) {
			for(int y = 0; y <= 2; y++) {
				if((y == 0 && x == 0) || (y > 0 && x > 0))
					continue;
				
				conflictMask = PredictMovementCollision(y, xopts[x], matrix);
				if(conflictMask == 0) {
					currentElement.SendMessage("Rotate", defaultRotateAngle, SendMessageOptions.DontRequireReceiver);
					if(xopts[x] > 0)
						MoveRight(xopts[x]);
					if(xopts[x] < 0)
						MoveLeft(-xopts[x]);
					MoveDown(y);
					goto endSpringAside;
				}
			}
		}
		
		endSpringAside:
		return;		
	}
	
	void MoveLeft(int step) {
		if(step > 0 && PredictMovementCollision(0, -step) == 0) {
			while(step > 0) {			
				curElGlass_x--;
				currentElement.SendMessage("MoveLeft", SendMessageOptions.DontRequireReceiver);
				step--;
			}
		}		
	}
	
	void MoveRight(int step) {
		if(step > 0 && PredictMovementCollision(0, step) == 0) {
			while(step > 0) {			
				curElGlass_x++;
				currentElement.SendMessage("MoveRight", SendMessageOptions.DontRequireReceiver);
				step--;
			}
		}		
	}

	void MoveDown(int step) {
		if(step > 0) {
			int conflictMask = PredictMovementCollision(step, 0);
			if(conflictMask > 0) {
				finishCurrentItem();
				nextItem = true;				
				return;
			}
			while(step > 0) {
				curElGlass_y++;
				currentElement.SendMessage("MoveDown", SendMessageOptions.DontRequireReceiver);
				step--;
			}
		}		
	}
	
	private int PredictMovementCollision(int my, int mx) {
		string[] matrix = currentElement.matrix;
		
		return PredictMovementCollision(my, mx, matrix);
	}
	
	/**
	 * @return int	Boolean mask:
	 * 				0 - conflict with nothing
	 * 				1 - conflict with another blocks
	 * 				2 - conflict with glass border 
	 */
	private int PredictMovementCollision(int my, int mx, string[] matrix) {
		int conflictMask = 0;
		for(int y = 0; y < matrix.Length; y++) {
			for(int x = 0; x < matrix[y].Length; x++) {
				try {
					if(matrix[y][x] == '1' && glass[curElGlass_y + 1 + y + my][curElGlass_x + 1 + x + mx] != '0') {
						char c = glass[curElGlass_y + 1 + y + my][curElGlass_x + 1 + x + mx];
						if(c == '1')
							conflictMask |= CONFLICT_ELEMENT;
						if(c == '2')
							conflictMask |= CONFLICT_GLASS;
						goto endCheckMovement;
					}
				}
				catch(System.IndexOutOfRangeException e) {
					return CONFLICT_GLASS_OUTOFRANGE;
				}
			}
		}
		
		endCheckMovement:
		return conflictMask;
	}	
}
