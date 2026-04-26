using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameUiOverlayView))]
public class GameUiOverlayViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSection("Canvas", "canvasComponent", "canvasScaler", "graphicRaycaster");
        DrawSection("HUD", "hudRoot", "cheatButton", "pauseButton");
        DrawSection("Pause Panel", "pausePanel", "pauseTitle", "resumeButton", "pauseRetryButton", "pauseBackButton", "musicLabel", "musicSlider", "sfxLabel", "sfxSlider");
        DrawSection("Result Panel", "resultPanel", "resultTitle", "resultSubtitle", "nextLevelButton", "resultRetryButton", "resultBackButton");

        EditorGUILayout.Space();

        GameUiOverlayView view = (GameUiOverlayView)target;
        bool isPersistentAsset = EditorUtility.IsPersistent(view);

        if (isPersistentAsset)
        {
            EditorGUILayout.HelpBox("This is the prefab asset root. To avoid prefab corruption warnings, automatic rebuilding is disabled here. Put the prefab in a scene or open it in Prefab Mode, then use the button below if needed.", MessageType.Warning);
        }

        if (GUILayout.Button("Build Missing UI"))
        {
            Undo.RegisterFullObjectHierarchyUndo(view.gameObject, "Build Game UI");
            view.BuildMissingUi();
            EditorUtility.SetDirty(view);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Pause Panel") && view.PausePanel != null)
            Selection.activeGameObject = view.PausePanel;
        if (GUILayout.Button("Select Result Panel") && view.ResultPanel != null)
            Selection.activeGameObject = view.ResultPanel;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Drag buttons and text in Scene View after the hierarchy exists. Automatic rebuild on validate is disabled to prevent the console errors you saw.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string title, params string[] propertyNames)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach (string propertyName in propertyNames)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property);
        }
        EditorGUILayout.Space(4f);
    }
}
