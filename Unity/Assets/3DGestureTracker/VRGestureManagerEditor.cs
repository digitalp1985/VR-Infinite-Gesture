using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(VRGestureManager)), CanEditMultipleObjects]
public class VRGestureManagerEditor : Editor
{
	VRGestureManager vrGestureManager;

	// neural net gui helpers
	int selectedNeuralNetIndex = 0;
	string newNeuralNetName;

	// gestures gui helpers
	string editGesturesButtonText;
	bool editGestures = true;

	public enum EditorListOption 
	{
		None = 0,
		ListSize = 1,
		ListLabel = 2,
		ElementLabels = 4,
		Buttons = 8,
		Default = ListSize | ListLabel | ElementLabels,
		NoElementLabels = ListSize | ListLabel,
		ListLabelButtons = ListLabel | Buttons,
		All = Default | Buttons
	}

	private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

	private static GUIContent
	useToggleContent = new GUIContent("", "use this gesture"),
	moveButtonContent = new GUIContent("\u21b4", "move down"),
	duplicateButtonContent = new GUIContent("+", "duplicate"),
	deleteButtonContent = new GUIContent("-", "delete"),
	addButtonContent = new GUIContent("+", "add element");

    public override void OnInspectorGUI()
    {
//        DrawDefaultInspector();

		vrGestureManager = (VRGestureManager)target;

		serializedObject.Update();

		ShowTransforms();
		ShowNeuralNets();
		// if a neural net is selected
		if (neuralNetGUIMode == NeuralNetGUIMode.ShowPopup)
			ShowGestures();
		ShowTrainButton();
		serializedObject.ApplyModifiedProperties();
    }

	void ShowTransforms ()
	{
		EditorGUILayout.PropertyField(serializedObject.FindProperty("vrRigAnchors"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHead"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHand"));
	}

	void ShowNeuralNets()
	{
		
		EditorGUILayout.LabelField("NEURAL NETWORK");
		string[] neuralNetsArray = new string[0];
		if (vrGestureManager.neuralNets.Count > 0)
			neuralNetsArray = ConvertStringListPropertyToStringArray("neuralNets");


		// STATE CONTROL
		if (neuralNetGUIMode == NeuralNetGUIMode.EnterNewNetName)
		{
			ShowNeuralNetCreateNewOptions();
		}
		else if (neuralNetsArray.Length == 0) // if the neural nets list is empty show a big + button
		{
			neuralNetGUIMode = NeuralNetGUIMode.None;
		}
		else // draw the popup and little plus and minus buttons
		{
			neuralNetGUIMode = NeuralNetGUIMode.ShowPopup;
		}

		// RENDER
		GUILayout.BeginHorizontal();
		switch (neuralNetGUIMode)
		{
			case (NeuralNetGUIMode.None):
				// show big + button
				if (GUILayout.Button("+"))
				{
					newNeuralNetName = "";
					GUI.FocusControl("Clear"); 
					neuralNetGUIMode = NeuralNetGUIMode.EnterNewNetName;
					newNeuralNetName = "";
				}
			break;
			case (NeuralNetGUIMode.ShowPopup):
				ShowNeuralNetPopup(neuralNetsArray);
			break;
		}
		GUILayout.EndHorizontal();

		// TEMP

		// DEBUG ONLY
//		ShowList(serializedObject.FindProperty("neuralNets"), EditorListOption.ListLabelButtons);


	}

	enum NeuralNetGUIMode { None, EnterNewNetName, ShowPopup };
	NeuralNetGUIMode neuralNetGUIMode;

	void ShowNeuralNetCreateNewOptions ()
	{
		newNeuralNetName = EditorGUILayout.TextField(newNeuralNetName);
		if (GUILayout.Button("Create Network"))
		{
			if (string.IsNullOrEmpty(newNeuralNetName))
			{
				EditorUtility.DisplayDialog("Please give the new neural network a name", " ", "ok");
			}
			else if (vrGestureManager.CheckForDuplicateNeuralNetName(newNeuralNetName))
			{
				EditorUtility.DisplayDialog(
					"The name " + newNeuralNetName + " is already being used, " +
					"please name it something else.", " ", "ok"
				);
			}
			else 
			{
				vrGestureManager.CreateNewNeuralNet(newNeuralNetName);
				selectedNeuralNetIndex = vrGestureManager.neuralNets.IndexOf(newNeuralNetName);
				neuralNetGUIMode = NeuralNetGUIMode.ShowPopup;
			}
		}

	}

