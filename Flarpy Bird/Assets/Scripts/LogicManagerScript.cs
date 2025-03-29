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


    [ContextMenu("Increase Score")]
	public void addScore(int scoreToAdd)
	{
		playerScore += scoreToAdd;
        if (scoreText != null)  // Prevents null errors
        {
            scoreText.text = playerScore.ToString();
        }
        else
        {
            Debug.LogError("ScoreText is not assigned in the Inspector!");
        }
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
		gameOverScreen.SetActive(value: true);
	}
}
