using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeSceneTransition : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Fade == null)
        {
            DontDestroyOnLoad(transform.gameObject);
            GameManager.Fade = this;
        }
        else
            Destroy(this.gameObject);
    }
    public void FadeStart(string scene, float duration = 1)
    {
        StartCoroutine(this.LoadAsyncScene(scene, duration));
    }
    public IEnumerator LoadAsyncScene(string scene, float duration)
    {
        GetComponent<Animator>().SetBool("Fade", false);
        yield return new WaitForSeconds(duration);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneUtility.GetBuildIndexByScenePath(scene), LoadSceneMode.Single);
        while (!asyncLoad.isDone)
            yield return null;

        GetComponent<Animator>().SetBool("Fade", true);
    }
    public void FadeStart(int scene, float duration = 1)
    {
        StartCoroutine(this.LoadAsyncScene(scene, duration));
    }
    public IEnumerator LoadAsyncScene(int scene, float duration)
    {
        GetComponent<Animator>().SetBool("Fade", false);
        yield return new WaitForSeconds(duration);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        while (!asyncLoad.isDone)
            yield return null;

        GetComponent<Animator>().SetBool("Fade", true);
    }
}
