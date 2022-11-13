using Unity.Netcode;

public struct CommandRequestData : INetworkSerializable
{
    private Define.CommandType _commandTypeEnum;

    public CommandRequestData(Define.CommandType commandType)
    {
        _commandTypeEnum = commandType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _commandTypeEnum);
    }
}
