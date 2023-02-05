using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

public class BossController : MonoBehaviour
{
    [SerializeField]
    GameObject rootPrefab;
    SpriteRenderer sr;
    [SerializeField]
    int phase;
    [Space]
    [Header("Sounds")]
    [SerializeField]
    AudioSource earthquakeSource;
    [SerializeField]
    AudioSource rootRising;
    [SerializeField]
    AudioClip show;
    [SerializeField]
    AudioSource lastTentacle;
    [Space]
    [Header("Damage")]
    [SerializeField] RectTransform healthBar;
    public int maxHealth = 15;
    [SerializeField]
    int health;
    [SerializeField]
    bool invulnerable;
    [SerializeField]
    float flashDuration = .09f;
    [SerializeField]
    Material flashMaterial;
    Material originalMaterial;
    int healthPerPhase = 15;
    [Header("DamagePhase")]
    [SerializeField] Transform[] tentacleBarrier;
    [SerializeField] Transform pusher;
    [SerializeField] Vector3 pusherFinalPosition;
    [SerializeField] float pusherDuration;
    [SerializeField] float damagePhaseDuration;
    [Space]
    [Header("Attack Parameters")]
    [SerializeField]
    bool canAttack;
    public bool CanAttack {
        get { return canAttack; }
        set {
            canAttack = value;
        }
    }
    [SerializeField]
    float attackCooldown;
    [SerializeField]
    int attacksBeforeTentacles;
    int attacksCount;
    Coroutine attackRoutine;
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
                if (tentacleCoroutine != null) {
                    StartCoroutine(LastTentacleCoroutine());
                    StopCoroutine(tentacleCoroutine);
                }
                if (attackRoutine != null) {
                    StopCoroutine(attackRoutine);
                    attackRoutine = null;
                    canAttack = false;
                }
                StartCoroutine(EnterDamagePhase());
                tentaclesDefeated = 0;
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
    [Space]
    [Header("Faces")]
    [SerializeField]
    Sprite weakFaceSprite;
    [SerializeField]
    Sprite angryFaceSprite;
    [SerializeField]
    Sprite happyFaceSprite;
    [SerializeField]
    Sprite deadFaceSprite;
    [SerializeField]
    Transform faceFinalPosition;
    [SerializeField]
    GameObject face;
    [SerializeField]
    float moveDuration;
    float topBound;
    SpriteRenderer faceSpriteRenderer;
    [Space]
    [Header("Lake")]
    [SerializeField]
    GameObject MidWater;
    [SerializeField]
    GameObject NoWater;
    [SerializeField]
    float lakeDuration;

    private void Start() {
        phase = 1;
        health = maxHealth;
        healthPerPhase = maxHealth/3;
        invulnerable = true;
        DivideRegions();
        faceSpriteRenderer = face.GetComponent<SpriteRenderer>();
        face.transform.position = faceFinalPosition.position + (Vector3.up * topBound);
        sr = GetComponent<SpriteRenderer>();
        originalMaterial = sr.material;
        canAttack = true;
        attacksCount = 0;
        attackRoutine = null;
    }

    IEnumerator Attack(){
        yield return new WaitForSeconds(attackCooldown);
        Debug.Log("Attack cooldown finished");

        if (attacksCount < attacksBeforeTentacles){
            Debug.Log("Selecting random attack");
            float rand = Random.Range(0f, 1f);
            if (rand <= .33f && phase != 1){
                ExpanseAttack();
            }else if ((rand <= .66f && phase != 1) || (rand <= .5f && phase == 1)){
                SideTentaclesAttack();
            }else {
                DolphinAttack();
            }
            attacksCount += 1;
        }else{
            Debug.Log("Selecting tentacle attack");
            TentaclesAttack();
            attacksCount = 0;
        }

        CanAttack = false;
        attackRoutine = null;
    }

