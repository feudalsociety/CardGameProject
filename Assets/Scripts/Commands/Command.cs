
public abstract class Command
{
    protected CommandRequestData _data;
    public ref CommandRequestData Data => ref _data;

    public Command(CommandRequestData data)
    {
        _data = data;
    }

    // list of everything that we have to do with this command (draw a card, play a card, play spell effect, etc...)
    // there are 2 options of timing : 
    // 1) use tween sequences and call CommandExecutionComplete in OnComplete()
    // 2) use coroutines (IEnumerator) and WaitFor... to introduce delays, call CommandExecutionComplete() in the end of coroutine
    public abstract void Execute();

    //public static Command MakeCommand(CommandRequestData data)
    //{
    //    switch (data)
    //    { 
            
    //    }

    //}
}

// ��û�� ��ü�� ���·� ĸ��ȭ�Ͽ�(����� ����� ĸ��ȭ�����ν�)
// ����ڰ� ���� ��û�� ���߿� �̿��� �� �ֵ��� �ż��� �̸�,
// �Ű����� �� ��û�� �ʿ��� ������ ���� �Ǵ� �α�, ����� �� �ְ� �ϴ� ����