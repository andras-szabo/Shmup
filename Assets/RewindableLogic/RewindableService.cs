using System;
using UnityEngine;

public class RewindableService : MonoBehaviour
{
	public static RewindableService Instance;

	public event Action OnGhostDisappeared;

	public bool ShouldRewind { get; private set; }

	public bool IsRewindingAllowed { get; private set; } // Can be disallowed e.g. because of ghost
	public bool IsRecordingAllowed { get; private set; } // Can be disallowed e.g. because of ghost
	public bool IsInputRequestingRewind { get; private set; }

	public int RewindableFrameCount { get; private set; }

	public bool holdDoubleTapForRewind;

	private bool _ghostShownInPastUpdate;
	private InputController _inputController;

	private void Awake()
	{
		Instance = this;
	}

	private void CheckInput()
	{
		if (_inputController == null) { _inputController = InputController.Instance; }
		IsInputRequestingRewind = _inputController.HasDoubleTapped;
	}

	private void FixedUpdate()
	{
		CheckInput();
		CheckGhostState();

		IsRewindingAllowed = !Ghost.IsReplaying && RewindableFrameCount >= 0;
		IsRecordingAllowed = !Ghost.IsShown;

		ShouldRewind = IsInputRequestingRewind && IsRewindingAllowed;
	}

	private void CheckGhostState()
	{
		if (!Ghost.IsShown)
		{
			if (_ghostShownInPastUpdate)
			{
				if (OnGhostDisappeared != null)
				{
					OnGhostDisappeared();
				}

				RewindableFrameCount = 0;
			}
			else
			{
				if (!IsInputRequestingRewind)
				{
					RewindableFrameCount = Mathf.Min(RewindableFrameCount + 1, Rewindable.LOG_SIZE_FRAMES);
				}
			}
		}

		if (IsInputRequestingRewind && !Ghost.IsReplaying)
		{
			RewindableFrameCount = Mathf.Max(RewindableFrameCount - 1, 0);
		}

		_ghostShownInPastUpdate = Ghost.IsShown;
	}
}
