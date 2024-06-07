using UnityEditor;
using UnityEngine;

public class MPCTargetCheckerWindow : EditorWindow {
    private static int panelID = 0;


    #region Public Static
    public static void ShowWindow(int min, int target) {
        panelID = 0;
        var instance = GetWindow<MPCTargetCheckerWindow>("Target API check", true);
        instance.minSize = new Vector2(400, 200);
        instance.Show();
    }
    public static void ShowDirectoriesDeletedWindow() {
        panelID = 1;
        var instance = GetWindow<MPCTargetCheckerWindow>("Directories Deleted", true);
        instance.minSize = new Vector2(400, 200);
        instance.Show();
    } 
    #endregion



    private void OnGUI() {
        GUILayout.Space(20);
        if (panelID == 0) {
            GUILayout.Label("Your API levels were not correct", EditorStyles.boldLabel);
            GUILayout.Space(20);
            GUILayout.Label($"We have updated your target API to 33");
            GUILayout.Label("We have updated your min API to 24 ");
        }
        else {
            GUILayout.Label("Old Folders were deleted from this project", EditorStyles.boldLabel);
        }

        GUILayout.Space(20);
    }
}