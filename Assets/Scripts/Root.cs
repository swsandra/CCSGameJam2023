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

    [Header("Damage")]
    [SerializeField] float flashDuration = .09f;
    [SerializeField] Material flashMaterial;
    Material originalMaterial;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        originalMaterial = sr.material;
    }

    public void Damage() {
        health -= 1;
        if (health <= 0) {
            StopCoroutine(HideRoutine());
            GetComponent<Animator>().enabled = false;
            StartCoroutine(DieRoutine());
        }
        else {
            StartCoroutine(damageRoutine());
            // TODO: Feedback
        }
    }

    IEnumerator damageRoutine() {
        sr.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMaterial;
    }

    IEnumerator DieRoutine() {
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

    IEnumerator HideRoutine() {
        yield return new WaitForSeconds(idleDuration);
        anim.SetTrigger("Hide");
        yield return new WaitForSeconds(.5f);
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        StartCoroutine(HideRoutine());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }
}
