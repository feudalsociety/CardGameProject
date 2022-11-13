using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_PCardDisplay : UI_Base
{
    public int CardId { get; private set; } = -1;

    enum Texts
    {
        CardInfo
    }

    private void Start()
    {

    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    public void SetPCardDisplayData(ServerCardBaseData data)
    {
        TMP_Text dataText = Get<TMP_Text>((int)Texts.CardInfo);
        dataText.text = data.ToString();
        CardId = data.CardId;
    }
}
