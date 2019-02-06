using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Garter))]
public class HGarterEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("GameArter SDK worker. Must be visible in automatically initialized DoNotDestroyOnLoad object. If is visible clasically in hiearchy, remove it. Otherwise it will throw errors.", MessageType.Info);
	}
}