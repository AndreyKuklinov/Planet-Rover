using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomResetController : MonoBehaviour
{
    public event Action<Room> RoomReset;

    RoomLoader loader;
    RoverHandConfig config;
    Room snapshot;
    GameObject snapshotContainer;
    readonly List<PreserveOnRoomReset> preserved = new();
    sealed class HandCharge
    {
        public float HeldTime;
        public float Progress;
        public bool IsCharging;
        public bool WaitingForRelease = true;
    }
    readonly Dictionary<PlayerDevice, HandCharge> charges = new();
    int rosterVersion = -1;

    public void Initialize(RoomLoader roomLoader, RoverHandConfig settings)
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
        if (!room.CanUseReset) return;
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
    }

    public void ClearSnapshot()
    {
        if (snapshotContainer != null) Destroy(snapshotContainer);
        snapshot = null;
        snapshotContainer = null;
        preserved.Clear();
        ClearCharges();
    }

    void OnDisable() => ClearCharges();

    void ClearCharges()
    {
        charges.Clear();
        rosterVersion = -1;
    }

    public bool TryGetHandCharge(Direction direction, out float progress)
    {
        progress = 0f;
        var input = PartyInputManager.Instance;
        if (!isActiveAndEnabled || input == null || rosterVersion != input.RosterVersion || snapshot == null || loader.CurrentRoom == null
            || !loader.CurrentRoom.CanUseReset || Time.timeScale <= 0f) return false;
        int index = input.DirectionToPlayerIndex[direction];
        if (index < 0 || index >= input.PlayerCount) return false;
        var player = input.Players[index];
        if (player == null || !player.IsDropHeld || !charges.TryGetValue(player, out var charge)
            || !charge.IsCharging) return false;
        progress = charge.Progress;
        return true;
    }

    void Update()
    {
        var input = PartyInputManager.Instance;
        if (snapshot == null || config == null || loader.CurrentRoom == null
            || Time.timeScale <= 0f || input == null)
        {
            ClearCharges();
            return;
        }
        TickHold(input.Players, input.RosterVersion, Time.deltaTime);
    }

    public void TickHold(IReadOnlyList<PlayerDevice> players, int currentRosterVersion, float deltaTime)
    {
        if (snapshot == null || config == null || loader.CurrentRoom == null || !loader.CurrentRoom.CanUseReset)
        {
            ClearCharges();
            return;
        }
        if (rosterVersion != currentRosterVersion)
        {
            ClearCharges();
            rosterVersion = currentRosterVersion;
        }
        bool allReady = players.Count > 0;
        foreach (var player in players)
        {
            if (player == null) { allReady = false; continue; }
            if (!charges.TryGetValue(player, out var charge))
            {
                charge = new HandCharge();
                charges.Add(player, charge);
            }
            if (!player.IsDropHeld)
            {
                charge.HeldTime = charge.Progress = 0f;
                charge.IsCharging = charge.WaitingForRelease = false;
                allReady = false;
                continue;
            }
            // Each player must release once after entry, a roster change, or a reset.
            if (charge.WaitingForRelease) { allReady = false; continue; }
            charge.HeldTime += Mathf.Max(0f, deltaTime);
            charge.IsCharging = charge.HeldTime >= Mathf.Max(0f, config.RoomResetHoldThreshold);
            charge.Progress = Mathf.Clamp01((charge.HeldTime - Mathf.Max(0f, config.RoomResetHoldThreshold))
                / Mathf.Max(0.01f, config.RoomResetHoldDuration));
            if (!charge.IsCharging || charge.Progress < 1f) allReady = false;
        }
        if (allReady) ResetRoom();
    }

    public void ResetRoom()
    {
        if (snapshot == null || loader.CurrentRoom == null || !loader.CurrentRoom.CanUseReset) return;
        ClearCharges();
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
