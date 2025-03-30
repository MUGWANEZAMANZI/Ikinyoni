using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // If using UI elements for questions

public class MarsQuizManager : MonoBehaviour
{
    public LogicManagerScript logicManager; // Reference to your existing LogicManager
    public GameObject quizPanel; // UI panel to display question
    public Text questionText; // UI text for the question
    public Button[] answerButtons; // Buttons for multiple-choice answers

    private int lastQuizScore = 0; // Track when to trigger the next quiz
    private List<Question> questions = new List<Question>();
    private Question currentQuestion;
    public bool inQuiz = false;


    public BirdScript birdScript; // Reference to BirdScript
    public PipeMoveScript pipeMoveScripts; // Array of PipeMoveScripts
    public PipeSpawnerScript pipeSpawnScript;

    void Start()
    {
        lastQuizScore = logicManager.playerScore; // Initialize score tracking
        InitializeQuestions();

    }

    void Update()
    {
        if (logicManager.playerScore >= lastQuizScore + 5) // Every +10 score
        {
            lastQuizScore = logicManager.playerScore; // Update last checkpoint
            AskQuestion();
        }
    }

    void InitializeQuestions()
    {
        questions.Add(new Question("What is the largest volcano on Mars?", new string[] { "Olympus Mons", "Vesuvius", "Mauna Loa", "Everest" }, 0));
        questions.Add(new Question("How long is a day on Mars?", new string[] { "24 hours", "12 hours", "25 hours", "26 hours" }, 2));
        questions.Add(new Question("What gives Mars its red color?", new string[] { "Iron oxide", "Copper dust", "Sulfur clouds", "Lava" }, 0));
        questions.Add(new Question("Does Mars have an atmosphere?", new string[] { "Yes, but very thin", "No", "Same as Earth", "Only at poles" }, 0));

        //Mars Colonization
        questions.Add(new Question("What is the average temperature on Mars?", new string[] { "-63°C", "-20°C", "0°C", "30°C" }, 0));
        questions.Add(new Question("Which company is actively working on colonizing Mars?", new string[] { "SpaceX", "Blue Origin", "NASA", "Boeing" }, 0));
        questions.Add(new Question("Which spacecraft is being developed by SpaceX for Mars travel?", new string[] { "Starship", "Falcon Heavy", "Orion", "Dragon" }, 0));
        questions.Add(new Question("What is the main component of the Martian atmosphere?", new string[] { "Carbon dioxide", "Oxygen", "Nitrogen", "Hydrogen" }, 0));
        questions.Add(new Question("How long does it take to travel to Mars with current technology?", new string[] { "6-9 months", "3-4 years", "1-2 months", "1 year" }, 0));
        questions.Add(new Question("What is one major health risk for astronauts traveling to Mars?", new string[] { "Radiation exposure", "Low oxygen", "Low temperatures", "Micrometeorites" }, 0));
        questions.Add(new Question("What is a potential solution for producing oxygen on Mars?", new string[] { "MOXIE", "Hydroponics", "Solar panels", "Reacting with iron" }, 0));
        questions.Add(new Question("Which of these challenges must be overcome to colonize Mars?", new string[] { "Radiation", "Lack of food", "Extreme cold", "All of the above" }, 3));
        questions.Add(new Question("How can water be obtained on Mars?", new string[] { "Extracting from ice", "Drilling deep wells", "Shipping from Earth", "Creating through chemistry" }, 0));
        questions.Add(new Question("Which of these is a proposed method to create a livable atmosphere on Mars?", new string[] { "Terraforming", "Geoengineering", "Magnetosphere creation", "Underground domes" }, 0));
        questions.Add(new Question("What type of power source is most practical for a Mars colony?", new string[] { "Nuclear", "Solar", "Wind", "Fossil fuels" }, 0));
        questions.Add(new Question("What is a major psychological challenge of living on Mars?", new string[] { "Isolation", "Lack of entertainment", "No seasons", "Excessive noise" }, 0));

        //Plaing tress to mars
        questions.Add(new Question("What is the biggest challenge in growing plants on Mars?", new string[] { "Lack of liquid water", "Too much sunlight", "Too much oxygen", "Excessive heat" }, 0));
        questions.Add(new Question("What kind of soil exists on Mars?", new string[] { "Regolith", "Loam", "Clay", "Silt" }, 0));
        questions.Add(new Question("How can trees survive on Mars?", new string[] { "Artificial greenhouses", "Direct exposure", "Planting in ice", "Growing underwater" }, 0));
        questions.Add(new Question("What gas do trees produce that is essential for human survival?", new string[] { "Oxygen", "Carbon dioxide", "Nitrogen", "Methane" }, 0));
        questions.Add(new Question("What technology could help grow plants on Mars?", new string[] { "Hydroponics", "Nuclear reactors", "Solar panels", "Terraforming lasers" }, 0));
        questions.Add(new Question("Why is Mars' atmosphere unsuitable for plants?", new string[] { "Too much CO2 and low pressure", "Too much oxygen", "Too much nitrogen", "It has no atmosphere" }, 0));
        questions.Add(new Question("Which element found in Martian soil is toxic to plants?", new string[] { "Perchlorates", "Phosphorus", "Silicon", "Nitrogen" }, 0));
        questions.Add(new Question("How could trees help create a sustainable environment on Mars?", new string[] { "Generating oxygen and stabilizing soil", "Providing shelter", "Producing methane", "Blocking radiation" }, 0));
        questions.Add(new Question("What is the best way to simulate Earth-like conditions for plant growth on Mars?", new string[] { "Controlled greenhouses", "Open-air plantations", "Directly planting in Martian soil", "Using solar mirrors" }, 0));
        questions.Add(new Question("Which type of plant is most likely to survive in Martian conditions?", new string[] { "Algae and mosses", "Oak trees", "Cacti", "Apple trees" }, 0));

    }

