using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// TODO 複数のInputSettingsDataをコピー出来るとより楽になる
// TODO D&Dによる並び替え
// TODO? Applyした時のモーダル表示
// TODO InputTypeを複数一気に設定できるようにする。(EnumPopupではなくEnumFlagsField使えばいけそう)→マウス移動とスティックが競合するのでNG

public class InputManagerGeneratorWindow : EditorWindow {
	Vector2 scrollPos;

	List<InputSettingData> inputSettingList = new List<InputSettingData>();

	const string addStr = "Add";
	const string loadInputManagerStr = "Load InputManager";
	const string clearSettingsModalMessage = "If you cleared settings, don't possible to return.\n(It's unrelated to InputManager.)";
	const string missingStr = "Missing";

	/// <summary>
	/// EditorPrefs keys.
	/// </summary>
	const string INPUT_SETTING_NUM_KEY = "Input settings num";
	const string IS_DISPLAY_KEY = "Is display";
	const string NAME_KEY = "Input name";
	const string DESCRIPTIVE_KEY = "Descriptive";
	const string NEGATIVE_DESCRIPTIVE_KEY = "Negative descriptive";
	const string NEGATIVE_BUTTON_KEY = "Negative button";
	const string POSITIVE_BUTTON_KEY = "Positive button";
	const string ALT_NEGATIVE_BUTTON_KEY = "Alt negative button";
	const string ALT_POSITIVE_BUTTON_KEY = "Alt positive button";
	const string GRAVITY_BUTTON_KEY = "Gravity";
	const string DEAD_KEY = "Dead";
	const string SENSITIVITY_KEY = "Sensitivity";
	const string SNAP_KEY = "Snap";
	const string INVERT_KEY = "Invert";
	const string INPUT_TYPE_KEY = "Input type";
	const string AXIS_TYPE_KEY = "Axis type";
	const string JOYSTICK_NUM_KEY = "Joystick num";

	[MenuItem ("Tools/InputManagerGenerator")]
	static public void Init() {
		var window = GetWindow<InputManagerGeneratorWindow>();
		window.Show ();
	}

	void OnEnable() {
		// Load property with EditorPrefs.
		int inputSettingsCount = EditorPrefs.GetInt(INPUT_SETTING_NUM_KEY, 0);
		for (int i = 0; i < inputSettingsCount; i++) {
			InputSettingData data = new InputSettingData(EditorPrefs.GetString (NAME_KEY + i, missingStr), EditorPrefs.GetBool (IS_DISPLAY_KEY + i, false));
			data.descriptive = EditorPrefs.GetString (DESCRIPTIVE_KEY + i, missingStr);
			data.negativeDescriptive = EditorPrefs.GetString (NEGATIVE_DESCRIPTIVE_KEY + i, missingStr);
			data.negativeButton = EditorPrefs.GetString (NEGATIVE_BUTTON_KEY + i, missingStr);
			data.positiveButton = EditorPrefs.GetString (POSITIVE_BUTTON_KEY + i, missingStr);
			data.altNegativeButton = EditorPrefs.GetString (ALT_NEGATIVE_BUTTON_KEY + i, missingStr);
			data.altPositiveButton = EditorPrefs.GetString (ALT_POSITIVE_BUTTON_KEY + i, missingStr);
			data.gravity = EditorPrefs.GetFloat (GRAVITY_BUTTON_KEY + i, 0);
			data.dead = EditorPrefs.GetFloat (DEAD_KEY + i, 0);
			data.sensitivity = EditorPrefs.GetFloat (SENSITIVITY_KEY + i, 0);
			data.snap = EditorPrefs.GetBool (SNAP_KEY + i, false);
			data.invert = EditorPrefs.GetBool (INVERT_KEY + i, false);
			data.inputType = (InputType)EditorPrefs.GetInt (INPUT_TYPE_KEY + i, (int)InputType.KeyOrMouseButton);
			data.axisType = (AxisType)EditorPrefs.GetInt (AXIS_TYPE_KEY + i, (int)AxisType.XAxis);
			data.joystickNum = (JoystickNum)EditorPrefs.GetInt (JOYSTICK_NUM_KEY + i, (int)JoystickNum.All);
			inputSettingList.Add (data);
		}
	}

