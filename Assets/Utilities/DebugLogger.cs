using RingBuffer;
using UnityEngine.UI;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
	public enum FPSQuality
	{
		None,
		Low,
		Mid,
		High
	}

	public const int MEMORY_SAMPLE_PER_FRAME = 60;
	public const int FPS_HISTOGRAM_COUNT = 60;
	public const float FPS_LOW = 24f;
	public const float FPS_MID = 35f;

	public Text fpsLabel;
	public Text memLabel;
	public Text entityLabel;
	public GameObject rewindMarker;

	public Color fpsLowColor, fpsMidColor, fpsHighColor;
	public Image fpsIndicatorImage;
	public Material fpsHistogramMaterial;
	public Material fpsDistributionMaterial;

	private bool _tookFirstMeasurement;
	private float _fpsMin, _fpsMax, _fpsAvg;

	private int _fpsLowCount, _fpsMidCount, _fpsHighCount;

	private RingBuffer<float> _pastFps = new RingBuffer<float>(FPS_HISTOGRAM_COUNT);
	private float[] _fpsArray = new float[FPS_HISTOGRAM_COUNT];
	private Color[] _colArray = new Color[FPS_HISTOGRAM_COUNT];
	
	private int _framesLeftUntilMemorySample = MEMORY_SAMPLE_PER_FRAME;

	public void Reset()
	{
		_tookFirstMeasurement = false;
		_pastFps.Clear(deallocate: true);
		_fpsLowCount = 1;
		_fpsMidCount = 1;
		_fpsHighCount = 1;
	}

	private void Update()
	{
		RecordFPSData(Time.deltaTime);
		UpdateFPSLabel();
		UpdateMemoryLabelIfNeeded();
		UpdateEntityLabel();
		UpdateRewindMarker();
	}

	private void UpdateRewindMarker()
	{
		rewindMarker.gameObject.SetActive(RewindableService.Instance.ShouldRewind);
	}

	private void UpdateEntityLabel()
	{
		entityLabel.text = string.Format("E: {0} TR: {1} BUL: {2} PS: {3} RFC: {4}", GenericPool.pooledObjectCount,
																		DataPoolContainer.Instance.TransformDataPool.AvailableCount,
																		TransformSystem.Instance.InUseCount,
																		DataPoolContainer.Instance.PlayerShipDataPool.AvailableCount,
																		RewindableService.Instance.RewindableFrameCount);
	}

	private void UpdateMemoryLabelIfNeeded()
	{
		if (_framesLeftUntilMemorySample-- == 0)
		{
			var mem = System.GC.GetTotalMemory(false);
			memLabel.text = string.Format("Managed heap size: {0} Mb", mem / 1024 / 1024);
			_framesLeftUntilMemorySample = MEMORY_SAMPLE_PER_FRAME;
		}
	}

	private void UpdateFPSLabel()
	{
		fpsLabel.text = string.Format("FPS: min {0:F1} / max {1:F1} / avg {2:F1}",
									_fpsMin, _fpsMax, _fpsAvg);
	}

	private void RecordFPSData(float deltaT)
	{
		var current = 1f / deltaT;
		if (!_tookFirstMeasurement)
		{
			TakeFirstMeasurement(current);
		}
		else
		{
			_fpsMin = Mathf.Min(_fpsMin, current);
			_fpsMax = Mathf.Min(Mathf.Max(_fpsMax, current), 60f);
			_fpsAvg = (_fpsAvg + current) / 2f;
		}

		ShowFpsHistogram(current);
		ShowDistributionOverTime(current);
	}

	private void ShowFpsHistogram(float currentFPS)
	{
		_pastFps.Push(currentFPS);
		_pastFps.ToArray(_fpsArray);
		for (int i = 0; i < _fpsArray.Length; ++i)
		{
			_colArray[i] = _fpsArray[i] < FPS_LOW ? fpsLowColor
												  : _fpsArray[i] < FPS_MID ? fpsMidColor : fpsHighColor;

			fpsHistogramMaterial.SetColorArray("_Array", _colArray);
		}
	}

	private void ShowDistributionOverTime(float currentFPS)
	{
		if (currentFPS < FPS_LOW)
		{
			_fpsLowCount++;
		}
		else if (currentFPS < FPS_MID)
		{
			_fpsMidCount++;
		}
		else
		{
			_fpsHighCount++;
		}

		float sum = _fpsLowCount + _fpsMidCount + _fpsHighCount;
		var lowMid = (float)_fpsLowCount / sum;
		var midHi = (float)(sum - _fpsHighCount) / sum;

		fpsDistributionMaterial.SetFloat("_LowMidThreshold", lowMid);
		fpsDistributionMaterial.SetFloat("_MidHighThreshold", midHi);
	}

	private void TakeFirstMeasurement(float currentFPS)
	{
		_tookFirstMeasurement = true;
		_fpsMin = currentFPS;
		_fpsMax = currentFPS;
		_fpsAvg = currentFPS;
	}
}
