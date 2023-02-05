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
    [Header("Sounds")]
    [SerializeField] AudioClip damage;
    [SerializeField] AudioClip show;
    [SerializeField] AudioClip die;

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
        health -= 1;
        if (health < 0) {
            return;
        }
        health = Mathf.Clamp(health, 0, boss.tentaclesHealth);
        healthBar.localScale = new Vector3((float)health / boss.tentaclesHealth, healthBar.localScale.y, healthBar.localScale.z);
        if (health <= 0) {
            StopAllCoroutines();
            GetComponent<Animator>().enabled = false;
            StartCoroutine(DieRoutine());
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
        AudioSource.PlayClipAtPoint(damage, transform.position);
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMaterial;
    }

    IEnumerator DieRoutine() {
        AudioSource.PlayClipAtPoint(damage, transform.position);
        AudioSource.PlayClipAtPoint(die, transform.position);
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

    public void ShowSound() {
        AudioSource.PlayClipAtPoint(show, transform.position);
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
