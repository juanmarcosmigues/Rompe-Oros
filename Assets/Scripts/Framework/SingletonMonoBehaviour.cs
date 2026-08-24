using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SingletonMonoBehaviour<T> : MonoBehaviour where T:UnityEngine.Object {
	protected static T instance;

	protected virtual void Awake() {
		if (instance != null && instance != this) {
			Debug.LogError("There are two instances of " + typeof(T).FullName + "!");
		}
		instance = this as T;
	}
	
	void OnDestroy() {
		if (instance == this) instance = null;
	}
	
	public static T Instance {
		get {
			return GetInstance();
		}
	}

	public static T GetInstance() {
		if (instance != null) return instance;
		instance = (T)FindFirstObjectByType(typeof(T));
		if (instance == null
		#if UNITY_EDITOR
		&& EditorApplication.isPlayingOrWillChangePlaymode
		#endif
		) {
			throw new InvalidOperationException("There is no " + typeof(T).FullName + "!");
		}
		return instance;
	}
	
	public static bool HasInstance(){
		if (instance != null) return true;
		instance = (T)FindFirstObjectByType(typeof(T));
		return instance != null;
	}
}
