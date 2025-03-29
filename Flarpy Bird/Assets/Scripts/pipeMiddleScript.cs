using UnityEngine;

public class pipeMiddleScript : MonoBehaviour
{
	public LogicManagerScript logic;

	private void Start()
	{
		logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
	}

	private void Update()
	{
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == 3)
		{
			logic.addScore(1);
		}
	}
}
