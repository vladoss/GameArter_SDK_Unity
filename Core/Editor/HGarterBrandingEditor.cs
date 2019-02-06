using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(HGarterBranding))]
public class HGarterBrandingEditor : Editor {
	
	public override void OnInspectorGUI()
	{
		HGarterBranding init = (HGarterBranding)target;

		EditorGUILayout.BeginHorizontal ();
		init.brand = (HGarterBranding.Brand)EditorGUILayout.EnumPopup ("Brand", (HGarterBranding.Brand)init.brand);
		EditorGUILayout.EndHorizontal();

		if (init.brand == HGarterBranding.Brand.individual) {

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel("Source Texture");
			init.logoTexture = (Texture)EditorGUILayout.ObjectField (init.logoTexture, typeof(Texture), allowSceneObjects:true);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("url to open");
			init.logoRedirect = EditorGUILayout.TextField (init.logoRedirect);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (AssetPreview.GetAssetPreview(init.logoTexture));
			EditorGUILayout.EndHorizontal ();
		}
	}
}
