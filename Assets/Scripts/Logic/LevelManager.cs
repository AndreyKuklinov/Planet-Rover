using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public event Action<LevelData> LevelFinished;
    public event Action LevelStarted;
    public event Action RoomFinished;
    public LevelData CurrentLevel { get; private set; }
    public int CompletedRoomCount { get; private set; }
    public int FinalStars { get; private set; }
    public bool IsLevelRunning => CurrentLevel != null && !IsRoomTestFinished;
    public bool IsRoomTestFinished { get; private set; }
    public bool IsTestingRoom => CurrentLevel != null && (IsRoomTestSession || CurrentLevel.IsRoomTest);
    public float ElapsedTime => roomTimer.ElapsedTime;
    public float TimeRemaining => roomTimer.TimeRemaining;
    public bool IsRoomTestSession { get; private set; }
    public void BeginRoomTestSession() => IsRoomTestSession = true;
    public float StandardTime => levelLoader.StandardTime * playerTimeMultiplier;
    public int AchievableStars => !IsLevelRunning || roomTimer.HasExpired ? 0
        : CurrentLevel.Mode == LevelMode.Standard ? starConfig.GetStars(ElapsedTime, StandardTime) : 3;
    public float TimeUntilStarLoss => IsLevelRunning && CurrentLevel.Mode == LevelMode.Standard
        ? starConfig.GetTimeUntilStarLoss(ElapsedTime, StandardTime) : float.PositiveInfinity;
    public float GetStarFill(int starNumber)
        => IsLevelRunning && CurrentLevel.Mode == LevelMode.Standard
            ? starConfig.GetStarFill(starNumber, ElapsedTime, StandardTime) : 0f;

    [SerializeField] LevelLoader levelLoader;
    [field: SerializeField] public ObjectiveTracker ObjectiveTracker { get; private set; }
    [SerializeField] RoomTimer roomTimer;
    [SerializeField] StarConfig starConfig;
    [SerializeField] PlayerCountTimeConfig playerCountTimeConfig;
    float playerTimeMultiplier = 1f;
    private bool roomCompletionPending;

    private void OnEnable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled += OnAllObjectivesCompleted;
        roomTimer.TimeOut += OnTimeOut;
    }
    private void OnDisable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled -= OnAllObjectivesCompleted;
        roomTimer.TimeOut -= OnTimeOut;
    }
    public void StartLevel(LevelData level)
    {
        if (level.Mode == LevelMode.Standard && (starConfig == null || !starConfig.IsValid))
            throw new InvalidOperationException("Assign a valid shared Star Config to LevelManager.");
        if (playerCountTimeConfig == null)
            throw new InvalidOperationException("Assign a Player Count Time Config to LevelManager.");
        playerTimeMultiplier = playerCountTimeConfig.GetMultiplier(
            PartyInputManager.Instance == null ? 1 : PartyInputManager.Instance.PlayerCount);
        if (playerTimeMultiplier <= 0f || float.IsNaN(playerTimeMultiplier) || float.IsInfinity(playerTimeMultiplier))
            throw new InvalidOperationException("Player time multipliers must be finite and positive.");
        levelLoader.SetLevelData(level);
        CurrentLevel = level;
        IsRoomTestFinished = false;
        CompletedRoomCount = 0;
        FinalStars = 0;
        roomCompletionPending = false;
        if (IsTestingRoom || level.Mode == LevelMode.Tutorial)
            roomTimer.StartStopwatch();
        else
            roomTimer.StartLevelTime(level.Mode == LevelMode.Challenge ? StandardTime : StandardTime * starConfig.MaximumMultiplier);
        LevelStarted?.Invoke();
        StartNextRoom();
    }
    public void StopCurrentLevel()
    {
        if (!IsLevelRunning)
            throw new InvalidOperationException("No active level.");
        EndLevel(0);
    }
    public void RestartLevel() => StartLevel(CurrentLevel);

    private void StartNextRoom()
    {
        roomCompletionPending = false;
        ObjectiveTracker.ClearObjectives();
        levelLoader.LoadNextRoom();
    }
    private void OnAllObjectivesCompleted() => roomCompletionPending = IsLevelRunning;
    private void OnTimeOut()
    {
        if (IsLevelRunning && !IsTestingRoom)
            EndLevel(0);
    }
    private void LateUpdate()
    {
        // Defer transitions until item interactions and objective registration have finished.
        if (!IsLevelRunning || !roomCompletionPending)
            return;
        roomCompletionPending = false;
        if (ObjectiveTracker.AllObjectives.Count == 0 || ObjectiveTracker.FulfilledProportion < 1f)
            return;
        if (roomTimer.HasExpired)
        {
            EndLevel(0);
            return;
        }
        CompletedRoomCount++;
        RoomFinished?.Invoke();
        if (CompletedRoomCount < CurrentLevel.RoomCountToComplete)
            StartNextRoom();
        else
            EndLevel(AchievableStars);
    }
    private void EndLevel(int stars)
    {
        var level = CurrentLevel;
        bool isRoomTest = IsTestingRoom;
        FinalStars = stars;
        if (isRoomTest)
            IsRoomTestFinished = true;
        else
            CurrentLevel = null;
        roomCompletionPending = false;
        roomTimer.StopTime();
        if (!isRoomTest)
            SaveDataManager.AddStars(level, stars);
        LevelFinished?.Invoke(level);
    }
}
