using UnityEngine;
using UnityEditor.Scripting.Python;

public class PythonTestRun : MonoBehaviour
{
    void Start()
    {
        // Define the Python script as a string
        string pythonScript = @"
import UnityEngine

# Access the PlayerController component
player = UnityEngine.GameObject.Find(""Player"").GetComponent(""PlayerCustomController"")

# Simulate inputs
player.MoveLeft()
player.Jump()
UnityEngine.Time.deltaTime  # Simulate time passing
player.StopMoving()
player.Jump()
";

        // Run the Python script
        RunPythonScript(pythonScript);
    }

    void RunPythonScript(string script)
    {
        try
        {
            PythonRunner.RunString(script);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error running Python script: " + e.Message);
        }
    }
}