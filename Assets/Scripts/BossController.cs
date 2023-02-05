using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

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
    GameObject sideTentaclePrefab;
    [SerializeField]    
    Transform[] spawns;
    [SerializeField]
    float sideTentacleSpeed;
    [SerializeField]
    float movementTime;
    [SerializeField]
    float idleTime;
    [Space]
    [Header("Tentacles Attack")]
    [SerializeField] int tentaclesDefeated;
    public int TentaclesDefeated {
        get { return tentaclesDefeated; }
        set {
            tentaclesDefeated = value;
            if (tentaclesDefeated >= tentaclesNeeded) {
                if (tentacleCoroutine != null)
                    StartCoroutine(LastTentacleCoroutine());
                    FindObjectOfType<CameraScript>().StopRumbling();
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
    [Space]
    [Header("Dolphin Attack")]
    [SerializeField]
    GameObject dolphinSidePrefab;
    [SerializeField]
    GameObject dolphinFrontPrefab;
    [SerializeField]
    int regions = 4;
    [SerializeField]
    float yStartPosition;
    [SerializeField]
    float maxFrontXPosition;
    [SerializeField]
    float xOffset;
    [SerializeField]
    int maxDolphinsPerRegion = 4;
    [SerializeField]
    float dolphinCooldown = 0.8f;
    float leftLimit;
    float rightLimit;

    private void Start() {
        DivideRegions();
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
    void SideTentaclesAttack() {
        StartCoroutine(SideTentaclesCoroutine());
    }
    IEnumerator SideTentaclesCoroutine() {
        bool up = Random.Range(0,2) > 0;
        int firstSide = Random.Range(0,2);
        int secondSide = Random.Range(0,2);

        int direction = firstSide == 0 ? 1 : -1;
        GameObject first = Instantiate(sideTentaclePrefab,spawns[2 * (up ? 1 : 0) + firstSide].position, Quaternion.Euler(0,0, -direction * 90));
        first.GetComponent<SideTentacle>().direction = direction;
        first.GetComponent<SideTentacle>().speed = sideTentacleSpeed;
        first.GetComponent<SideTentacle>().movementTime = movementTime;
        first.GetComponent<SideTentacle>().idleTime = idleTime;

        yield return new WaitForSeconds(2);
        direction = secondSide == 0 ? 1 : -1;
        GameObject second = Instantiate(sideTentaclePrefab,spawns[2 * (!up ? 1 : 0) + secondSide].position, Quaternion.Euler(0,0,-direction * 90));
        second.GetComponent<SideTentacle>().direction = direction;
        second.GetComponent<SideTentacle>().speed = sideTentacleSpeed;
        second.GetComponent<SideTentacle>().movementTime = movementTime;
        second.GetComponent<SideTentacle>().idleTime = idleTime;
    }

    [ContextMenu("TentaclesAttack")]
    void TentaclesAttack() {
        tentacleCoroutine = StartCoroutine(TentaclesAttackCoroutine());
    }

    IEnumerator TentaclesAttackCoroutine() {
        FindObjectOfType<CameraScript>().StartRumbling();
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
        FindObjectOfType<CameraScript>().StopRumbling();
    }

    IEnumerator LastTentacleCoroutine() {
        Time.timeScale = 0.5f;
        FindObjectOfType<CameraScript>().ZoomIn();
        yield return new WaitForSeconds(1);
        Time.timeScale = 1;
        FindObjectOfType<CameraScript>().ZoomOut();
    }

    void DivideRegions(){
        Camera cam = Camera.main;
        float camXOffset = cam.transform.position.x;
        Vector3 screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
        rightLimit = screenBounds.x + xOffset;
        leftLimit = (screenBounds.x*-1)+(camXOffset*2) - xOffset;
    }

    [ContextMenu("DolphinAttack")]
    void DolphinAttack(){
        float currentLeft = leftLimit;
        float partition_size = (Mathf.Abs(leftLimit)+Mathf.Abs(rightLimit))/regions;
        float currentRight = currentLeft+partition_size;
        Vector3 position;
        Transform dolphin;
        for (int i=0; i<regions; i++){
            position = new Vector3(Random.Range(currentLeft, currentRight), yStartPosition, 0f);
            dolphin = createDolphin(position, null);
            // Change order in layer
            dolphin.GetComponent<SortingGroup>().sortingOrder = i < regions/2 ? i : regions-i;
            StartCoroutine(DolphinOut(0, dolphin));
            currentLeft = currentRight;
            currentRight = currentLeft+partition_size;
        }
    }

    Transform createDolphin(Vector3 position, Transform parent){
        GameObject dolphinPrefab = Mathf.Abs(position.x) > maxFrontXPosition ? dolphinSidePrefab : dolphinFrontPrefab;
        Transform child = Instantiate(dolphinPrefab, position, Quaternion.identity, parent).transform;
        if (parent != null){
            child.GetComponent<SortingGroup>().sortingOrder = parent.GetComponent<SortingGroup>().sortingOrder;
        }
        if (position.x > 0){
            child.GetComponent<SpriteRenderer>().flipX = true;
        }
        return child;
    }

    IEnumerator DolphinOut(int recursion, Transform parent) {
        if (recursion >= maxDolphinsPerRegion) {
            yield return new WaitForSeconds(2);
            StartCoroutine(DolphinBack(parent));
            yield break;
        }
        yield return new WaitForSeconds(dolphinCooldown);
        Vector3 position = parent.transform.position;
        float distance = parent.GetComponent<SpriteRenderer>().bounds.size.x/1.5f;
        Vector3 director = parent.position.x >= 0 ? new Vector3(1,-1,0) : new Vector3(-1,-1,0);
        position += Quaternion.Euler(0, 45, 0) * director * distance;
        Transform child = createDolphin(position, parent);
        StartCoroutine(DolphinOut(recursion + 1, child));
    }

    IEnumerator DolphinBack(Transform branch) {
        branch.GetComponent<Animator>().Play("In");
        yield return new WaitForSeconds(dolphinCooldown);
        Destroy(branch.gameObject);

        Transform parent = branch.parent;
        if (!parent){
            yield break;
        }
        StartCoroutine(DolphinBack(parent));
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
            pivot.Rotate(new Vector3(0,0,rotationSpeed * rotationDirection * Time.deltaTime));
        }
    }
}
