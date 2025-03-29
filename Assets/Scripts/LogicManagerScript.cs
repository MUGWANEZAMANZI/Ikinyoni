using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;	

public class LogicManagerScript : MonoBehaviour
{
	public GameObject gameOverScreen;

	public int playerScore;

	public GameObject bird;

    public TextMeshProUGUI scoreText;
    public Text highScoreText;
	//public bool dead = false;




    //Load high score
    void Start()
    {
        LoadHighScore();
    }


    [ContextMenu("Increase Score")]
	public void addScore(int scoreToAdd)
	{
		playerScore += scoreToAdd;
		//Debug.Log("Collision inside score to add");
        if (scoreText != null)  // Prevents null errors
        {
            scoreText.text = playerScore.ToString();
        }
        else
        {
            Debug.LogError("ScoreText is not assigned in the Inspector!");
        }
    }


    public void CheckHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (playerScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", playerScore);
            PlayerPrefs.Save();
            highScoreText.text = "High Score: " + playerScore;
            Debug.Log(highScoreText.text);
        }
    }

    public void LoadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;
        Debug.Log(highScore);
    }

    public void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void IsDead()
	{
		if (bird.transform.position.y < 0f || bird.transform.position.y > 300f)
		{
			GameOver();
			//dead = true;
		}
	}



    public void OpenGitHub()
    {
        Application.OpenURL("https://github.com/MUGWANEZAMANZI"); // Replace with your GitHub URL
    }

    public void OpenEmail()
    {
        Application.OpenURL("mailto:mmaudace@gmailcom"); // Replace with your LinkedIn URL
    }



public void GameOver()
	{
        CheckHighScore();
        gameOverScreen.SetActive(value: true);
		//dead = true;
	}
	void Update()
	{
		//Debug.Log(scoreText.text);
	}
	
}
