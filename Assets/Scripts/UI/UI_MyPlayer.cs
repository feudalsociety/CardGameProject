using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MyPlayer : UI_Scene
{
    public static readonly float DistanceFromCamera = 700.0f;

    enum Buttons
    {
        EndTurn
    }

    enum Texts
    {
        EnemyPlayerNameText,
        MyPlayerNameText
    }

    [SerializeField] public UI_Deck Deck;
    [SerializeField] public UI_Grave Grave;
    [SerializeField] public MyHandManager MyHandManager;

    private TMP_Text _myPlayerName;
    private TMP_Text _enemyPlayerName;

    private void Start() { }

    public override void Init()
    {
        // scene�� sorting �ʿ���� sorting order �ڵ����� 0���� ������
        // world space�� ������ card
        // ��ġ�����̴�.
        gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, DistanceFromCamera);
        gameObject.transform.localRotation = Quaternion.identity;

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        // Add Event
        GetButton((int)Buttons.EndTurn).gameObject.AddUIEvent(EndMyTurn);
        _myPlayerName = GetTMPText((int)Texts.MyPlayerNameText);
        _enemyPlayerName = GetTMPText((int)Texts.EnemyPlayerNameText);

        // Instantiate�� �Ǿ����� Bind�� �ȵǾ OpenDeckPanel�� �۵����� �ʴ´�.
        //Deck = Managers.UI.MakeSubItem<UI_Deck>(parent: gameObject.transform);
        //Grave = Managers.UI.MakeSubItem<UI_Grave>(parent: gameObject.transform);
        //HandSlots = GetObject((int)GameObjects.HandSlots).transform;
    }

    private void EndMyTurn(PointerEventData data)
    {
        TurnManager.Instance.EndTurnServerRpc();
    }

    public void SetMyPlayerNameDisplay(string myPlayerName)
    {
        _myPlayerName.text = myPlayerName;
    }

    public void SetEnemyPlayerNameDisplay(string enemyPlayerName)
    {
        _enemyPlayerName.text = enemyPlayerName;
    }
}
