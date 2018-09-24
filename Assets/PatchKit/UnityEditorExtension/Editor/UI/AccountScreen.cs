using PatchKit.UnityEditorExtension.Core;
using UnityEditor;
using UnityEngine;

namespace PatchKit.UnityEditorExtension.UI
{
public class AccountScreen : Screen
{
    #region GUI

    public override string Title
    {
        get { return "Account"; }
    }

    public override Vector2? Size
    {
        get { return new Vector2(400, 145); }
    }

    public override void UpdateIfActive()
    {
        if (!IsAccountLinked)
        {
            Push<NotLinkedAccountScreen>().Initialize();
        }
    }

    public override void Draw()
    {
        GUILayout.Label(
            "You have successfully linked your PatchKit account.",
            EditorStyles.boldLabel);

        GUILayout.Label("API key:");
        EditorGUILayout.SelectableLabel(ApiKey, EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Change", GUILayout.Width(100)))
            {
                Dispatch(() => OpenLinkDialog());
            }

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Unlink", GUILayout.Width(100)))
            {
                Dispatch(() => Unlink());
            }

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
    }

    #endregion

    #region Logic

    public void Initialize()
    {
    }

    public override void OnActivatedFromTop(object result)
    {
    }

    private string ApiKey
    {
        get
        {
            ApiKey? value = Config.GetLinkedAccountApiKey();

            return value.HasValue ? value.Value.Value : string.Empty;
        }
    }

    private bool IsAccountLinked
    {
        get { return Config.GetLinkedAccountApiKey().HasValue; }
    }

    private void OpenLinkDialog()
    {
        var screen = Push<LinkAccountScreen>();
        screen.Initialize();
    }

    private void Unlink()
    {
        if (EditorUtility.DisplayDialog(
            "Are you sure?",
            "Are you sure that you want to unlink your PatchKit account from this project?\n\nThis operation cannot be undone.",
            "Yes",
            "No"))
        {
            Config.UnlinkAccount();
        }
    }

    #endregion
}
}