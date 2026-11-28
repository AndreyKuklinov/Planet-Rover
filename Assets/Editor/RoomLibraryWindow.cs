using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

internal sealed class RoomLibraryWindow : EditorWindow
{
    const string Root = "Assets/Prefabs/Levels";
    [SerializeField] List<Room> selected = new();
    [SerializeField] List<string> filters = new();
    [SerializeField] string search = "", from = "", through = "";
    [SerializeField] bool allRooms, untagged, undated, dimWalls;
    [SerializeField] LevelData level;
    [SerializeField] List<Room> lineup = new(), original = new();
    [SerializeField] bool lineupOpen, pending;
    List<RoomLibraryEntry> entries = new();
    List<RoomLibraryEntry> visible = new();
    readonly Dictionary<Room, List<LevelData>> usedIn = new();
    Vector2 dragStart;
    bool canDrag;
    bool reload = true;
    Vector2 leftScroll, cardsScroll, detailScroll;
    ReorderableList order;
    string tagText = "", status = "";
    RoomLibraryTags Catalog => RoomLibraryTags.instance;
    const float LeftWidth = 190, RightWidth = 258;

    [MenuItem("Window/Level Design/Room Library")]
    static void ShowLibrary() => Open(null);

    internal static void Open(LevelData target)
    {
        var window = GetWindow<RoomLibraryWindow>("Room Library");
        window.Show();
        if (target != null && window.ChangeLevel(target))
            window.lineupOpen = true;
    }

