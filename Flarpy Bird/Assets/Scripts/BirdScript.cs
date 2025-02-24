using UnityEngine;

public class BirdScript : MonoBehaviour
{
	public Rigidbody2D rigidbody;

	public LogicManagerScript logic;

	public float flarpStrength = 30f;

	public bool BirdIsAlive = true;

	private void Start()
	{
		rigidbody.GetComponent<Rigidbody2D>();
		logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && BirdIsAlive)
		{
			rigidbody.velocity = Vector2.up * flarpStrength;
		}
		if (Input.GetKeyUp(KeyCode.X) && !BirdIsAlive)
		{
			Application.Quit();
		}
		logic.IsDead();
	}

	public void OnCollisionEnter2D()
	{
		logic.GameOver();
		BirdIsAlive = false;
	}
}
