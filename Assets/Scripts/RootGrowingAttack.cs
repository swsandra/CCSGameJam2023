using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootGrowingAttack : MonoBehaviour
{
    [SerializeField]
    GameObject rootPrefab;
    [SerializeField]
    bool finalPhase;
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

    private void Start() {
        Attack();
    }

    [ContextMenu("atacar")]
    void Attack() {
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

    IEnumerator Contract(Transform branch){
        Transform parent = branch.parent;
        branch.GetComponent<Animator>().Play("RootContraction");
        yield return new WaitForSeconds(divideCooldown);
        Destroy(branch.gameObject);
        if (parent.name == "Pivot") {
            rotate = false;
            pivot.Rotate(Vector3.zero);
            yield break;
        }
        if (parent) {
           StartCoroutine(Contract(parent));
        }
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
