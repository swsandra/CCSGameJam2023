using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    public int health = 3;
    public float idleDuration = 4f;
    BossController boss;
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
        boss = GameObject.FindObjectOfType<BossController>();
    }

    public void Damage() {
        health -= 1;
        if (health == 0) {
            StopAllCoroutines();
            GetComponent<Animator>().enabled = false;
            StartCoroutine(DieRoutine());
        }
        else if (health < 0) {
            return;
        }
        else {
            StartCoroutine(damageRoutine());
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
        boss.TentaclesDefeated += 1;
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
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
        if (health <= 0) {
            health = boss.tentaclesHealth;
        }
        sr.material.color = Color.white;
        sr.material = originalMaterial;
        StopAllCoroutines();
    }
}
