using UnityEngine;
using System.Collections;

/// <summary>
/// Convert between size/angular size/distance
/// </summary>
public static class AngularSize{
	public static float GetAngSize (float distance, float size) {
		return 2.0f * Mathf.Atan (size * 0.5f / distance) * Mathf.Rad2Deg;
	}
	public static float GetSize (float distance, float angularSize) {
		return  2.0f * distance * Mathf.Tan (angularSize * 0.5f * Mathf.Deg2Rad);
	}
	public static float GetDistance (float angularSize, float size) {
		return size * 0.5f / Mathf.Tan (angularSize * 0.5f * Mathf.Deg2Rad);
	}
}