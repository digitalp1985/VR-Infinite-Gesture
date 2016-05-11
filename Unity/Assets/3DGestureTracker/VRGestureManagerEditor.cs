using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(VRGestureManager)), CanEditMultipleObjects]
public class VRGestureManagerEditor : Editor
{
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

		serializedObject.Update();

		ShowTransforms();
		ShowNeuralNets();
		ShowGestures();

		serializedObject.ApplyModifiedProperties();
    }

	void ShowTransforms ()
	{
		EditorGUILayout.PropertyField(serializedObject.FindProperty("vrRigAnchors"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHead"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHand"));
	}

	int selectedNeuralNetIndex = 0;

	void ShowNeuralNets()
	{
		
		EditorGUILayout.LabelField("NEURAL NETWORK");
		GUILayout.BeginHorizontal();
		string[] neuralNetsArray = ConvertStringListPropertyToStringArray("neuralNets");
		if (neuralNetsArray.Length == 0) // if the neural nets list is empty show a big + button
		{
			if (GUILayout.Button("+"))
			{

			}
		}
		else // draw the popup and little plus and minus buttons
		{
			selectedNeuralNetIndex = EditorGUILayout.Popup(selectedNeuralNetIndex, neuralNetsArray);
			if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth))
			{

			}
			if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
			{

			}
			GUILayout.EndHorizontal();
		}

		// DEBUG ONLY
//		ShowList(serializedObject.FindProperty("neuralNets"), EditorListOption.ListLabelButtons);


	}

	void ShowGestures()
	{
		EditorGUILayout.LabelField("GESTURES IN THIS NETWORK");
		ShowList(serializedObject.FindProperty("gestures"), EditorListOption.Buttons);
		EditGesturesButtonUpdate();
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

	string editGesturesButtonText;
	bool editGestures = true;

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