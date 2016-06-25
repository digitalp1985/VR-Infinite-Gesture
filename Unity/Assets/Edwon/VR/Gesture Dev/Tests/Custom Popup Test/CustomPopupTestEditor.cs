using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace Edwon.VR.Gesture.Test
{

	[CustomEditor(typeof(CustomPopupTest))]
	public class CustomPopupTestEditor : Editor
	{
		int choiceIndex;
		CustomPopupTest testManager;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			testManager = target as CustomPopupTest;

			DrawPopup();

			EditorUtility.SetDirty(target);
		}

		void DrawPopup()
		{
			String[] stringArray = ConvertStringListPropertyToStringArray("list");
			choiceIndex = Array.IndexOf(stringArray, testManager.choice);

			// If the choice is not in the array then the _choiceIndex will be -1 so set back to 0
			if (choiceIndex < 0)
				choiceIndex = 0;

			choiceIndex = EditorGUILayout.Popup(choiceIndex, stringArray);

			// Update the selected choice in the underlying object
			if (stringArray.Length > 0)
			{
				//				choiceIndex = 0;
				testManager.choice = stringArray[choiceIndex];
			}
			else
			{
				testManager.choice = null;
			}
		}
			
		string[] ConvertStringListPropertyToStringArray(string listName)
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
			
	}
}