    void OnEnable()
    {
        minSize = new Vector2(820, 460);
        saveChangesMessage = "Apply the edited room lineup to Level Data?";
        hasUnsavedChanges = pending;
        reload = true;
        Undo.undoRedoPerformed += OnUndo;
        EditorApplication.projectChanged += Refresh;
    }
    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndo;
        EditorApplication.projectChanged -= Refresh;
    }
    void OnUndo()
    {
        if (!pending)
            LoadLineup();
        Refresh();
    }
    internal void Refresh()
    {
        reload = true;
        Repaint();
    }

    void Index()
    {
        entries = AssetDatabase.FindAssets("t:Prefab", new[] { Root })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => allRooms || Path.GetDirectoryName(path)?.Replace('\\', '/') == Root + "/dated")
            .Select(path =>
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab != null && prefab.TryGetComponent<Room>(out var r) ? new RoomLibraryEntry(r, path) : null;
            }).Where(e => e != null).OrderByDescending(e => e.date).ThenByDescending(e => e.id, StringComparer.Ordinal).ToList();
        selected.RemoveAll(r => r == null);
        filters = filters.Select(id => Catalog.Find(id)?.id).Where(id => id != null).Distinct().ToList();
        usedIn.Clear();
        foreach (var guid in AssetDatabase.FindAssets("t:LevelData", new[] { "Assets" }))
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data?.Rooms == null)
                continue;
            foreach (var room in data.Rooms.Where(r => r != null).Distinct())
            {
                if (!usedIn.TryGetValue(room, out var levels))
                    usedIn.Add(room, levels = new List<LevelData>());
                levels.Add(data);
            }
        }
        reload = false;
    }

    static bool ParseDate(string value, out DateTime date)
    {
        return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
    void Filter()
    {
        bool hasFrom = ParseDate(from, out var start), hasEnd = ParseDate(through, out var end);
        visible = entries.Where(e => (!undated || !e.date.HasValue)
            && (!hasFrom || e.date >= start) && (!hasEnd || e.date <= end)
            && (!untagged || e.room.LibraryTagIds.Count == 0)
            && e.MatchesTags(Catalog.Categories, filters)
            && (string.IsNullOrWhiteSpace(search) || Contains(e.id, search) || Contains(e.Title, search) || Contains(e.Summary, search))).ToList();
    }
    static bool Contains(string text, string query) => text.IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;

    void OnGUI()
    {
        if (reload && Event.current.type == EventType.Layout)
            Index();
        if (Event.current.type == EventType.Layout)
            Filter();
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Room Library", EditorStyles.boldLabel, GUILayout.Width(95));
            search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(100));
            dimWalls = GUILayout.Toggle(dimWalls, "Dim walls", EditorStyles.toolbarButton);
            lineupOpen = GUILayout.Toggle(lineupOpen, "Level lineup", EditorStyles.toolbarButton);
            if (GUILayout.Button("Manage tags", EditorStyles.toolbarButton))
                RoomTagManager.ShowManager();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftWidth)))
                DrawFilters();
            using (new EditorGUILayout.VerticalScope())
                DrawCards();
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightWidth)))
                DrawDetails();
        }
        if (!string.IsNullOrEmpty(status))
            EditorGUILayout.HelpBox(status, MessageType.Info);
    }

    void DrawFilters()
    {
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
        GUILayout.Label("Browse", EditorStyles.boldLabel);
        bool broad = EditorGUILayout.Popup(allRooms ? 1 : 0, new[] { "Dated collection", "All level folders" }) == 1;
        if (broad != allRooms)
        {
            allRooms = broad;
            Refresh();
        }
        GUILayout.Space(6);
        GUILayout.Label("Creation date", EditorStyles.boldLabel);
        from = DateField("From", from);
        through = DateField("Through", through);
        if ((from.Length > 0 && !ParseDate(from, out _)) || (through.Length > 0 && !ParseDate(through, out _)))
            GUILayout.Label("Use YYYY-MM-DD", EditorStyles.miniLabel);
        undated = GUILayout.Toggle(undated, "Undated only");
        untagged = GUILayout.Toggle(untagged, "Untagged only");
        foreach (var c in Catalog.Categories)
        {
            GUILayout.Space(6);
            ColorLabel(c.name, c.color);
            foreach (var tag in c.tags)
            {
                bool on = filters.Contains(tag.id);
                bool next = GUILayout.Toggle(on, tag.name + "  (" + entries.Count(e => tag.IsAssigned(e.room)) + ")");
                if (on != next)
                {
                    if (next)
                        filters.Add(tag.id);
                    else
                        filters.Remove(tag.id);
                }
            }
        }
        if (GUILayout.Button("Clear filters"))
        {
            filters.Clear();
            untagged = undated = false;
            from = through = search = "";
        }
        EditorGUILayout.EndScrollView();
    }
    static string DateField(string label, string value)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(label, GUILayout.Width(52));
            return GUILayout.TextField(value);
        }
    }
    static void ColorLabel(string text, Color color)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            var r = GUILayoutUtility.GetRect(5, 15, GUILayout.Width(5));
            EditorGUI.DrawRect(r, color);
            GUILayout.Label(text, EditorStyles.boldLabel);
        }
    }

    void DrawCards()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"{visible.Count} rooms · {selected.Count} selected", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select results", EditorStyles.miniButton))
                selected = visible.Select(e => e.room).ToList();
            if (GUILayout.Button("Clear", EditorStyles.miniButton))
                selected.Clear();
        }
        if (filters.Count > 0)
            GUILayout.Label("Any selected tag per category · All selected categories", EditorStyles.miniLabel);
        cardsScroll = EditorGUILayout.BeginScrollView(cardsScroll);
        int columns = Mathf.Max(1, Mathf.FloorToInt((position.width - LeftWidth - RightWidth - 32) / 165));
        const int height = 145;
        var area = GUILayoutUtility.GetRect(0, Mathf.CeilToInt(visible.Count / (float)columns) * height, GUILayout.ExpandWidth(true));
        int first = Mathf.Max(0, Mathf.FloorToInt(cardsScroll.y / height) - 1) * columns;
        int limit = Mathf.Min(visible.Count, first + (Mathf.CeilToInt(position.height / height) + 3) * columns);
        float pitch = area.width / columns;
        for (int i = first; i < limit; i++)
        {
            var entry = visible[i];
            var card = new Rect(area.x + i % columns * pitch + 2, area.y + i / columns * height + 2, pitch - 4, height - 4);
            GUI.Box(card, GUIContent.none);
            if (selected.Contains(entry.room))
                EditorGUI.DrawRect(card, new Color(.24f, .53f, .83f, .33f));
            entry.Draw(new Rect(card.x + 4, card.y + 4, card.width - 8, 83), dimWalls);
            GUI.Label(new Rect(card.x + 5, card.y + 88, card.width - 10, 18), new GUIContent(entry.Title, entry.id), EditorStyles.boldLabel);
            GUI.Label(new Rect(card.x + 5, card.y + 106, card.width - 10, 16), entry.Title == entry.id ? entry.Size : entry.id + " · " + entry.Size, EditorStyles.miniLabel);
            float chipX = card.x + 5;
            foreach (var c in Catalog.Categories)
                foreach (var t in c.tags.Where(t => t.IsAssigned(entry.room)))
                {
                    var label = new GUIContent(c.name + ":" + t.name);
                    float width = EditorStyles.miniLabel.CalcSize(label).x + 8;
                    var chip = new Rect(chipX, card.y + 122, Mathf.Max(0, Mathf.Min(width, card.xMax - chipX - 4)), 15);
                    if (chip.width > 6)
                    {
                        EditorGUI.DrawRect(chip, new Color(c.color.r, c.color.g, c.color.b, .35f));
                        GUI.Label(chip, label, EditorStyles.miniLabel);
                    }
                    chipX += width + 3;
                }
            var ev = Event.current;
            if (ev.type == EventType.MouseDown && ev.button == 0 && card.Contains(ev.mousePosition))
            {
                if (ev.clickCount == 2)
                {
                    AssetDatabase.OpenAsset(entry.room.gameObject);
                    ev.Use();
                    continue;
                }
                if (ev.shift && selected.Count > 0)
                {
                    int anchor = visible.FindIndex(e => e.room == selected[selected.Count - 1]);
                    if (anchor >= 0)
                    foreach (var e in visible.GetRange(Math.Min(i, anchor), Math.Abs(i - anchor) + 1))
                        if (!selected.Contains(e.room))
                            selected.Add(e.room);
                }
                else if (ev.control || ev.command)
                {
                    if (!selected.Remove(entry.room))
                        selected.Add(entry.room);
                }
                else if (!selected.Contains(entry.room) || selected.Count == 1)
                {
                    selected.Clear();
                    selected.Add(entry.room);
                }
                dragStart = ev.mousePosition;
                canDrag = true;
                status = "";
                ev.Use();
                Repaint();
            }
            if (ev.type == EventType.MouseUp && card.Contains(ev.mousePosition) && canDrag)
            {
                if (!ev.control && !ev.command && !ev.shift)
                {
                    selected.Clear();
                    selected.Add(entry.room);
                }
                canDrag = false;
                ev.Use();
                Repaint();
            }
            if (ev.type == EventType.MouseDrag && canDrag && (ev.mousePosition - dragStart).sqrMagnitude > 16)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = selected.Select(r => (UnityEngine.Object)r.gameObject).ToArray();
                DragAndDrop.StartDrag("Rooms");
                canDrag = false;
                ev.Use();
            }
        }
        if (visible.Count == 0)
            EditorGUILayout.HelpBox("No rooms match these filters.", MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    void DrawDetails()
    {
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        if (selected.Count > 0)
        {
            if (selected.Count == 1)
            {
                var room = selected[0];
                var entry = entries.Find(e => e.room == room);
                if (entry != null)
                {
                    GUILayout.Label(entry.id, EditorStyles.boldLabel);
                    entry.Draw(GUILayoutUtility.GetRect(100, 105, GUILayout.ExpandWidth(true)), dimWalls);
                    GUILayout.Label(entry.Size + " · " + entry.Summary, EditorStyles.wordWrappedMiniLabel);
                }
                string title = EditorGUILayout.DelayedTextField(new GUIContent("Display title"), room.LibraryTitle ?? "");
                if (title != (room.LibraryTitle ?? ""))
                    EditRoom(room, so => so.FindProperty("libraryTitle").stringValue = title.Trim());
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Edit prefab"))
                        AssetDatabase.OpenAsset(room.gameObject);
                    using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                        if (GUILayout.Button("Test Room"))
                            RoomTestRunner.Launch(room);
                }
                if (usedIn.TryGetValue(room, out var levels))
                {
                    GUILayout.Label("Used in", EditorStyles.miniBoldLabel);
                    foreach (var asset in levels)
                        if (GUILayout.Button(asset.name, EditorStyles.linkLabel) && ChangeLevel(asset))
                            lineupOpen = true;
                }
            }
            DrawTagAssignment();
        }
        else
            GUILayout.Label("Select rooms to tag or add.", EditorStyles.wordWrappedMiniLabel);
        if (lineupOpen)
            DrawLineup();
        EditorGUILayout.EndScrollView();
    }

    static void EditRoom(Room room, Action<SerializedObject> edit)
    {
        string path = AssetDatabase.GetAssetPath(room.gameObject);
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        bool open = stage != null && stage.assetPath == path;
        if (open)
        {
            var staged = new SerializedObject(stage.prefabContentsRoot.GetComponent<Room>());
            edit(staged);
            if (staged.ApplyModifiedProperties())
                EditorSceneManager.MarkSceneDirty(stage.scene);
        }
        var so = new SerializedObject(room);
        edit(so);
        if (!so.ApplyModifiedProperties())
            return;
        PrefabUtility.SavePrefabAsset(room.gameObject);
    }
    internal static void Assign(IEnumerable<Room> rooms, string id, bool add)
    {
        var tag = RoomLibraryTags.instance.Find(id);
        if (tag == null)
            return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(add ? "Tag rooms" : "Remove room tag");
        foreach (var room in rooms)
            EditRoom(room, so => tag.SetAssigned(so, add));
        Undo.CollapseUndoOperations(group);
    }

    void DrawTagAssignment()
    {
        GUILayout.Space(6);
        GUILayout.Label($"Tag {selected.Count} selected", EditorStyles.boldLabel);
        if (GUILayout.Button("Add or remove tags…"))
        {
            var targets = selected.ToArray();
            Catalog.ShowAssignmentMenu(targets.Length, tag => targets.Count(tag.IsAssigned), (tag, add) =>
            {
                Assign(targets, tag.id, add);
                Refresh();
            });
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            tagText = GUILayout.TextField(tagText);
            if (GUILayout.Button("Tag", GUILayout.Width(38)))
            {
                var parts = tagText.Split(':');
                var cat = Catalog.Categories.FirstOrDefault(c => c.name.Equals(parts[0].Trim(), StringComparison.OrdinalIgnoreCase));
                if (parts.Length != 2 || cat == null || !RoomLibraryTags.ValidName(parts[1]))
                    status = "Enter category:tag. Create categories in Manage tags.";
                else
                {
                    var tag = Catalog.AddTag(cat, parts[1]);
                    Assign(selected, tag.id, true);
                    tagText = "";
                    status = "";
                    Refresh();
                }
            }
        }
        GUILayout.Label("Quick add or create · category:tag", EditorStyles.miniLabel);
    }

    bool ChangeLevel(LevelData next)
    {
        if (next == level)
            return true;
        if (pending)
        {
            int choice = EditorUtility.DisplayDialogComplex("Change level", "Apply your current lineup before switching levels?", "Apply", "Cancel", "Discard");
            if (choice == 1 || (choice == 0 && !ApplyLineup()))
                return false;
        }
        level = next;
        LoadLineup();
        return true;
    }
    void LoadLineup()
    {
        lineup = level != null && level.Rooms != null ? level.Rooms.ToList() : new List<Room>();
        original = new List<Room>(lineup);
        order = null;
        pending = hasUnsavedChanges = false;
    }
    void MarkLineup() => hasUnsavedChanges = pending = !original.SequenceEqual(lineup);
    internal bool ApplyLineup()
    {
        if (level == null || lineup.Count == 0 || lineup.Contains(null))
        {
            status = "Choose a level and at least one valid room.";
            return false;
        }
        if (!(level.Rooms ?? Array.Empty<Room>()).SequenceEqual(original))
        {
            status = "The level changed externally. Reload its lineup before applying.";
            return false;
        }
        var so = new SerializedObject(level);
        var rooms = so.FindProperty("<Rooms>k__BackingField");
        rooms.arraySize = lineup.Count;
        for (int i = 0; i < lineup.Count; i++)
            rooms.GetArrayElementAtIndex(i).objectReferenceValue = lineup[i];
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssetIfDirty(level);
        LoadLineup();
        status = "Lineup saved.";
        return true;
    }
    public override void SaveChanges()
    {
        if (ApplyLineup())
            base.SaveChanges();
    }
    public override void DiscardChanges()
    {
        LoadLineup();
        base.DiscardChanges();
    }

    void DrawLineup()
    {
        GUILayout.Space(8);
        GUILayout.Label("Level lineup", EditorStyles.boldLabel);
        var next = (LevelData)EditorGUILayout.ObjectField(level, typeof(LevelData), false);
        if (next != level)
            ChangeLevel(next);
        if (GUILayout.Button("New Level Data…"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Level Data", "New Level", "asset", "Save the level asset.", Root + "/dated/level data");
            if (!string.IsNullOrEmpty(path) && ChangeLevel(null))
            {
                if (File.Exists(path))
                {
                    status = "Choose an unused filename.";
                    return;
                }
                var asset = CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(asset, path);
                ChangeLevel(asset);
            }
        }
        if (level == null)
            return;
        LevelDataEditor.DrawTags(new[] { level });
        if (!pending && !(level.Rooms ?? Array.Empty<Room>()).SequenceEqual(original))
            LoadLineup();
        if (order == null)
        {
            order = new ReorderableList(lineup, typeof(Room), true, true, false, true);
            order.drawHeaderCallback = r => EditorGUI.LabelField(r, $"{lineup.Count} rooms");
            order.drawElementCallback = (r, i, active, focused) =>
            {
                var room = lineup[i];
                var entry = entries.Find(e => e.room == room);
                entry?.Draw(new Rect(r.x, r.y, 34, r.height - 1), dimWalls);
                EditorGUI.LabelField(new Rect(r.x + 38, r.y, r.width - 38, r.height), room == null ? "Missing room" : entry?.Title ?? room.name);
            };
            order.onRemoveCallback = l => { lineup.RemoveAt(l.index); l.index = Math.Min(l.index, lineup.Count - 1); MarkLineup(); };
            order.onReorderCallback = l => MarkLineup();
        }
        using (new EditorGUI.DisabledScope(!selected.Any(r => !lineup.Contains(r))))
            if (GUILayout.Button($"Add selected ({selected.Count})"))
            {
                foreach (var r in selected)
                if (!lineup.Contains(r))
                    lineup.Add(r);
                MarkLineup();
            }
        var dropArea = GUILayoutUtility.GetRect(40, 26, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop room prefabs here", EditorStyles.helpBox);
        var ev = Event.current;
        if (dropArea.Contains(ev.mousePosition) && (ev.type == EventType.DragUpdated || ev.type == EventType.DragPerform))
        {
            var dragged = DragAndDrop.objectReferences.OfType<GameObject>()
                .Where(go => AssetDatabase.IsMainAsset(go)).Select(go => go.GetComponent<Room>())
                .Where(r => r != null && !lineup.Contains(r)).Distinct().ToList();
            DragAndDrop.visualMode = dragged.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (ev.type == EventType.DragPerform && dragged.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                lineup.AddRange(dragged);
                MarkLineup();
            }
            ev.Use();
        }
        order.DoLayoutList();
        GUILayout.Label(level.IsRandomOrder ? "Random order" : "Listed order", EditorStyles.miniLabel);
        var settings = new SerializedObject(level);
        bool all = settings.FindProperty("beatAllRooms").boolValue;
        int count = settings.FindProperty("roomCountToComplete").intValue;
        GUILayout.Label(all ? "Complete all rooms" : $"Complete {count} rooms" + (count > lineup.Count ? " · rooms repeat" : ""), EditorStyles.miniLabel);
        if (GUILayout.Button("Edit level settings", EditorStyles.linkLabel))
            Selection.activeObject = level;
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!pending || lineup.Count == 0 || lineup.Contains(null)))
                if (GUILayout.Button("Apply lineup"))
                    ApplyLineup();
            if (GUILayout.Button("Reload lineup"))
                LoadLineup();
        }
    }
}
