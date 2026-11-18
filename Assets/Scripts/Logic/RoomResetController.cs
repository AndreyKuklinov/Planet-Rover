using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class RoomResetController : MonoBehaviour
{
    const float IndicatorHeight = 0.65f;
    public event Action<Room> RoomReset;

    RoomLoader loader;
    RoverHandConfig config;
    Room snapshot;
    GameObject snapshotContainer;
    readonly List<PreserveOnRoomReset> preserved = new();
    Rover rover;
    HandRenderer[] hands;
    RectTransform indicatorRect;
    HandResetProgressIndicator resetIndicator;
    float chargeTime;
    bool isCharging;
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
        if (indicatorRect != null) Destroy(indicatorRect.gameObject);
    }

    public void CaptureRoom(Room room)
    {
        if (loader == null || room != loader.CurrentRoom) return;
        ClearSnapshot();
        if (!room.CanUseReset) return;
        CacheRover(room);
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
        rover = null;
        hands = null;
        ClearCharge();
    }

    void CacheRover(Room room)
    {
        rover = room.GetComponentInChildren<Rover>();
        hands = rover != null ? rover.GetComponentsInChildren<HandRenderer>() : null;
    }

    void OnDisable() => ClearCharge();

    void ClearCharge()
    {
        chargeTime = 0f;
        isCharging = false;
        if (indicatorRect != null) indicatorRect.gameObject.SetActive(false);
    }

    bool CanCharge(PartyInputManager input)
    {
        if (snapshot == null || config == null || loader.CurrentRoom == null || !loader.CurrentRoom.CanUseReset
            || rover == null || hands == null || hands.Length == 0 || Time.timeScale <= 0f
            || input == null || !input.AllPlayersHoldingDrop)
            return false;

        foreach (var hand in hands)
        {
            if (hand == null) return false;
            int playerIndex = input.DirectionToPlayerIndex[hand.Direction];
            if (playerIndex >= input.PlayerCount || !hand.IsFullyTuckedBy(input.Players[playerIndex]))
                return false;
        }
        return true;
    }

    void LateUpdate()
    {
        var input = PartyInputManager.Instance;
        if (input != null && rosterVersion != input.RosterVersion)
        {
            ClearCharge();
            rosterVersion = input.RosterVersion;
        }

        if (!CanCharge(input))
        {
            ClearCharge();
            return;
        }

        if (isCharging) chargeTime += Time.deltaTime;
        isCharging = true;
        float duration = Mathf.Max(0.01f, config.RoomResetHoldDuration);
        if (chargeTime >= duration)
            ResetRoom();
        else
            ShowCharge(chargeTime / duration);
    }

    void ShowCharge(float progress)
    {
        if (resetIndicator == null)
        {
            var indicator = new GameObject("Room Reset Charge", typeof(RectTransform), typeof(Canvas));
            indicator.transform.SetParent(transform, false);
            indicatorRect = (RectTransform)indicator.transform;
            var canvas = indicator.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 1;
            var fill = new GameObject("Progress", typeof(RectTransform));
            fill.transform.SetParent(indicator.transform, false);
            resetIndicator = fill.AddComponent<HandResetProgressIndicator>();
            resetIndicator.color = Color.white;
            resetIndicator.raycastTarget = false;
        }

        indicatorRect.sizeDelta = resetIndicator.rectTransform.sizeDelta = Vector2.one * config.RoomResetIndicatorSize;
        indicatorRect.SetPositionAndRotation(rover.transform.position + Vector3.up * IndicatorHeight, Quaternion.identity);
        resetIndicator.SetProgress(progress);
        indicatorRect.gameObject.SetActive(true);
    }

    public void ResetRoom()
    {
        if (snapshot == null || loader.CurrentRoom == null || !loader.CurrentRoom.CanUseReset) return;
        ClearCharge();
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
        CacheRover(restored);
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
