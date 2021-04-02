﻿using System.Collections;
using UnityEngine;
using agora_gaming_rtc;

public class ProfileSelection : Photon.PunBehaviour
{
    private bool isBroadcaster;
    private AgoraVideoChat agoraScript;
    private IRtcEngine agoraEngine;

    [Header("UI Elements")]
    [SerializeField]
    private GameObject BroadCastSelectionPanel;
    [SerializeField]
    private GameObject PartyUIContainer;

    [Header("Broadcaster")]
    [SerializeField]
    private Material broadcasterMaterial;
    [SerializeField]
    private SkinnedMeshRenderer vikingMesh;
    
    void Start()
    {
        if(photonView.isMine)
        {
            agoraEngine = null;
            isBroadcaster = false;

            BroadCastSelectionPanel.SetActive(false);
            PartyUIContainer.SetActive(false);

            agoraScript = GetComponent<AgoraVideoChat>();
            StartCoroutine(AgoraEngineSetup());
        }
    }

    IEnumerator AgoraEngineSetup()
    {
        if (photonView.isMine)
        {
            agoraEngine = AgoraEngine.mRtcEngine;
            float engineTimer = 0f;
            float engineTimeout = 3f;


            while (agoraEngine == null)
            {
                agoraEngine = AgoraEngine.mRtcEngine;
                engineTimer += Time.deltaTime;

                if (engineTimer >= engineTimeout)
                {
                    Debug.LogError("InCallStats AgoraEngineSetup() Failure - No Agora Engine Found.");
                    yield break;
                }

                yield return null;
            }

            agoraEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            BroadCastSelectionPanel.SetActive(true);
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if(photonView.isMine)
        {
            base.OnPhotonPlayerConnected(newPlayer);

            TurnVikingGold();
        }
    }

    public void TurnVikingGold()
    {
        if (isBroadcaster)
        {
            photonView.RPC("UpdateBroadcasterMaterial", PhotonTargets.All);
        }
    }

    [PunRPC]
    public void UpdateBroadcasterMaterial()
    {
        vikingMesh.material = broadcasterMaterial;
    }

    public void ButtonSetBroadCastState(bool isNewStateBroadcaster)
    {
        if (photonView.isMine)
        {
            if (isNewStateBroadcaster)
            {
                agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
                agoraEngine.SetLocalPublishFallbackOption(STREAM_FALLBACK_OPTIONS.STREAM_FALLBACK_OPTION_AUDIO_ONLY);
                isBroadcaster = true;

                TurnVikingGold();
            }
            else
            {
                agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
                agoraEngine.SetRemoteSubscribeFallbackOption(STREAM_FALLBACK_OPTIONS.STREAM_FALLBACK_OPTION_AUDIO_ONLY);
                isBroadcaster = false;
            }

            PartyUIContainer.SetActive(true);
            BroadCastSelectionPanel.SetActive(false);
            
            agoraScript.JoinChannel();
        }
    }
}