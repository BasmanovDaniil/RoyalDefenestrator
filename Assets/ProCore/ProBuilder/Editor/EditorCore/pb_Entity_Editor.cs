using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

[CustomEditor(typeof(pb_Entity))]
[CanEditMultipleObjects]
public class pb_Entity_Editor : Editor
{
	pb_Entity ent;
	public enum ColType
	{
		MeshCollider,
		BoxCollider,
		SphereCollider
	}

	public void OnEnable()
	{
		ent = (pb_Entity)target;
		// if(ent.colliderType != pb_Entity.ColliderType.Upgraded) ent.GenerateCollisions();
	}

	public override void OnInspectorGUI()
	{
		GUI.changed = false;

		EntityType et = ent.entityType;
		et = (EntityType)EditorGUILayout.EnumPopup("Entity Type", et);
		if(et != ent.entityType) { ent.SetEntityType(et); GUI.changed = false; EditorUtility.SetDirty(ent); }

		// Convience
		GUILayout.Label("Add Collider", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();

			if(GUILayout.Button("Mesh Collider", EditorStyles.miniButtonLeft))
				AddCollider( ColType.MeshCollider );

			if(GUILayout.Button("Box Collider", EditorStyles.miniButtonMid))
				AddCollider( ColType.BoxCollider );

			if(GUILayout.Button("Remove Collider", EditorStyles.miniButtonRight))
				RemoveColliders();

		GUILayout.EndHorizontal();

		GUILayout.Space(4);

		if(GUI.changed)
			EditorUtility.SetDirty(ent);
	}

	private void AddCollider(ColType c)
	{
		RemoveColliders();
		
		foreach(pb_Entity obj in serializedObject.targetObjects)
		{
			GameObject go = obj.gameObject;

			switch(c)
			{
				case ColType.MeshCollider:
					go.AddComponent<MeshCollider>();
					break;

				case ColType.BoxCollider:	
					go.AddComponent<BoxCollider>();
					break;

				case ColType.SphereCollider:	
					go.AddComponent<SphereCollider>();
					break;

				default:
					break;
			}
		}

	}

	private void RemoveColliders()
	{
		foreach(pb_Entity obj in serializedObject.targetObjects)
		{
			foreach(Collider c in obj.gameObject.GetComponents<Collider>())
				DestroyImmediate(c);
		}
	}
}
