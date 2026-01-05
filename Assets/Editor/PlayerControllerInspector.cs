using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerInspector : Editor
{
    public VisualTreeAsset inspectorUXML;

    private VisualElement sprintTab, jumpTab, crouchTab, dashTab;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        inspectorUXML.CloneTree(root);

        SerializedProperty sprintProp = serializedObject.FindProperty("isSprintActive");
        SerializedProperty jumpProp = serializedObject.FindProperty("isJumpActive");
        SerializedProperty crouchProp = serializedObject.FindProperty("isCrouchActive");
        SerializedProperty dashProp = serializedObject.FindProperty("isDashActive");

        sprintTab = root.Q<VisualElement>("SprintTab");
        jumpTab = root.Q<VisualElement>("JumpTab");
        crouchTab = root.Q<VisualElement>("CrouchTab");
        dashTab = root.Q<VisualElement>("DashTab");

        void UpdateTabs()
        {
            serializedObject.Update();

            sprintTab.SetEnabled(sprintProp.boolValue);
            jumpTab.SetEnabled(jumpProp.boolValue);
            crouchTab.SetEnabled(crouchProp.boolValue);
            dashTab.SetEnabled(dashProp.boolValue);
        }

        UpdateTabs();

        root.Bind(serializedObject);
        root.TrackSerializedObjectValue(serializedObject, _ => UpdateTabs());

        return root;
    }
}
