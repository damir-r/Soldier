using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public TextMeshProUGUI ClickToStartText;
    public TextMeshProUGUI LeftPage;
    public TextMeshProUGUI RightPage;
    private Animator animator;
    private bool IsBookOpened = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ClickToStartText == null || LeftPage == null || RightPage == null)
            throw new System.Exception("Define all properties");
        animator = gameObject.GetComponent<Animator>();
        if (animator == null)
            throw new System.Exception("Can't locate Animator on current gameObject");
        animator.enabled = false;
    }

    public void BookClicked()
    {
        if (!IsBookOpened)
        {
            animator.enabled = true;
            StartCoroutine(FadeOutText(ClickToStartText));
        } else
        {
            SceneManager.LoadScene("Battle");
        }
    }

    public void BookOpened()
    {
        IsBookOpened = true;
        animator.enabled = false;
        Debug.Log("Reached animation event");
        ClickToStartText.text = "Нажмите чтобы пойти в бой";
        LeftPage.text = "ГЛАВА";
        RightPage.text = "ПЕРВАЯ";
        StartCoroutine(FadeInText(ClickToStartText));
        StartCoroutine(FadeInText(LeftPage));
        StartCoroutine(FadeInText(RightPage));
    }

    private IEnumerator FadeOutText(TextMeshProUGUI text)
    {
        Color color = text.color;
        float opacity = color.a;
        while (opacity > 0)
        {
            opacity -= .1f;
            text.color = new Color(color.r, color.g, color.b, opacity);
            yield return new WaitForSeconds(.05f);
        }
        text.color = new Color(color.r, color.g, color.b, 0);
        yield return null;
    }

    private IEnumerator FadeInText(TextMeshProUGUI text)
    {
        Color color = text.color;
        float opacity = color.a;
        while (opacity < 1)
        {
            opacity += .1f;
            text.color = new Color(color.r, color.g, color.b, opacity);
            yield return new WaitForSeconds(.05f);
        }
        text.color = new Color(color.r, color.g, color.b, 1);
        yield return null;
    }
}
