using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] Transform player;

    [ContextMenu("ZoomIn")]
    public void ZoomIn() {
        Debug.Log(player.localScale.y);
        StartCoroutine(.1f.Tweeng((p)=>transform.position=p, transform.position, new Vector3(player.position.x,player.position.y + 3 * player.localScale.y, -10)));
        StartCoroutine(.1f.Tweeng((s)=>GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize=s, 5f, 2f));
    }

    [ContextMenu("ZoomOut")]
    public void ZoomOut() {
        StartCoroutine(.8f.Tweeng((p)=>transform.position=p, transform.position, new Vector3(0,0,-10)));
        StartCoroutine(.8f.Tweeng((s)=>GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize=s, 2f, 5f));
    }
}
