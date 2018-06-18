using UnityEditor;

[CustomEditor(typeof(Hittable))]
public class HittableInspector : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var asHittable = (Hittable)target;

		EditorGUILayout.LabelField(string.Format("HP: {0:F2} / In graveyard: {1}", 
												 asHittable.CurrentHP, asHittable.myEntity.IsInGraveyard));
		EditorGUILayout.LabelField("Pending damage");

		foreach (var dmgInfo in asHittable.PendingDamageInfo())
		{
			EditorGUILayout.LabelField(dmgInfo);
		}
	}
}
