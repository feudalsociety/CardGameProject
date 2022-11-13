using System.Collections;
using Unity.Netcode;

public interface ClientCardBaseData
{
    public int CardId { get; set; }
    public string CardName { get; set; }
    public Define.CardType CardType { get; set;}
    public int Cost { get; set; }
}

public struct ClientUnitBaseData : ClientCardBaseData, INetworkSerializable
{
    private int _cardId;
    private string _cardName;
    private Define.CardType _cardType;
    private int _cost;

    public int CardId { get { return _cardId; } set { _cardId = value;} }
    public string CardName { get { return _cardName; } set { _cardName = value;} }
    public Define.CardType CardType { get { return _cardType; } set { _cardType = value; } }
    public int Cost { get { return _cost; } set { _cost = value; } }

    private int _attack;
    private int _hp;
    private int _mobility;

    public int Attack { get { return _attack; } set { _attack = value; } }
    public int Hp { get { return _hp; } set { _hp = value; } }  
    public int Mobility { get { return _mobility; } set { _mobility = value;} }

    public ClientUnitBaseData(int cardId, string cardName, Define.CardType cardType, int cost, int attack, int hp, int mobility)
    {
        _cardId = cardId;
        _cardName = cardName;
        _cardType = cardType;
        _cost = cost;
        _attack = attack;
        _hp = hp;
        _mobility = mobility;
    }

    public override string ToString()
    {
        return "[General Card Data]\n"
             + $"ID : {CardId}\n"
             + $"Name : {CardName}\n"
             + $"Type : {CardType}\n"
             + $"Cost : {Cost}\n"
             + "[Unit Data]\n"
             + $"Attack : {Attack}\n"
             + $"Hp : {Hp}\n"
             + $"Mobility : {Mobility}";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _cardId);
        serializer.SerializeValue(ref _cardName);
        serializer.SerializeValue(ref _cardType);
        serializer.SerializeValue(ref _cost);
        serializer.SerializeValue(ref _attack);
        serializer.SerializeValue(ref _hp);
        serializer.SerializeValue(ref _mobility);
    }
}
