using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    private GameManager gameManager;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("LevelExit");
            //Reset Scene and check scores
            //SceneManager.LoadScene(1);

        }
    }

    IEnumerator LevelExit()
    {
        yield return new WaitForSeconds(0.1f);

        //UIManager.instance.fadeToBlack = true;
        GameManager.instance.LevelComplete();

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }
}