	void OnDestroy() {
		// Save property with EditorPrefs.
		EditorPrefs.SetInt (INPUT_SETTING_NUM_KEY, inputSettingList.Count);
		for (int i = 0; i < inputSettingList.Count; i++) {
			InputSettingData data = inputSettingList [i];
			EditorPrefs.SetString (NAME_KEY + i, data.name);
			EditorPrefs.SetBool (IS_DISPLAY_KEY + i, data.isDisplay);
			EditorPrefs.SetString (DESCRIPTIVE_KEY + i, data.descriptive);
			EditorPrefs.SetString (NEGATIVE_DESCRIPTIVE_KEY + i, data.negativeDescriptive);
			EditorPrefs.SetString (NEGATIVE_BUTTON_KEY + i, data.negativeButton);
			EditorPrefs.SetString (POSITIVE_BUTTON_KEY + i, data.positiveButton);
			EditorPrefs.SetString (ALT_NEGATIVE_BUTTON_KEY + i, data.altNegativeButton);
			EditorPrefs.SetString (ALT_POSITIVE_BUTTON_KEY + i, data.altPositiveButton);
			EditorPrefs.SetFloat (GRAVITY_BUTTON_KEY + i, data.gravity);
			EditorPrefs.SetFloat (DEAD_KEY + i, data.dead);
			EditorPrefs.SetFloat (SENSITIVITY_KEY + i, data.sensitivity);
			EditorPrefs.SetBool (SNAP_KEY + i, data.snap);
			EditorPrefs.SetBool (INVERT_KEY + i, data.invert);
			EditorPrefs.SetInt (INPUT_TYPE_KEY + i, (int)data.inputType);
			EditorPrefs.SetInt (AXIS_TYPE_KEY + i, (int)data.axisType);
			EditorPrefs.SetInt (JOYSTICK_NUM_KEY + i, (int)data.joystickNum);
		}
	}

