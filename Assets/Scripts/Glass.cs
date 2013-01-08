using UnityEngine;
using System.Collections;

using AssemblyCSharp;

public class Glass : MonoBehaviour {
	
	public SpriteManager spriteManagerGlass, spriteManagerBlocks;
	private GameLoop gameLoop;
	
	public int BackgroundWidth = 640;
	public int BackgroundHeight = 480;
	
	public GameObject[] elements;
	public int BlockSpriteSize = 24;
	public int Colors = 5;
	
	public int HorizontalBlocks = 10;
	public int VerticalBlocks = 19;
	
	public Vector3 nextElementCenter = new Vector3(455, 363, 0);
	public Vector3 leftTopScreenGlassCorner = new Vector3(89, 472, 0);
	
	private Sprite[,] glassSprites;
	
	private GameObject currentGO, nextGO;
	private Element currentElement;
	private int curEl_width, curEl_height, curElGlass_x, curElGlass_y;	
	
	// Use this for initialization
	void Start () 
	{
		gameLoop = GameObject.Find("_Game").GetComponent<GameLoop>();
		spriteManagerBlocks = GameObject.Find("InitElementSpriteManager").GetComponent<LinkedSpriteManager>();

		spriteManagerGlass = GameObject.Find("InitBackGroundSpriteManager").GetComponent<LinkedSpriteManager>();
		spriteManagerGlass.AddSprite(gameObject, BackgroundWidth, BackgroundHeight, 
						new Vector2(0, 0), 
						new Vector2(1, 1), 
						new Vector3(BackgroundWidth / 2, BackgroundHeight / 2, 0), 
						false);

		glassSprites = new Sprite[VerticalBlocks, HorizontalBlocks];
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	public GameObject ThrowNextElement()
	{
		currentGO = nextGO;
		currentElement = currentGO.GetComponent<Element>();
		string[] matrix = currentElement.matrix;
		curEl_height = matrix.Length;
		curEl_width = matrix[0].Length;
		
		curElGlass_x = HorizontalBlocks / 2 - curEl_width / 2;
		curElGlass_y = 0;
				
		// Generate next element
		nextGO = CreateNextElement();
			
		// Move current into the glass to the left top line
		Vector3 newPosition = new Vector3(
			leftTopScreenGlassCorner.x + BlockSpriteSize * curEl_width / 2, 
			leftTopScreenGlassCorner.y - BlockSpriteSize * curEl_height / 2, 
			leftTopScreenGlassCorner.z);		

		// Move up if upper line of element don't contain blocks
		if(matrix[0].Trim('0'.ToString().ToCharArray()).Length == 0) {
			curElGlass_y--;
		}
		
		// Move left if left line of element don't contain blocks
		int LeftLineBlocksCount = 0;
		for(int y = 0; y < matrix.Length; y++) {
			if(matrix[y][0] == '1')
				LeftLineBlocksCount++;			
		}
		if(LeftLineBlocksCount == 0) {
			curElGlass_x--;
		}
		
		// Move to current position
		newPosition.y += BlockSpriteSize * curElGlass_y * (-1);
		newPosition.x += BlockSpriteSize * curElGlass_x;		
		
		// Set new position
		currentGO.transform.position = newPosition;
		
		return currentGO;
	}
	
	public GameObject CreateNextElement() 
	{
		int index = Random.Range(0, elements.Length);
		return nextGO = (GameObject)Instantiate(elements[index], nextElementCenter, gameObject.transform.rotation);
	}
	
	public void AbsorbElement(Element el, int ypos, int xpos) 
	{
		string[] matrix = el.matrix;
		int colorIndex = el.color;
		
		for(int gy = 0; gy < VerticalBlocks; gy++) {
			for(int gx = 0; gx < HorizontalBlocks; gx++) {
				// gy = ypos + y
				// gx = xpos + x
				int y = gy - ypos;
				int x = gx - xpos;
				if(x >=0 && y >=0 && x < matrix[0].Length && y < matrix.Length && matrix[y][x] == '1') {
					Vector3 moveTo = new Vector3(
						leftTopScreenGlassCorner.x + ((float)gx+0.5f)*BlockSpriteSize, 
						leftTopScreenGlassCorner.y + (-(float)gy-0.5f)*BlockSpriteSize, 
						0
						);
					glassSprites[gy, gx] = spriteManagerBlocks.AddSprite(
						gameObject,
						BlockSpriteSize, BlockSpriteSize,
						new Vector2(1f*colorIndex/Colors, 0),
						new Vector2(1f/Colors, 1),
						moveTo,
						false);
				}
			}
		}		
	}
	
	/**
	 * @matrix string[] Element matrix
	 * @elX, elY int	Glass position for element
	 * @mX, mY int  	Movement direction
	 * 
	 * @return int	Boolean mask:
	 * 				0 - conflict with nothing
	 * 				1 - conflict with another blocks
	 * 				2 - conflict with glass border 
	 */
	public int MatrixCollision(string[] matrix, int elX, int elY, int mX, int mY)
	{
		int conflictMask = 0;
		
		for(int y = 0; y < matrix.Length; y++) {
			for(int x = 0; x < matrix[y].Length; x++) {
				int gY = elY + y + mY;
				int gX = elX + x + mX;
				
				if(matrix[y][x] == '1') {					
					if(gY >= 0 && gY < VerticalBlocks && gX >=0 && gX < HorizontalBlocks) {
						if(glassSprites[gY, gX] != null)
							conflictMask |= (int)Conflicts.Element;
					}
					else if(gY == -1 || gY == VerticalBlocks && gX == -1 && gX == HorizontalBlocks) {
						conflictMask |= (int)Conflicts.Border;
					}
					else {
						conflictMask |= (int)Conflicts.OutOfRange;
					}
				}
			}
		}
		
		return conflictMask;
	}
	
	private int MatrixCollision(int mY, int mX) 
	{
		string[] matrix = currentElement.matrix;
		
		return MatrixCollision(matrix, curElGlass_x, curElGlass_y, mX, mY);
	}	
	
	public bool RotateElement(int angle) 
	{
		if(currentElement.isRotatable == false)
			return false;
				
		string[] matrix = currentElement.getRotatedMatrix(angle);
		
		// Correct glass coordinates for element
		int swap = curEl_height;
		curEl_height = curEl_width;
		curEl_width = swap;
		int diff = Mathf.Abs(curEl_height - curEl_width) / 2;
		curElGlass_x += (curEl_height > curEl_width ? 1 : -1) * diff;
		curElGlass_y += (curEl_height > curEl_width ? -1 : 1) * diff;

		// Normal rotation
		int conflictMask = MatrixCollision(matrix, curElGlass_x, curElGlass_y, 0, 0);
		if(conflictMask == 0) {
			currentElement.Rotate(angle);
			return true;
		}
		if((conflictMask & (int)Conflicts.Element) > 0)
			return false;
		
		// Spring aside glass borders logic
		int[] xopts = {0, -1, 1, -2, 2};
		for(int x = 0; x < xopts.Length; x++) {
			for(int y = 0; y <= 2; y++) {
				// Only vertical or horizontal spring back
				if((y == 0 && x == 0) || (y > 0 && x > 0))
					continue;
				
				int mX = xopts[x], mY = y;
				conflictMask = MatrixCollision(matrix, curElGlass_x, curElGlass_y, mX, mY);
				if(conflictMask == 0) {
					currentElement.Rotate(angle);
					if(mX > 0)
						MoveElementRight(mX);
					if(mX < 0)
						MoveElementLeft(-mX);
					MoveElementDown(mY);
					goto endSpringAside;
				}
			}
		}
		
		endSpringAside:
		return true;		
	}
	
	public bool MoveElementLeft(int step) 
	{
		if(step == 0)
			return true;
		if(MatrixCollision(0, -step) != 0)
			return false;
		
		while(step > 0) {
			curElGlass_x--;
			currentElement.MoveLeft();
			step--;
		}

		return true;
	}
	
	public bool MoveElementRight(int step) 
	{
		if(step == 0)
			return true;
		if(MatrixCollision(0, step) != 0)
			return false;
		
		while(step > 0) {
			curElGlass_x++;
			currentElement.MoveRight();
			step--;
		}

		return true;		
	}

	public bool MoveElementDown(int step) 
	{
		if(step == 0)
			return true;
		if(MatrixCollision(step, 0) != 0)
			return false;
		
		while(step > 0) {
			curElGlass_y++;
			currentElement.MoveDown();
			step--;
		}
		
		return true;
	}
	
	public bool FinishElement()
	{
		AbsorbElement(currentElement, curElGlass_y, curElGlass_x);
		currentElement.Remove();
		
		gameLoop.AddDeletedLines(RemoveFullLines());
		
		currentGO = ThrowNextElement();
		currentElement = currentGO.GetComponent<Element>();
		if(MatrixCollision(0, 0) != 0)
			return false;
		
		return true;
	}
	
	private int RemoveFullLines()
	{
		int deletedLines = 0;
		for(int gy = 0; gy < VerticalBlocks; gy++) {
			int blocks = 0;
			for(int gx = 0; gx < HorizontalBlocks; gx++) {
				if(glassSprites[gy, gx] != null)
					blocks++;
			}
			if(blocks == HorizontalBlocks) {
				RemoveFullLine(gy);
				deletedLines++;
			}
		}
		
		return deletedLines;
	}
	
	private void RemoveFullLine(int yline)
	{
		for(int gx = 0; gx < HorizontalBlocks; gx++) {
			spriteManagerBlocks.RemoveSprite(glassSprites[yline, gx]);
		}
		
		for(int gy = yline; gy >= 1; gy--) {
			for(int gx = 0; gx < HorizontalBlocks; gx++) {
				glassSprites[gy, gx] = glassSprites[gy-1, gx];
				if(glassSprites[gy, gx] != null) {
					Vector3 curPos = glassSprites[gy, gx].offset;
					glassSprites[gy, gx].offset = new Vector3(curPos.x, curPos.y - BlockSpriteSize, curPos.z);
					glassSprites[gy, gx].SetSizeXY(BlockSpriteSize, BlockSpriteSize);
				}
			}
		}		
	}
}























