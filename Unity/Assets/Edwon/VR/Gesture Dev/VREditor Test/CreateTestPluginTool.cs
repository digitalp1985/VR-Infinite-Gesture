#if UNITY_EDITOR_VR_DEV

using System;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEngine.VR.Menus;
using UnityEngine.VR.Tools;
using UnityEngine.VR.Utilities;
using UnityObject = UnityEngine.Object;

[MainMenuItem("Infinite Gesture", "Edwon", "description")]
public class CreateTestPluginTool : MonoBehaviour, ITool, IStandardActionMap, IConnectInterfaces, IInstantiateMenuUI, IUsesRayOrigin, IUsesSpatialHash
{
    [SerializeField]
    CreateTestPluginMenu m_MenuPrefab;

    GameObject m_ToolMenu;

    public Transform rayOrigin { get; set; }

    public ConnectInterfacesDelegate connectInterfaces { private get; set; }

    public Func<Transform, IMenu, GameObject> instantiateMenuUI { private get; set; }

    public Action<GameObject> addToSpatialHash { get; set; }
    public Action<GameObject> removeFromSpatialHash { get; set; }

    void Start()
    {
        m_ToolMenu = instantiateMenuUI(rayOrigin, m_MenuPrefab);
        var createPrimitiveMenu = m_ToolMenu.GetComponent<CreatePrimitiveMenu>();
        connectInterfaces(createPrimitiveMenu, rayOrigin);
    }

    public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
    {
        var standardInput = (Standard)input;

    }

}

#endif