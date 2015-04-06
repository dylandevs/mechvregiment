using UnityEngine;
using System.Collections;

public class MiniMapDamgeIcon : MonoBehaviour {
	
	public float life;

	public PoolManager pool;

	public SpriteRenderer sprite;

	public GameObject mechPos;
	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {



		if(life >= 0){
			life -= Time.deltaTime;
		}
		
		if(life < 0){
			pool.Deactivate(gameObject);
		}
		
		Color tempColour = sprite.color;
		tempColour.a = life/2f;
		sprite.color = tempColour;
		
	}
	
	void OnEnable(){
		life = 3f;
	}

	void updatePos(){
		Vector3 newPos = mechPos.transform.position + new Vector3(0,130,0);
		transform.position = newPos;
	}
}
