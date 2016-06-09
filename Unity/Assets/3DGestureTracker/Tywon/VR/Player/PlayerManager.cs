using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager {

    private static PlayerManager _instance;

    List<VRAvatar> avatars;
    Transform playerCam;

    private PlayerManager() {
        avatars = new List<VRAvatar>();
    }

    public static PlayerManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new PlayerManager();
            }
            return _instance;
        }
    }

    public VRAvatar GetPlayerAvatar(int index)
    {
        return avatars[index];
    }

    public Transform GetPlayerCamera(int index)
    {
        return playerCam;
    }
}