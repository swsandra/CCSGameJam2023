using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    [SerializeField] int health = 1;
    [SerializeField] float idleDuration = 3f;
    [SerializeField] float deathDuration;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Damage() {
        health -= 1;
        if (health <= 0) {
            StopCoroutine(Hide());
            GetComponent<Animator>().enabled = false;
            StartCoroutine(Die());
            return;
        }
        // TODO: Feedback
    }

    IEnumerator Die() {
        float t = 0f;
        while(t < 1) {
            sr.material.color = Color.Lerp(Color.white, Color.black, t);
            if (t < 1){ 
                t += Time.deltaTime/deathDuration;
            }
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    IEnumerator Hide() {
        yield return new WaitForSeconds(idleDuration);
        anim.SetTrigger("Hide");
        yield return new WaitForSeconds(.5f);
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        StartCoroutine(Hide());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }
}