    void AskQuestion()
    {
        if (questions.Count == 0) return;

        inQuiz = true;
        currentQuestion = questions[Random.Range(0, questions.Count)];
        quizPanel.SetActive(true);
        questionText.text = currentQuestion.text;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.options[i];
            int index = i;
            answerButtons[i].onClick.RemoveAllListeners(); // Clear previous listeners
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }


        if (birdScript != null) birdScript.enabled = false;
        if (pipeMoveScripts != null) pipeMoveScripts.enabled = false;
        if (pipeSpawnScript != null) pipeSpawnScript.enabled = false;

        // Set bird to kinematic
        if (birdScript != null && birdScript.rigidbody != null)
        {
            birdScript.rigidbody.bodyType = RigidbodyType2D.Kinematic;
            birdScript.rigidbody.velocity = Vector2.zero;
            birdScript.rigidbody.angularVelocity = 0f;
            birdScript.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        UpdatePipeMoveSpeeds();
    }



    void CheckAnswer(int selectedIndex)
    {
        if (selectedIndex == currentQuestion.correctIndex)
        {
            logicManager.addScore(10); // Add 10 bonus points for correct answer
        }
        quizPanel.SetActive(false); // Hide question UI

        //Enable scrpts again
        inQuiz = false;
        if (birdScript != null) birdScript.enabled = true;
        if (pipeMoveScripts != null) pipeMoveScripts.enabled = true;
        if (pipeSpawnScript != null) pipeSpawnScript.enabled = true;

        if (birdScript != null && birdScript.rigidbody != null)
        {
            birdScript.rigidbody.constraints = RigidbodyConstraints2D.None;
        }
        UpdatePipeMoveSpeeds();
    }


    void UpdatePipeMoveSpeeds()
    {
        PipeMoveScript[] pipeMoveScripts = FindObjectsOfType<PipeMoveScript>();
        foreach (PipeMoveScript pipeMoveScript in pipeMoveScripts)
        {
            if (inQuiz)
            {
                pipeMoveScript.moveSpeed = 0f;
            }
            else
            {
                pipeMoveScript.moveSpeed = pipeMoveScript.originalMoveSpeed; // Restore original move speed
            }
        }
    }

 
}


[System.Serializable]
public class Question
{
    public string text;
    public string[] options;
    public int correctIndex;

    public Question(string text, string[] options, int correctIndex)
    {
        this.text = text;
        this.options = options;
        this.correctIndex = correctIndex;
    }
}
