using UnityEngine;

public class PipeMoveScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float deadZone = -34.6f;
    public MarsQuizManager manager;

    public float originalMoveSpeed; // Store the original moveSpeed

    private void Start()
    {
        originalMoveSpeed = moveSpeed; // Capture the original moveSpeed
    }

    private void Update()
    {
       if(manager != null && manager.inQuiz)
        {
            return;
        }

        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }
}