using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Scene : UI_Base
{
    public override void Init()
    {
        // scene�� sorting �ʿ���� sorting order �ڵ����� 0���� ������
        Managers.UI.SetCanvus(gameObject, false, RenderMode.ScreenSpaceOverlay);
    }
}
