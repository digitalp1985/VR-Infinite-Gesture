using UnityEngine;
using System.Collections;

namespace WinterMute
{

    public class VRGestureUISwitcher : MonoBehaviour
    {
        VRAvatar myAvatar;
        IInput input;

        Transform playerHead;
        Transform playerHandL;
        Transform playerHandR;

        public VRGestureUI vrGestureUI;

        void Start()
        {
            myAvatar = PlayerManager.GetPlayerAvatar(0);

            playerHead = myAvatar.headTF;
            playerHandR = myAvatar.vrRigAnchors.rHandAnchor;
            playerHandL = myAvatar.vrRigAnchors.lHandAnchor;

            if (Config.gestureUIHand == GestureUIHand.Right)
            {
                input = myAvatar.GetInput(VROptions.Handedness.Left);
            }
            else if (Config.gestureUIHand == GestureUIHand.Left)
            {
                input = myAvatar.GetInput(VROptions.Handedness.Right);
            }
        }

        void Update ()
        {
            // if vr button 1 toggle the vr gesture UI visibility
            if (input.GetButtonDown(InputOptions.Button.Button1))
            {
                vrGestureUI.gameObject.SetActive(!vrGestureUI.gameObject.activeInHierarchy);
            }
        }
    }
}
