using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomResetController : MonoBehaviour
{
    public event Action<Room> RoomReset;
    public float HoldProgress { get; private set; }

    RoomLoader loader;
    RoomResetConfig config;
    Room snapshot;
    GameObject snapshotContainer;
    readonly List<PreserveOnRoomReset> preserved = new();
    float heldTime;
    bool waitingForRelease;
    int rosterVersion = -1;

    public void Initialize(RoomLoader roomLoader, RoomResetConfig settings)
    {
        Room.RoomStarted -= CaptureRoom;
        loader = roomLoader;
        config = settings;
        Room.RoomStarted += CaptureRoom;
    }

    void OnDestroy()
    {
        Room.RoomStarted -= CaptureRoom;
        if (snapshotContainer != null) Destroy(snapshotContainer);
    }

    public void CaptureRoom(Room room)
    {
        if (loader == null || room != loader.CurrentRoom) return;
        ClearSnapshot();
        foreach (var marker in room.GetComponentsInChildren<PreserveOnRoomReset>(true))
            if (marker.transform.parent.GetComponentInParent<PreserveOnRoomReset>() == null)
                preserved.Add(marker);

        snapshotContainer = new GameObject("Room Reset Snapshot");
        snapshotContainer.transform.SetParent(transform, false);
        snapshotContainer.SetActive(false);
        snapshot = Instantiate(room, snapshotContainer.transform);
        foreach (var marker in snapshot.GetComponentsInChildren<PreserveOnRoomReset>(true))
        {
            marker.gameObject.SetActive(false);
            Destroy(marker.gameObject);
        }
        heldTime = 0f;
        HoldProgress = 0f;
        // Do not reset immediately when entering a room with drop already held.
        waitingForRelease = true;
    }

    public void ClearSnapshot()
    {
        if (snapshotContainer != null) Destroy(snapshotContainer);
        snapshot = null;
        snapshotContainer = null;
        preserved.Clear();
        heldTime = HoldProgress = 0f;
    }

    void Update()
    {
        var input = PartyInputManager.Instance;
        if (snapshot == null || config == null || loader.CurrentRoom == null
            || Time.timeScale <= 0f || input == null)
        {
            heldTime = HoldProgress = 0f;
            waitingForRelease = true;
            return;
        }
        TickHold(input.AllPlayersHoldingDrop, input.AnyPlayerHoldingDrop,
            input.RosterVersion, Time.deltaTime);
    }

    public void TickHold(bool allHeld, bool anyHeld, int currentRosterVersion, float deltaTime)
    {
        if (rosterVersion != currentRosterVersion)
        {
            rosterVersion = currentRosterVersion;
            heldTime = HoldProgress = 0f;
        }
        if (waitingForRelease)
        {
            if (!anyHeld) waitingForRelease = false;
            return;
        }
        if (!allHeld || snapshot == null || config == null)
        {
            heldTime = HoldProgress = 0f;
            return;
        }
        heldTime += Mathf.Max(0f, deltaTime);
        HoldProgress = Mathf.Clamp01(heldTime / Mathf.Max(0.01f, config.HoldDuration));
        if (HoldProgress < 1f) return;
        waitingForRelease = true;
        heldTime = HoldProgress = 0f;
        ResetRoom();
    }

    public void ResetRoom()
    {
        if (snapshot == null || loader.CurrentRoom == null) return;
        var oldRoom = loader.CurrentRoom;
        var oldGrid = oldRoom.Grid;
        var livePreserved = new List<(PreserveOnRoomReset marker, GridObject obj, bool registered)>();
        foreach (var marker in preserved)
        {
            if (marker == null) continue;
            var obj = marker.GetComponent<GridObject>();
            livePreserved.Add((marker, obj, obj != null && oldGrid.Objects.Contains(obj)));
            if (obj != null) oldGrid.ForgetObject(obj);
            marker.transform.SetParent(transform, true);
        }

        oldRoom.gameObject.SetActive(false);
        var restored = Instantiate(snapshot, snapshotContainer.transform);
        restored.PrepareForReset();
        restored.transform.SetParent(oldRoom.transform.parent, true);
        loader.SetResetRoom(restored);
        restored.Grid.AttachAllObjects();
        foreach (var entry in livePreserved)
        {
            entry.marker.transform.SetParent(restored.Grid.transform, true);
            if (entry.obj == null) continue;
            entry.obj.RebindGrid(restored.Grid);
            if (entry.registered) restored.Grid.TrackObject(entry.obj);
        }
        Destroy(oldRoom.gameObject);
        RoomReset?.Invoke(restored);
    }
}
