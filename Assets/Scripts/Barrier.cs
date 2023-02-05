using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] AudioClip show;
    public void DisableHealthBar() {
        return;
    }

    public void EnableHealthBar() {
        return;
    }

    public void ShowSound() {
        AudioSource.PlayClipAtPoint(show, transform.position);
    }
}
