using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [SerializeField]
    AudioClip MenuSong;
    [SerializeField]
    AudioClip GameSongIntro;
    [SerializeField]
    AudioClip GameSongLoop;
    [SerializeField]
    GameObject Boss;
    [SerializeField]
    GameObject Player;
    [SerializeField]
    GameObject UI;

    private void Start() {
        GetComponent<AudioSource>().clip = MenuSong;
        GetComponent<AudioSource>().Play();
        GetComponent<AudioSource>().loop = true;
    }

    public void StartGame()
    {
        StartCoroutine(PlayGameSong());
        Player.GetComponent<PlayerController>().enabled = true;
        Boss.GetComponent<BossController>().enabled = true;
        UI.SetActive(false);
    }
 
    IEnumerator PlayGameSong()
    {
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().clip = GameSongIntro;
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(GameSongIntro.length);
        GetComponent<AudioSource>().clip = GameSongLoop;
        GetComponent<AudioSource>().Play();
        GetComponent<AudioSource>().loop = true;
    }
}
