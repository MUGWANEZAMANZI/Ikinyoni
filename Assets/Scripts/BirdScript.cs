using UnityEngine;

public class BirdScript : MonoBehaviour
{
	public Rigidbody2D rigidbody;

	public LogicManagerScript logic;
	public MarsQuizManager manager;

	public float flarpStrength = 30f;

	public bool BirdIsAlive = true;

    private bool freezeYPosition = false;

    private void Start()
	{
		rigidbody.GetComponent<Rigidbody2D>();
		logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
	}

	private void Update()
	{
		if (manager.inQuiz)
			{
			freezeYPosition = true;
		}
		else
		{
			freezeYPosition = false;
		}

        if (freezeYPosition)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f); // Zero out Y velocity
            rigidbody.position = new Vector2(rigidbody.position.x, rigidbody.position.y); // Keep Y position constant
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.touchCount > 0 ) && BirdIsAlive)
		{
			rigidbody.velocity = Vector2.up * flarpStrength;
		}
		if ((Input.GetKeyUp(KeyCode.X) || Input.GetKey(KeyCode.Escape)) && !BirdIsAlive)
		{
			Application.Quit();
		}
		logic.IsDead();
	}

	public void OnCollisionEnter2D()
	{
		
		if (manager.inQuiz)
		{
            BirdIsAlive = true;
			Debug.Log("Coollided in Quiz");
		}
		else
		{
			logic.GameOver();
            BirdIsAlive = false;
        }
		
	}


}
