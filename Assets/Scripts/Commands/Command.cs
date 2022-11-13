
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

// 요청을 객체의 형태로 캡슐화하여(실행될 기능을 캡슐화함으로써)
// 사용자가 보낸 요청을 나중에 이용할 수 있도록 매서드 이름,
// 매개변수 등 요청에 필요한 정보를 저장 또는 로깅, 취소할 수 있게 하는 패턴