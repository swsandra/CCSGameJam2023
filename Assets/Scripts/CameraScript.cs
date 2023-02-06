using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] Transform player;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Awake() {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    [ContextMenu("ZoomIn")]
    public void ZoomIn() {
        StartCoroutine(.1f.Tweeng((p)=>transform.position=p, transform.position, new Vector3(player.position.x,player.position.y + 3 * player.localScale.y, -10)));
        StartCoroutine(.1f.Tweeng((s)=>cinemachineVirtualCamera.m_Lens.OrthographicSize=s, 5f, 2f));
    }

    [ContextMenu("CameraToTree")]
    public void CameraToTree() {
        StartCoroutine(.5f.Tweeng((p)=>transform.position=p, transform.position, new Vector3(0,3.5f, -10)));
    }

    [ContextMenu("ZoomOut")]
    public void ZoomOut() {
        StartCoroutine(.8f.Tweeng((p)=>transform.position=p, transform.position, new Vector3(0,0,-10)));
        StartCoroutine(.8f.Tweeng((s)=>cinemachineVirtualCamera.m_Lens.OrthographicSize=s, 2f, 5f));
    }

    [ContextMenu("StartRumbling")]
    public void StartRumbling() {
        CinemachineBasicMultiChannelPerlin perlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = 0.5f;
        perlin.m_FrequencyGain = 0.5f;
    }

    [ContextMenu("StopRumbling")]
    public void StopRumbling() {
        CinemachineBasicMultiChannelPerlin perlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = 0;
        perlin.m_FrequencyGain = 0;
    }
}
