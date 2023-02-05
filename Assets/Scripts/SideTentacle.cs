using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideTentacle : MonoBehaviour
{

    public int direction;
    public float speed;
    public float movementTime;
    public float idleTime;

    void Start()
    {
        StartCoroutine(TentacleMovement());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0,0);
    }

    IEnumerator TentacleMovement() {
        int initialDirection = direction;
        yield return new WaitForSeconds(movementTime);
        direction = 0;
        yield return new WaitForSeconds(idleTime);
        direction = initialDirection * -1;
        yield return new WaitForSeconds(movementTime);
        Destroy(gameObject);
    }
}
