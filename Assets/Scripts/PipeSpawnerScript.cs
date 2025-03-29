using UnityEngine;

public class PipeSpawnerScript : MonoBehaviour
{
	public GameObject pipe;
	public MarsQuizManager manager;
	//public LogicManagerScript logic;

	public float spawnRate = 10f;

	public float timer = 0f;

	public float heightOffset = 10f;

	private void Start()
	{
		SpawnPipe();
	}

	private void Update()
	{
        if (!manager.inQuiz)
        {
            if (timer < spawnRate)
            {
                timer += Time.deltaTime;
                return;
            }
            //if (!logic.dead)
            //{

            SpawnPipe();

            //}
            timer = 0f;
        }
       
	}

	private void SpawnPipe()
	{
		float minInclusive = base.transform.position.y - heightOffset;
		float maxInclusive = base.transform.position.y + heightOffset;
		Object.Instantiate(pipe, new Vector3(base.transform.position.x, Random.Range(minInclusive, maxInclusive), 0f), base.transform.rotation);
        
		
	}
}