	void ShowNeuralNetPopup (string[] neuralNetsArray)
	{
		selectedNeuralNetIndex = EditorGUILayout.Popup(selectedNeuralNetIndex, neuralNetsArray);
		string selectedNeuralNetName = "";
		if (selectedNeuralNetIndex < neuralNetsArray.Length)
			selectedNeuralNetName = neuralNetsArray[selectedNeuralNetIndex];

		vrGestureManager.SelectNeuralNet(selectedNeuralNetName);

		// + button
		if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth))
		{
			newNeuralNetName = "";
			GUI.FocusControl("Clear");
			neuralNetGUIMode = NeuralNetGUIMode.EnterNewNetName;

		}

		// - button
		if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
		{
			if (ShowNeuralNetDeleteDialog(selectedNeuralNetName))
			{
				vrGestureManager.DeleteNeuralNet(selectedNeuralNetName);
				if (vrGestureManager.neuralNets.Count > 0)
					selectedNeuralNetIndex = 0;
			}
		}
	}

	bool ShowNeuralNetDeleteDialog (string neuralNetName)
	{
		return EditorUtility.DisplayDialog("Delete the " + neuralNetName + " neural network?", 
			"This cannot be undone.",
			"ok",
			"cancel"
		);
	}

	void ShowGestures()
	{
		EditorGUILayout.LabelField("GESTURES IN THIS NETWORK");
		ShowList(serializedObject.FindProperty("gestures"), EditorListOption.Buttons);
		EditGesturesButtonUpdate();
	}

	void ShowTrainButton()
	{
		if (GUILayout.Button("Train"))
		{
			vrGestureManager.BeginTraining();
		}
	}

	string[] ConvertStringListPropertyToStringArray (string listName)
	{
		SerializedProperty sp = serializedObject.FindProperty(listName).Copy();
		if (sp.isArray)
		{
			int arrayLength = 0;
			sp.Next(true); // skip generic field
			sp.Next(true); // advance to array size field

			// get array size
			arrayLength = sp.intValue;

			sp.Next(true); // advance to first array index

			// write values to list
			string[] values = new string[arrayLength];
			int lastIndex = arrayLength - 1;
			for (int i = 0; i < arrayLength; i++)
			{
				values[i] = sp.stringValue; // copy the value to the array
				if (i < lastIndex) 
					sp.Next(false); // advance without drilling into children
			}
			return values;
		}
		return null;
	}

	void ShowList (SerializedProperty list, EditorListOption options = EditorListOption.Default)
	{
		bool showListLabel = (options & EditorListOption.ListLabel) != 0;
		bool showListSize = (options & EditorListOption.ListSize) != 0;
		if (showListLabel)
		{
			EditorGUILayout.PropertyField(list);
			EditorGUI.indentLevel += 1;
		}
		if (!showListLabel || list.isExpanded)
		{
			SerializedProperty size = list.FindPropertyRelative("Array.size");
			if (showListSize)
			{
				EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
			}
			if (size.hasMultipleDifferentValues)
			{
				EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
			}
			else
			{
				ShowElements(list, options);
			}
		}
		if (showListLabel)
			EditorGUI.indentLevel -= 1;
	}

	private static void ShowElements (SerializedProperty list, EditorListOption options)
	{
		if (!list.isArray)
		{
			EditorGUILayout.HelpBox(list.name + " is neither an array nor a list", MessageType.Error);
			return;
		}

		bool showElementLabels = (options & EditorListOption.ElementLabels) != 0;
		bool showButtons = (options & EditorListOption.Buttons) != 0;

		// render the list
		for (int i = 0; i < list.arraySize; i++)
		{

			if (showButtons)
			{
				EditorGUILayout.BeginHorizontal();
			}
			if (showElementLabels)
			{
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
			}
			else
			{
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);
			}
			if (showButtons)
			{
				ShowButtons(list, i);
				EditorGUILayout.EndHorizontal();
			}
		}

		// if the list is empty show the plus button
		if (showButtons && list.arraySize == 0 && GUILayout.Button(addButtonContent, EditorStyles.miniButton))
		{
			list.arraySize += 1;
		}
	}

	private static void ShowButtons (SerializedProperty list, int index)
	{
		// use button
		if (GUILayout.Toggle(false, useToggleContent, miniButtonWidth))
		{
//			Debug.Log("do ssomething toggle");
		}
		// plus button
		if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth))
		{
			list.InsertArrayElementAtIndex(index);
		}
		// minus button
		if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
		{
			int oldSize = list.arraySize;
			list.DeleteArrayElementAtIndex(index);
			if (list.arraySize == oldSize)
				list.DeleteArrayElementAtIndex(index);
		}
	}
		
	void EditGesturesButtonUpdate ()
	{
		editGesturesButtonText = editGestures ? "Edit Gestures" : editGesturesButtonText = "Save Gestures";

		VRGestureManager script = (VRGestureManager)target;
		if (GUILayout.Button(editGesturesButtonText))
		{
			editGestures = !editGestures;
			//            script.TestMe();
		}
	}

}