using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UI_Base : MonoBehaviour
{
	protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
	public abstract void Init();

	// Start에 놓는 이유는 어떤 UI는 다른 UI가 먼저 Init되기를 원하는 경우가 있기 때문이다.
	private void Start()
	{
		Init();
	}

	protected void Bind<T>(Type type) where T : UnityEngine.Object
	{
		// Type이 enum이라는 보장은 없지만 enum을 넘겨줬으므로 
		string[] names = Enum.GetNames(type);
		UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
		_objects.Add(typeof(T), objects);

		// 연결 모든 이름 순회, 찾은 것을 object 배열에 넣는다.
		for (int i = 0; i < names.Length; i++)
		{
			if (typeof(T) == typeof(GameObject))
				// Gameobject 전용 버전
				objects[i] = Util.findChild(this.gameObject, names[i], true);
			else
				objects[i] = Util.findChild<T>(this.gameObject, names[i], true);

			if (objects[i] == null)
				Debug.Log($"Failed to Bind {names[i]}");
		}
	}

	protected T Get<T>(int idx) where T : UnityEngine.Object
	{
		UnityEngine.Object[] objects = null;
		if (_objects.TryGetValue(typeof(T), out objects) == false)
			return null;

		return objects[idx] as T;
	}

	protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
	protected Text GetText(int idx) { return Get<Text>(idx); }
	protected TMP_Text GetTMPText(int idx) { return Get<TMP_Text>(idx); }	
	protected Button GetButton(int idx) { return Get<Button>(idx); }
	protected Image GetImage(int idx) { return Get<Image>(idx); }

	public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_EventHandler evt = go.GetOrAddComponent<UI_EventHandler>();

		switch (type)
		{
			case Define.UIEvent.Click:
				evt.OnClickHandler -= action;
				evt.OnClickHandler += action;
				break;
			case Define.UIEvent.BeginDrag:
				evt.OnBeginDragHandler -= action;
				evt.OnBeginDragHandler += action;	
				break;
			case Define.UIEvent.Drag:
				evt.OnDragHandler -= action;
				evt.OnDragHandler += action;
				break;
			case Define.UIEvent.EndDrag:
				evt.OnEndDragHandler -= action;
				evt.OnEndDragHandler += action;
				break;
		}
	}

	// remove Event
	public static void RemoveEvent(GameObject go, Define.UIEvent type = Define.UIEvent.Click)
    {
		UI_EventHandler evt = go.GetOrAddComponent<UI_EventHandler>();

		switch (type)
		{
			case Define.UIEvent.Click:
				evt.OnClickHandler = null;
				break;
			case Define.UIEvent.BeginDrag:
				evt.OnBeginDragHandler = null;
				break;
			case Define.UIEvent.Drag:
				evt.OnDragHandler = null;
				break;
			case Define.UIEvent.EndDrag:
				evt.OnEndDragHandler = null;
				break;
		}
	}
}
