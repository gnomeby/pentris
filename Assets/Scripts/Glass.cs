using UnityEngine;
using System.Collections;

public class Glass : MonoBehaviour {
	
	public SpriteManager spriteManager;
	
	private const float SPRITE_WIDTH = 640;
	private const float SPRITE_HEIGHT = 480;
	
	void Awake() {
		GameObject initSpriteManager = GameObject.Find("InitBackGroundSpriteManager");
		spriteManager = initSpriteManager.GetComponent<LinkedSpriteManager>();
	}
	
	// Use this for initialization
	void Start () {
		spriteManager.AddSprite(gameObject, SPRITE_WIDTH, SPRITE_HEIGHT, 
						new Vector2(0, 0), 
						new Vector2(1, 1), 
						new Vector3(SPRITE_WIDTH / 2, SPRITE_HEIGHT / 2, 0), 
						false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
