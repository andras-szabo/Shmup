using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpaceBender : MonoBehaviour
{
	public const int WEIGHTED_OBJECT_MAX_COUNT = 16;
	public const int RIPPLE_MAX_COUNT = 8;
	public const float RIPPLE_MAX_RADIUS = 16f;
	
	public static SpaceBender Instance;

	public Material multiCurvedSpaceMaterial;
	private Vector4[] _objectArray = new Vector4[WEIGHTED_OBJECT_MAX_COUNT];
	private Vector4[] _ripples = new Vector4[RIPPLE_MAX_COUNT];

	private int _arrayLength;
	private bool _arrLimitReached;
	private bool _ripplesChanged;

	public float bulletWeight;
	public Slider bulletWeightSlider;

	public bool tint;
	public Color startTintColor;
	public Color midTintColor;
	public Color endTintColor;
	public float colorChangeDurationSecs;
	private float _elpasedSeconds;

	private void Awake()
	{
		Instance = this;
		for (int i = 0; i < WEIGHTED_OBJECT_MAX_COUNT; ++i)
		{
			_objectArray[i] = new Vector4(0, 0, 0, 0);

			if (i < RIPPLE_MAX_COUNT)
			{
				_ripples[i] = new Vector4(0, 0, 0, 0);
			}
		}
	}

	public void StartRipple(float viewportX, float viewportY, 
							float weight, float expansionPerFrame)
	{
		var rippleIndex = TryFindFreeRippleIndex();
		if (rippleIndex >= 0 && rippleIndex < RIPPLE_MAX_COUNT)
		{
			StartCoroutine(RippleRoutine(rippleIndex, viewportX, viewportY, weight, expansionPerFrame));
		}
	}

	private int TryFindFreeRippleIndex()
	{
		for (int i = 0; i < _ripples.Length; ++i)
		{
			if (Mathf.Approximately(_ripples[i].z, 0f))
			{
				return i;
			}
		}

		return -1;
	}

	private IEnumerator RippleRoutine(int rippleIndex, float vpX, float vpY,
									  float weight, float radiusGrowthPerFrame)
	{
		var radius = radiusGrowthPerFrame;
		var worldCoords = ViewportUtility.GetWorldPosition(new Vector2(vpX, vpY));

		while (radius < RIPPLE_MAX_RADIUS)
		{
			SetRipple(rippleIndex, new Vector4(worldCoords.x, worldCoords.y, radius, weight));
			yield return null;
			radius += radiusGrowthPerFrame;
		}

		SetRipple(rippleIndex, Vector4.zero);
	}

	private void SetRipple(int index, Vector4 rippleVector)
	{
		_ripples[index] = rippleVector;
		_ripplesChanged = true;
	}

	private void Start()
	{
		BulletWeightChanged();
		multiCurvedSpaceMaterial.SetVectorArray("_Ripples", _ripples);
	}

	public void SetPosition(Vector4 posAndWeight)
	{
		_objectArray[_arrayLength] = posAndWeight;
		_objectArray[_arrayLength++].z = 1f;

		if (_arrayLength >= WEIGHTED_OBJECT_MAX_COUNT)
		{
			//Reserve place 0 for player's ship
			_arrLimitReached = true;
			_arrayLength = 1;
		}
	}

	private void LateUpdate()
	{
		ResetNotUsedElements();
		multiCurvedSpaceMaterial.SetVectorArray("_Array", _objectArray);
		_arrLimitReached = false;
		_arrayLength = 0;

		if (_ripplesChanged)
		{
			multiCurvedSpaceMaterial.SetVectorArray("_Ripples", _ripples);
			_ripplesChanged = false;
		}

		//TODO
		//multiCurvedSpaceMaterial.SetFloat("_RotationAngleInRadians", Mathf.Deg2Rad * Mathf.Sin(Time.timeSinceLevelLoad / 8f) * 72f); 
	}

	private void ResetNotUsedElements()
	{
		for (int i = _arrLimitReached ? WEIGHTED_OBJECT_MAX_COUNT : _arrayLength; i < WEIGHTED_OBJECT_MAX_COUNT; ++i)
		{
			//z: either 1 or 0, for hopeful optimizations in the shader
			_objectArray[i].z = 0f;
		}
	}

	private void Update()
	{
		if (tint)
		{
			Tint();
		}
	}

	public void BulletWeightChanged()
	{
		bulletWeight = bulletWeightSlider.value;
	}

	private void Tint()
	{
		_elpasedSeconds += Time.smoothDeltaTime;

		if (_elpasedSeconds > colorChangeDurationSecs)
		{
			var tmp = startTintColor;
			startTintColor = midTintColor;
			midTintColor = endTintColor;
			endTintColor = tmp;

			_elpasedSeconds = 0f;
		}

		var color = Color.Lerp(startTintColor, midTintColor, _elpasedSeconds / colorChangeDurationSecs);
		multiCurvedSpaceMaterial.SetColor("_TintColor", color);
	}
}