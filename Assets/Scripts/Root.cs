using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Root : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    Collider2D col;
    public int health = 3;
    public float idleDuration = 4f;
    BossController boss;
    [SerializeField] float deathDuration;
    [SerializeField] RectTransform healthBar;

    [Header("Damage")]
    [SerializeField] float flashDuration = .09f;
    [SerializeField] Material flashMaterial;
    Material originalMaterial;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        originalMaterial = sr.material;
        boss = GameObject.FindObjectOfType<BossController>();
    }

    public void Damage() {
        health = Mathf.Clamp(health-1, 0, boss.tentaclesHealth);
        healthBar.localScale = new Vector3((float)health / boss.tentaclesHealth, healthBar.localScale.y, healthBar.localScale.z);
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

    public void DisableHealthBar() {
        healthBar.parent.gameObject.SetActive(false);
    }

    public void EnableHealthBar() {
        healthBar.parent.gameObject.SetActive(true);
    }

    IEnumerator damageRoutine() {
        sr.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMaterial;
    }

    IEnumerator DieRoutine() {
        boss.TentaclesDefeated += 1;
        col.enabled = false;
        float t = 0f;
        while(t < 1) {
            sr.material.color = Color.Lerp(Color.white, Color.black, t);
            if (t < 1){ 
                t += Time.deltaTime/deathDuration;
            }
            yield return new WaitForEndOfFrame();
        }
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
        if (col != null)
            col.enabled = false;
        StartCoroutine(HideRoutine());
    }

    private void OnDisable() {
        if (health <= 0) {
            health = boss.tentaclesHealth;
            healthBar.localScale = new Vector3(1, healthBar.localScale.y, healthBar.localScale.z);
        }
        sr.material.color = Color.white;
        sr.material = originalMaterial;
        GetComponent<Animator>().enabled = true;
        StopAllCoroutines();
    }
}