    [ContextMenu("ExpanseAttack")]
    void ExpanseAttack() {
        rootRising.Play();
        Transform baseRoot1 = Instantiate(rootPrefab, pivot.transform.position, Quaternion.Euler(0,0,180 - Random.Range(20, 45)), pivot).transform;
        StartCoroutine(Expand(0, baseRoot1));
        Transform baseRoot2 = Instantiate(rootPrefab, pivot.transform.position, Quaternion.Euler(0,0,180 + Random.Range(20, 45)), pivot).transform;
        StartCoroutine(Expand(0, baseRoot2));
    }

    IEnumerator Expand(int recursion, Transform father) {
        if (recursion == maxDivides) {
            yield return new WaitForSeconds(0.3f);
            rootRising.Stop();
            yield return new WaitForSeconds(1.7f);
            if(phase == 3){
                rotate = true;
                rootRising.Play();
                yield return new WaitForSeconds(rotationDuration);
                rootRising.Stop();
            }
            rootRising.Play();
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
            rootRising.Stop();
            rotate = false;
            pivot.Rotate(Vector3.zero);
            canAttack = true;
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
        earthquakeSource.Play();
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
            tent.GetComponent<Root>().healthBar.localScale = new Vector3(1,1,1);
        }
        TentaclesDefeated = 0;
        FindObjectOfType<CameraScript>().StopRumbling();
        earthquakeSource.Stop();
        canAttack = true;
    }

    IEnumerator LastTentacleCoroutine() {
        Time.timeScale = 0.5f;
        FindObjectOfType<CameraScript>().ZoomIn();
        lastTentacle.Play();
        yield return new WaitForSeconds(1);
        Time.timeScale = 1;
        FindObjectOfType<CameraScript>().ZoomOut();
        earthquakeSource.Stop();
        FindObjectOfType<CameraScript>().StopRumbling();
    }

    void DivideRegions(){
        Camera cam = Camera.main;
        float camXOffset = cam.transform.position.x;
        float camYOffset = cam.transform.position.y;
        Vector3 screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
        rightLimit = screenBounds.x - xOffset;
        leftLimit = (screenBounds.x*-1)+(camXOffset*2) + xOffset;
        topBound = transform.GetComponent<SpriteRenderer>().bounds.size.y/4;
    }

