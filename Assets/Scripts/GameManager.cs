using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    AudioClip MenuSong;
    [SerializeField]
    AudioClip GameSongIntro;
    [SerializeField]
    AudioClip GameSongLoop;
    [SerializeField]
    AudioClip WinSong;
    [SerializeField]
    AudioClip FailSong;
    [SerializeField]
    GameObject Boss;
    [SerializeField]
    GameObject Player;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

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
        UIManager.instance.HideTitle();
    }

    public void Win()
    {
        Player.GetComponent<PlayerController>().enabled = false;
        GetComponent<AudioSource>().clip = WinSong;
        GetComponent<AudioSource>().Play();
        Boss.GetComponent<BossController>().GameEnds();
        UIManager.instance.ShowWin();
    }

    public void GameOver()
    {
        Boss.GetComponent<BossController>().ShowHappyFace();
        Boss.GetComponent<BossController>().GameEnds();
        GetComponent<AudioSource>().clip = FailSong;
        GetComponent<AudioSource>().Play();
        Player.GetComponent<PlayerController>().enabled = false;
        UIManager.instance.ShowGameOver();
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Game");
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
