using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField]
    GameObject rootPrefab;
    [SerializeField]
    bool finalPhase;
    [Header("Root Expansion Attack")]
    [Header("Division")]
    [SerializeField]
    int maxDivides = 5;
    [SerializeField]
    float divideCooldown = 0.4f;
    [Header("Rotation")]
    [SerializeField]
    Transform pivot;
    [SerializeField]
    float rotationSpeed = 0.2f;
    [SerializeField]
    int rotationDuration = 10;
    float rotationDirection = 1;
    bool rotate;
    [Space]
    [Header("Side Tentacles Attack")]
    [SerializeField]
    GameObject[] spawns;
    [Space]
    [Header("Tentacles Attack")]
    [SerializeField] int tentaclesDefeated;

    public int TentaclesDefeated {
        get { return tentaclesDefeated; }
        set {
            tentaclesDefeated = value;
            if (tentaclesDefeated >= tentaclesNeeded) {
                if (tentacleCoroutine != null) 
                    StopCoroutine(tentacleCoroutine);
                // StartCoroutine(HideTentacles)
            }
        }
    }
    [SerializeField] int tentaclesNeeded;
    public int tentaclesHealth;
    [SerializeField] int tentacleRounds;
    [SerializeField] int tentacleSpawnDelay;
    [SerializeField] Transform[] tentacles;
    [SerializeField] Transform bottomLeft;
    [SerializeField] Transform topRight;
    Coroutine tentacleCoroutine;

    private void Start() {

    }

    [ContextMenu("ExpanseAttack")]
    void ExpanseAttack() {
        Transform baseRoot1 = Instantiate(rootPrefab, pivot.transform.position, Quaternion.Euler(0,0,180 - Random.Range(20, 45)), pivot).transform;
        StartCoroutine(Expand(0, baseRoot1));
        Transform baseRoot2 = Instantiate(rootPrefab, pivot.transform.position, Quaternion.Euler(0,0,180 + Random.Range(20, 45)), pivot).transform;
        StartCoroutine(Expand(0, baseRoot2));
    }

    IEnumerator Expand(int recursion, Transform father) {
        if (recursion == maxDivides) {
            yield return new WaitForSeconds(2);
            if(finalPhase){
                rotate = true;
                yield return new WaitForSeconds(rotationDuration);
            }
            StartCoroutine(Contract(father));
            yield break;
        }
        yield return new WaitForSeconds(divideCooldown);
        float angle = father.eulerAngles.z;
        Vector3 position = father.GetChild(0).transform.position;
        
        if ( Random.Range(0f,1f) > 0.4f) {
            Transform child1 = Instantiate(rootPrefab, position, Quaternion.Euler(0,0,angle - Random.Range(20, 45)), father).transform;
            StartCoroutine(Expand(recursion + 1, child1));
            Transform child2 = Instantiate(rootPrefab, position, Quaternion.Euler(0,0,angle + Random.Range(20, 45)), father).transform;
            StartCoroutine(Expand(recursion + 1, child2));
        } else {
            Transform child = Instantiate(rootPrefab, position, Quaternion.Euler(0,0,angle + Random.Range(-30,30)), father).transform;
            StartCoroutine(Expand(recursion + 1, child));
        }
    }

    IEnumerator Contract(Transform branch) {
        Transform parent = branch.parent;
        branch.GetComponent<Animator>().Play("RootContraction");
        yield return new WaitForSeconds(divideCooldown);
        Destroy(branch.gameObject);
        if (parent.name == "ExpansionPivot") {
            rotate = false;
            pivot.Rotate(Vector3.zero);
            yield break;
        } else {
           StartCoroutine(Contract(parent));
        }
    }

    [ContextMenu("SideTentaclesAttack")]
    // IEnumerator SideTentaclesAttack() {
    //     if (Random.Range(0,2) > 0) {
    //         if (Random.Range(0,2) > 0) {
    //             Instantiate
    //         } else {

    //         }
    //     } else {
    //         if (Random.Range(0,2) > 0) {
                
    //         } else {

    //         }
    //     }
    // }

    [ContextMenu("TentaclesAttack")]
    void TentaclesAttack() {
        tentacleCoroutine = StartCoroutine(TentaclesAttackCoroutine());
    }

    IEnumerator TentaclesAttackCoroutine() {
        float tentDuration = tentacles[0].GetComponent<Root>().idleDuration;

        for(int i = 0; i < tentacleRounds; i++) {
            for (int j = 0; j < tentacles.Length; j++) {
                Transform tent = tentacles[j];
                tent.position = new Vector3(Random.Range(bottomLeft.position.x, topRight.position.x), Random.Range(bottomLeft.position.y, topRight.position.y), 0);
                tent.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(tentDuration + tentacleSpawnDelay);
        }

        // If not killed, set health back to full
        foreach(Transform tent in tentacles) {
            tent.GetComponent<Root>().health = tentaclesHealth;
        }
        TentaclesDefeated = 0;
    }

    private void Update() {
        if (rotate) {
            float angle = pivot.eulerAngles.z;
            angle = (angle > 180) ? angle - 360 : angle;
            if (angle > 25) {
                rotationDirection = -1;
            } else if (angle < -25) {
                rotationDirection = 1;
            }
            pivot.Rotate(new Vector3(0,0,rotationSpeed * rotationDirection));
        }
    }
}