    [ContextMenu("DolphinAttack")]
    void DolphinAttack(){
        DivideRegions();
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
            rootRising.Play();
            StartCoroutine(DolphinOut(0, dolphin));
            currentLeft = currentRight;
            currentRight = currentLeft+partition_size;
        }
    }

    Transform createDolphin(Vector3 position, Transform parent){
        AudioSource.PlayClipAtPoint(show, position);
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
            rootRising.Stop();
            yield return new WaitForSeconds(2);
            rootRising.Play();
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
            rootRising.Stop();
            canAttack = true;
            yield break;
        }
        StartCoroutine(DolphinBack(parent));
    }

    [ContextMenu("ShowWeakFace")]
    void ShowWeakFace(){
        invulnerable = false;
        faceSpriteRenderer.sprite = weakFaceSprite;
        face.transform.position = faceFinalPosition.position + (Vector3.up * topBound);
        StartCoroutine(MoveFaceCoroutine(face.transform.position, faceFinalPosition.position, moveDuration));
    }

    [ContextMenu("HideWeakFace")]
    void HideWeakFace(){
        StartCoroutine(HideWeakFaceCoroutine());
    }

    IEnumerator HideWeakFaceCoroutine(){
        invulnerable = true;
        canAttack = true;
        faceSpriteRenderer.sprite = angryFaceSprite;
        yield return new WaitForSeconds(1);
        Vector3 finalPosition = faceFinalPosition.position + (Vector3.up * topBound);
        StartCoroutine(MoveFaceCoroutine(face.transform.position, finalPosition, moveDuration));

        StartCoroutine(pusherDuration.Tweeng((v)=> pusher.localPosition=v, Vector3.zero, pusherFinalPosition));
        yield return new WaitForSeconds(pusherDuration);
        foreach(Transform tent in tentacleBarrier) {
            tent.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(2);
        pusher.localPosition = Vector3.zero;
    }

    [ContextMenu("ShowHappyFace")]
    public void ShowHappyFace(){
        faceSpriteRenderer.sprite = happyFaceSprite;
        face.transform.position = faceFinalPosition.position + (Vector3.up * topBound);
        StartCoroutine(MoveFaceCoroutine(face.transform.position, faceFinalPosition.position, moveDuration));
    }

    [ContextMenu("ShowDeadFace")]
    public void ShowDeadFace(){
        faceSpriteRenderer.sprite = deadFaceSprite;
        // face.transform.position = faceFinalPosition.position + (Vector3.up * topBound);
        // StartCoroutine(MoveFaceCoroutine(face.transform.position, faceFinalPosition.position, moveDuration));
    }

    IEnumerator MoveFaceCoroutine(Vector3 startPos, Vector3 endPos, float duration) {
        Vector3 currentPos = startPos;
        float t = 0f;
        while(t < duration) {
            face.transform.position = Vector3.Lerp(currentPos, endPos, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
        face.transform.position = endPos;
        yield return null;
    }

    [ContextMenu("Enter Damage")]
    void DamagePhase() {
        TentaclesDefeated = tentaclesNeeded;
    }

    IEnumerator EnterDamagePhase() {
        foreach(Transform tent in tentacleBarrier) {
            tent.GetComponent<Animator>().SetTrigger("Hide");
        }
        yield return new WaitForSeconds(.5f);
        foreach(Transform tent in tentacleBarrier) {
            tent.gameObject.SetActive(false);
        }
        ShowWeakFace();

        yield return new WaitForSeconds(damagePhaseDuration);

        HideWeakFace();
    }

    [ContextMenu("SecondPhase")]
    void SecondPhase(){
        phase = 2;
        attacksBeforeTentacles += 1;
        attackCooldown -= 1;
        regions += 2;
        StartCoroutine(HideWeakFaceCoroutine());
        ShowMidWater();
    }

    [ContextMenu("ThirdPhase")]
    void ThirdPhase(){
        phase = 3;
        attacksBeforeTentacles += 1;
        attackCooldown -= 1;
        // regions += 2;
        StartCoroutine(HideWeakFaceCoroutine());
        ShowNoWater();
    }

    [ContextMenu("ShowMidWater")]
    void ShowMidWater(){
        StartCoroutine(lakeDuration.Tweeng((a)=>MidWater.GetComponent<SpriteRenderer>().color += new Color (0, 0, 0, a), 0f, 1f));
    }

    [ContextMenu("ShowNoWater")]
    void ShowNoWater(){
        StartCoroutine(lakeDuration.Tweeng((a)=>NoWater.GetComponent<SpriteRenderer>().color += new Color (0, 0, 0, a), 0f, 1f));
    }

    [ContextMenu("TakeDamage")]
    public void Damage() {
        if (invulnerable) return;
        health -= 1;
        healthBar.localScale = new Vector3((float)health / maxHealth, healthBar.localScale.y, healthBar.localScale.z);
        if (health < 0) {
            return;
        }
        health = Mathf.Clamp(health, 0, health);
        // healthBar.localScale = new Vector3((float)health / boss.tentaclesHealth, healthBar.localScale.y, healthBar.localScale.z);
        Debug.Log("Continue current phase "+phase);
        StartCoroutine(damageRoutine());
        if (health == 0) {
            Death();
            GameManager.instance.Win();
        }else if (health <= healthPerPhase && phase == 2){ // Third phase
            ThirdPhase();
        }else if (health <= healthPerPhase*2 && phase == 1){ // Second phase
            SecondPhase();
        }
    }

    IEnumerator damageRoutine() {
        sr.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMaterial;
    }

    public void GameEnds(){
        StopAllCoroutines();
    }

    void Death(){
        invulnerable = true;
        ShowDeadFace();
        GameEnds();
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

        if (canAttack && attackRoutine == null){
            attackRoutine = StartCoroutine(Attack());
        }
    }

    private void OnEnable() {
        foreach(Transform tent in tentacleBarrier) {
            tent.gameObject.SetActive(true);
        }
    }
}
