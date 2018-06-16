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

	public Color fpsLowColor, fpsMidColor, fpsHighColor;
	public Image fpsIndicatorImage;
	public Material fpsHistogram;

	private bool _tookFirstMeasurement;
	private float _fpsMin, _fpsMax, _fpsAvg;
	private FPSQuality _fpsTrend;
	private int _framesOfCurrentTrend;

	private RingBuffer<float> _pastFps = new RingBuffer<float>(FPS_HISTOGRAM_COUNT);
	private float[] _fpsArray = new float[FPS_HISTOGRAM_COUNT];
	private Color[] _colArray = new Color[FPS_HISTOGRAM_COUNT];
	
	private int _framesLeftUntilMemorySample = MEMORY_SAMPLE_PER_FRAME;

	public void Reset()
	{
		_tookFirstMeasurement = false;
	}

	private void Update()
	{
		RecordFPSData(Time.deltaTime);
		UpdateFPSLabel();
		UpdateMemoryLabelIfNeeded();
		UpdateEntityLabel();
	}

	private void UpdateEntityLabel()
	{
		entityLabel.text = string.Format("Pooled entity count: {0}", GenericPool.pooledObjectCount);
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

		//ShowTrendIndicator();
		ShowFpsHistogram(current);
	}

	private void ShowFpsHistogram(float current)
	{
		_pastFps.Push(current);
		_pastFps.ToArray(_fpsArray);
		for (int i = 0; i < _fpsArray.Length; ++i)
		{
			_colArray[i] = _fpsArray[i] < FPS_LOW ? fpsLowColor
												  : _fpsArray[i] < FPS_MID ? fpsMidColor : fpsHighColor;

			fpsHistogram.SetColorArray("_Array", _colArray);
		}
	}

	private void ShowTrendIndicator()
	{
		var currentTrend = _fpsAvg < FPS_LOW ? FPSQuality.Low
											 : _fpsAvg < FPS_MID ? FPSQuality.Mid : FPSQuality.High;

		if (_fpsTrend != currentTrend)
		{
			if (--_framesOfCurrentTrend <= 0)
			{
				_fpsTrend = currentTrend;
				fpsIndicatorImage.color = _fpsTrend == FPSQuality.Low ? fpsLowColor
																	  : _fpsTrend == FPSQuality.Mid ? fpsMidColor : fpsHighColor;
			}
		}
		else
		{
			_framesOfCurrentTrend = Mathf.Min(10, _framesOfCurrentTrend + 1);
		}
	}

	private void TakeFirstMeasurement(float currentFPS)
	{
		_tookFirstMeasurement = true;
		_fpsMin = currentFPS;
		_fpsMax = currentFPS;
		_fpsAvg = currentFPS;
	}
}
