using UnityEngine;

public class PipeMoveScript : MonoBehaviour
{
	public float moveSpeed = 5f;

	public float deadZone = -34.6f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.position = base.transform.position + Vector3.left * moveSpeed * Time.deltaTime;
		if (base.transform.position.x < deadZone)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
