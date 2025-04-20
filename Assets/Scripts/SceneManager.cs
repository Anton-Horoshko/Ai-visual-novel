using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public Animator transition;
    public float TransitionTime = 1f;

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(Transition(sceneIndex));
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator Transition(int sceneIndex)
    {
        transition.SetTrigger("Play");

        yield return new WaitForSeconds(TransitionTime);

        SceneManager.LoadScene(sceneIndex);
    }
}
