using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[FilePath("ProjectSettings/RoomLibraryTags.asset", FilePathAttribute.Location.ProjectFolder)]
internal sealed class RoomLibraryTags : ScriptableSingleton<RoomLibraryTags>
{
    [Serializable]
    internal sealed class Tag
    {
        public string id, name;
        [SerializeField] List<string> formerIds = new();
        public Tag(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public bool Owns(string storedId) => storedId == id || formerIds.Contains(storedId);
        public bool IsAssigned(Room room) => room.LibraryTagIds.Any(Owns);
        public bool IsAssigned(LevelData level) => level.LibraryTagIds.Any(Owns);

        public void SetAssigned(SerializedObject asset, bool add)
        {
            var ids = asset.FindProperty("libraryTagIds");
            for (int i = ids.arraySize - 1; i >= 0; i--)
            {
                if (!Owns(ids.GetArrayElementAtIndex(i).stringValue)) continue;
                if (add) return;
                ids.DeleteArrayElementAtIndex(i);
            }
            if (add) ids.GetArrayElementAtIndex(ids.arraySize++).stringValue = id;
        }

        public void Absorb(Tag source)
        {
            formerIds.Add(source.id);
            formerIds.AddRange(source.formerIds);
        }
    }

    [Serializable]
    internal sealed class Category
    {
        public string name;
        public Color color;
        public List<Tag> tags = new();

        public Category(string name, Color color, params string[] names)
        {
            this.name = name;
            this.color = color;
            foreach (string label in names)
                tags.Add(new Tag(name + "." + label, label));
        }
    }

    [SerializeField]
    List<Category> categories = new()
    {
        new Category("layout", new Color(.52f,.74f,.98f), "big", "open", "partition", "ring"),
        new Category("mechanic", new Color(.52f,.83f,.66f), "planter", "dye-cloud", "timed-door"),
        new Category("task", new Color(.98f,.77f,.45f), "parallel", "handoff", "choose-a-route", "carry-together")
    };
    internal IReadOnlyList<Category> Categories => categories;
    internal void Persist() => Save(true);
    internal Tag Find(string id) => categories.SelectMany(c => c.tags).FirstOrDefault(t => t.Owns(id));

    internal void ShowAssignmentMenu(int selectionCount, Func<Tag, int> assignedCount, Action<Tag, bool> assign)
    {
        var menu = new GenericMenu();
        foreach (var category in categories)
        foreach (var tag in category.tags)
        {
            string label = category.name + "/" + tag.name;
            int count = assignedCount(tag);
            if (count < selectionCount)
                menu.AddItem(new GUIContent("Add/" + label), false, () => assign(tag, true));
            if (count > 0)
                menu.AddItem(new GUIContent("Remove/" + label), false, () => assign(tag, false));
        }
        menu.ShowAsContext();
    }

    internal void Merge(Category sourceCategory, Tag source, Tag target)
    {
        if (source == target)
            return;
        target.Absorb(source);
        sourceCategory.tags.Remove(source);
        Persist();
    }
    static bool Equal(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    internal static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name) && !name.Contains(':');

    internal Category AddCategory(string name, Color color)
    {
        name = name.Trim();
        if (!ValidName(name) || categories.Any(c => Equal(c.name, name)))
            return null;
        var category = new Category(name, color);
        categories.Add(category);
        Persist();
        return category;
    }

    internal Tag AddTag(Category category, string name)
    {
        name = name.Trim();
        if (!ValidName(name))
            return null;
        var existing = category.tags.Find(t => Equal(t.name, name));
        if (existing != null)
            return existing;
        var tag = new Tag(Guid.NewGuid().ToString("N"), name);
        category.tags.Add(tag);
        Persist();
        return tag;
    }
}
