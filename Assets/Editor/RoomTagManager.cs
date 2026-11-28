using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal sealed class RoomTagManager : EditorWindow
{
    Vector2 scroll;
    string categoryName = "", status = "", newTag = "";
    int activeCategory;
    Color categoryColor = new Color(.62f, .77f, .94f);
    RoomLibraryTags Catalog => RoomLibraryTags.instance;

    internal static void ShowManager()
    {
        var window = GetWindow<RoomTagManager>(true, "Room tags");
        window.minSize = new Vector2(360, 340);
        window.Show();
    }

    void Changed()
    {
        Catalog.Persist();
        foreach (var w in Resources.FindObjectsOfTypeAll<RoomLibraryWindow>())
            w.Refresh();
    }

    void OnGUI()
    {
        GUILayout.Label("Categories", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            categoryName = GUILayout.TextField(categoryName);
            categoryColor = EditorGUILayout.ColorField(categoryColor, GUILayout.Width(55));
            if (GUILayout.Button("Add category", GUILayout.Width(100)))
            {
                var added = Catalog.AddCategory(categoryName, categoryColor);
                if (added == null)
                    status = "Enter a unique category name without a colon.";
                else
                {
                    activeCategory = Catalog.Categories.Count - 1;
                    categoryName = "";
                    status = "";
                    Changed();
                }
            }
        }
        activeCategory = EditorGUILayout.Popup("Edit category", Mathf.Clamp(activeCategory, 0, Catalog.Categories.Count - 1),
            Catalog.Categories.Select(c => c.name).ToArray());
        var category = Catalog.Categories[activeCategory];
        using (new EditorGUILayout.HorizontalScope())
        {
            string name = EditorGUILayout.DelayedTextField(category.name).Trim();
            if (name != category.name)
            {
                if (RoomLibraryTags.ValidName(name) && !Catalog.Categories.Any(c => c != category && string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    category.name = name;
                    status = "";
                    Changed();
                }
                else
                    status = "Category names must be unique.";
            }
            var color = EditorGUILayout.ColorField(category.color, GUILayout.Width(55));
            if (color != category.color)
            {
                category.color = color;
                Changed();
            }
        }
        GUILayout.Space(8);
        GUILayout.Label("Tags", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            newTag = GUILayout.TextField(newTag);
            if (GUILayout.Button("Add tag", GUILayout.Width(78)))
            {
                if (Catalog.AddTag(category, newTag) != null)
                {
                    newTag = "";
                    status = "";
                    Changed();
                }
                else
                    status = "Enter a tag name without a colon.";
            }
        }
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var tag in category.tags)
        {
            string name;
            using (new EditorGUILayout.HorizontalScope())
            {
                name = EditorGUILayout.DelayedTextField(tag.name).Trim();
                if (GUILayout.Button("Merge into…", GUILayout.Width(84)))
                {
                    var menu = new GenericMenu();
                    foreach (var otherCategory in Catalog.Categories)
                        foreach (var target in otherCategory.tags)
                            if (target != tag)
                                menu.AddItem(new GUIContent(otherCategory.name + "/" + target.name), false, () =>
                                {
                                    if (EditorUtility.DisplayDialog("Merge tags", $"Rooms tagged {category.name}:{tag.name} will use {otherCategory.name}:{target.name}.", "Merge", "Cancel"))
                                    {
                                        Catalog.Merge(category, tag, target);
                                        Changed();
                                    }
                                });
                    menu.ShowAsContext();
                }
            }
            if (name == tag.name)
                continue;
            if (!RoomLibraryTags.ValidName(name) || category.tags.Any(t => t != tag && string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase)))
            {
                status = "Tag names must be unique within a category.";
                continue;
            }
            tag.name = name;
            status = "";
            Changed();
        }
        EditorGUILayout.EndScrollView();
        if (status.Length > 0)
            EditorGUILayout.HelpBox(status, MessageType.Info);
    }
}
