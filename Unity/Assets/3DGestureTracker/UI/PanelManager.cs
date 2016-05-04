using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour
{
    Animator panelAnim;
    public string initialPanel;

    void OnEnable ()
    {
        panelAnim = GetComponent<Animator>();

        // initialize with main menu focused
        FocusPanel(initialPanel);
    }

    public void FocusPanel (string panelName)
    {
        panelAnim.SetTrigger(panelName);
        //GameObject panel = transform.FindChild(panelName).gameObject;
        //GameObject selectableButton = FindFirstEnabledSelectable(panel);
        //SetSelected(selectableButton);
    }



    // UTILITY

	static GameObject FindFirstEnabledSelectable (GameObject gameObject)
	{
		GameObject go = null;
		var selectables = gameObject.GetComponentsInChildren<Selectable> (true);
		foreach (var selectable in selectables) {
			if (selectable.IsActive () && selectable.IsInteractable ()) {
				go = selectable.gameObject;
				break;
			}
		}
		return go;
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);

		var standaloneInputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
		if (standaloneInputModule != null && standaloneInputModule.inputMode == StandaloneInputModule.InputMode.Buttons)
			return;

		EventSystem.current.SetSelectedGameObject(null);
	}
}
