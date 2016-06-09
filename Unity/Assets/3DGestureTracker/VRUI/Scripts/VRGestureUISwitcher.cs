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
            myAvatar = PlayerManager.Instance.GetPlayerAvatar(0);

            playerHead = myAvatar.headTF;
            playerHandR = myAvatar.vrRigAnchors.rHandAnchor;
            playerHandL = myAvatar.vrRigAnchors.lHandAnchor;

            if (Config.gestureHand == GestureHand.Right)
            {
                input = myAvatar.GetInput(VROptions.Handedness.Left);
            }
            else if (Config.gestureHand == GestureHand.Left)
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