	void OnGUI() {
		Color bgColor = GUI.backgroundColor;

		using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos)) {
			scrollPos = scrollView.scrollPosition;

			if (inputSettingList.Count == 0) {
				EditorGUILayout.LabelField (string.Format ("Input settings is nothing. You can \"{0}\" or \"{1}\".", addStr, loadInputManagerStr));
			}

			for (int i = 0; i < inputSettingList.Count; i++) {
				DrawInputSettingsProperty (inputSettingList [i]);
			}
		}

		// Exchange
		for (int i = 0; i < inputSettingList.Count; i++) {
			if (inputSettingList[i].isUp) {
				inputSettingList[i].isUp = false;
				if (i != 0) {
					InputSettingData work = inputSettingList[i];
					inputSettingList[i] = inputSettingList[i - 1];
					inputSettingList[i - 1] = work;
				}
			}
			if (inputSettingList[i].isDown) {
				inputSettingList[i].isDown = false;
				if (i != inputSettingList.Count - 1) {
					InputSettingData work = inputSettingList[i];
					inputSettingList[i] = inputSettingList[i + 1];
					inputSettingList[i + 1] = work;
				}
			}
		}

		// Duplicate
		int count = inputSettingList.Count;
		for (int i = 0; i < count; i++) {
			if (inputSettingList[i].isDuplicate) {
				inputSettingList[i].isDuplicate = false;
				InputSettingData clone = inputSettingList[i].Clone ();
				inputSettingList.Add (clone);
			}
		}

		// Remove
		for (int i = inputSettingList.Count - 1; i >= 0; i--) {
			if (inputSettingList[i].isRemove) {
				inputSettingList.RemoveAt (i);
			}
		}

		using (new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (addStr)) {
				inputSettingList.Add (new InputSettingData("New Input"));
			}
		}

		using (new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (loadInputManagerStr)) {
				inputSettingList.AddRange (LoadSettingsFromInputManager ());
			}
		}

		using (new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace ();
			using (new EditorGUI.DisabledGroupScope(inputSettingList.Count == 0)) {
				if (GUILayout.Button ("Clear Settings")) {
					ClearSettings ();
				}
			}

			GUI.backgroundColor = Color.green;
			if (GUILayout.Button ("Apply")) {
				Apply ();
			}
			GUI.backgroundColor = bgColor;
		}
	}

	/// <summary>
	/// Draw settings data property.
	/// </summary>
	/// <param name="data"></param>
	void DrawInputSettingsProperty(InputSettingData data) {
		data.isDisplay = Foldout (data.name, data.isDisplay, out data.isDuplicate, out data.isRemove, out data.isUp, out data.isDown);

		if (data.isDisplay) {
			data.name = EditorGUILayout.TextField ("Name", data.name);
			data.descriptive = EditorGUILayout.TextField ("Descriptive", data.descriptive);
			data.inputType = (InputType)EditorGUILayout.EnumPopup ("InputType", data.inputType);
			data.axisType = DrawAxisTypeProperty (data.inputType, data.axisType);
			data.joystickNum = DrawJoystickNumProperty (data.inputType, data.joystickNum);
			DrawPositiveButtonProperty (data.inputType, ref data.positiveButton, ref data.altPositiveButton);
			data.gravity = DrawGravityProperty (data.inputType, data.gravity);
			data.dead = EditorGUILayout.FloatField ("Dead", data.dead);
			data.snap = DrawSnapProperty (data.inputType, data.snap);
			data.sensitivity = EditorGUILayout.FloatField ("Sensitivity", data.sensitivity);
			data.invert = EditorGUILayout.Toggle ("Invert", data.invert);
			data.isSetNegativeButton = EditorGUILayout.Toggle ("Further set Negative Button", data.isSetNegativeButton);
			if (data.isSetNegativeButton) {
				DrawNegativeButtonProperty (data.inputType, ref data.negativeDescriptive, ref data.negativeButton, ref data.altNegativeButton);
			}
		}
	}

	/// <summary>
	/// Override settings data to InputManager.
	/// </summary>
	void Apply() {
		InputManagerAccessor accessor = new InputManagerAccessor ();
		accessor.SetInputSettingData (inputSettingList);
	}

	/// <summary>
	/// Claer all input settings data.
	/// </summary>
	void ClearSettings() {
		if (EditorUtility.DisplayDialog ("Clear Settings", clearSettingsModalMessage, "Continue", "Cancel")){
			inputSettingList.Clear ();
		}
	}

	List<InputSettingData> LoadSettingsFromInputManager() {
		InputManagerAccessor accessor = new InputManagerAccessor ();
		return accessor.GetInputSettingDataList ();
	}

	/// <summary>
	/// Draw foldout leyout.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="display"></param>
	/// <param name="remove"></param>
	/// <returns></returns>
    bool Foldout(string title, bool display, out bool duplicate, out bool remove, out bool isUp, out bool isDown) {
		float height = 22;
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.label).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = height;
        style.contentOffset = new Vector2(20f, -2f);

		Rect rect;
		using (new EditorGUILayout.HorizontalScope()) {
			GUILayout.Box (title, style, GUILayout.MaxHeight(height));
        	rect = GUILayoutUtility.GetLastRect();
			isUp = GUILayout.Button ("▲", GUILayout.MaxWidth(25), GUILayout.MaxHeight(height));
			isDown = GUILayout.Button ("▼", GUILayout.MaxWidth(25), GUILayout.MaxHeight(height));
			duplicate = GUILayout.Button("Duplicate", GUILayout.MaxWidth(70), GUILayout.MaxHeight (height));
			remove = GUILayout.Button ("Remove", GUILayout.MaxWidth(60), GUILayout.MaxHeight (height));
		}

        var e = Event.current;
        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint) {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
            display = !display;
            e.Use();
        }

		// if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition)) {
		// 	DragAndDrop.PrepareStartDrag ();
		// 	Object[] objects = new Object[1]{inputSettingList[0]};
		// 	DragAndDrop.objectReferences = objects;
		// 	DragAndDrop.StartDrag( inputSettingList[0].name);
        //     e.Use();
		// }

        return display;
    }

	#region Draw properties
	void DrawPositiveButtonProperty(InputType inputType, ref string positiveButton, ref string altPositiveButton) {
		bool editable = inputType == InputType.KeyOrMouseButton;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			positiveButton = EditorGUILayout.TextField ("PositiveButton", positiveButton);
			altPositiveButton = EditorGUILayout.TextField ("Alt PositiveButton", altPositiveButton);
		}
	}

	void DrawNegativeButtonProperty(InputType inputType, ref string descriptive, ref string negativeButton, ref string altNegativeButton) {
		bool editable = inputType == InputType.KeyOrMouseButton;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			descriptive = EditorGUILayout.TextField("Negative Descriptive", descriptive);
			negativeButton = EditorGUILayout.TextField("Negative Button", negativeButton);
			altNegativeButton = EditorGUILayout.TextField("Alt Negative Button", altNegativeButton);
		}
	}

	AxisType DrawAxisTypeProperty(InputType inputType, AxisType axisType) {
		bool editable = inputType == InputType.JoystickAxis || inputType == InputType.MouseMovement;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			return (AxisType)EditorGUILayout.EnumPopup("AxisType", axisType);
		}
	}

	float DrawGravityProperty(InputType inputType, float gravity) {
		bool editable = inputType == InputType.KeyOrMouseButton;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			return EditorGUILayout.FloatField("Gravity", gravity);
		}
	}

	bool DrawSnapProperty(InputType inputType, bool snap) {
		bool editable = inputType == InputType.KeyOrMouseButton;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			return EditorGUILayout.Toggle ("Snap", snap);
		}
	}

	JoystickNum DrawJoystickNumProperty(InputType inputType, JoystickNum joystickNum) {
		bool editable = inputType == InputType.JoystickAxis;
		using (new EditorGUI.DisabledGroupScope (editable == false)) {
			return (JoystickNum)EditorGUILayout.EnumPopup ("Joystick Num", joystickNum);
		}
	}
	#endregion
}
