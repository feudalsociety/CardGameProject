using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    // 입력이 있으면 Event로 전파
    public Action KeyAction = null;
    public Action<Define.MouseEvent, Define.MouseButton> MouseAction = null;
    public Action<float> ScrollAction = null;

    public void OnUpdate()
    {
        if (Input.anyKey) KeyAction?.Invoke();

        if (MouseAction != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Down, Define.MouseButton.Left);
            }
            if (Input.GetMouseButtonUp(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Up, Define.MouseButton.Left);
            }
            if (Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press, Define.MouseButton.Left);
            }
            if (Input.GetMouseButton(1))
            {
                MouseAction.Invoke(Define.MouseEvent.Press, Define.MouseButton.Right);
            }
        }

        if (ScrollAction != null)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                ScrollAction.Invoke(Input.GetAxis("Mouse ScrollWheel"));
            }
        }
    }

    #region rayCastOnUI
    public bool IsPointerOverUIElement(string name = null)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(), name);
    }

    public bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, string name = null)
    {
        if (eventSystemRaysastResults.Count != 0)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[0];
            if (name == null && curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
            else if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI") && curRaysastResult.gameObject.name == name)
            {
                return true;
            }
        }
        return false;
    }

    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
    #endregion


    #region RayCastOnGameObject
    public bool IsPointerOverElement(string name = null, int layerMask = 1 << 0, Camera camera = null)
    {
        return IsPointerOverElement(GetRaycastResult(layerMask, camera), name);
    }

    public GameObject GetPointerOverElement(string name = null, int layerMask = 1 << 0, Camera camera = null)
    {
        return GetPointerOverElement(GetRaycastResult(layerMask, camera), name);
    }

    public bool IsPointerOverElement(GameObject hitObject, string name = null)
    {
        if (name == null && hitObject != null)
        {
            return true;
        }
        else if (hitObject != null && hitObject.name == name)
        {
            return true;
        }

        return false;
    }

    public GameObject GetPointerOverElement(GameObject hitObject, string name = null)
    {
        if (name == null && hitObject != null)
        {
            return hitObject;
        }
        else if (hitObject != null && hitObject.name == name)
        {
            return hitObject;
        }

        return null;
    }

    private GameObject GetRaycastResult(int layerMask, Camera camera = null)
    {
        Ray ray;
        RaycastHit info;

        if (camera == null)
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out info, Mathf.Infinity, layerMask))
        {
            GameObject target = info.collider.gameObject;
            return target;
        }

        return null;
    }
    #endregion

    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
        ScrollAction = null;
    }
}
