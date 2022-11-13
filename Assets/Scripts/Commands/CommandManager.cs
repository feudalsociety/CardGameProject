using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CommandManager : MonoBehaviour
{
    private Queue<Command> _commandQueue = new Queue<Command>();

    private bool _playingQueue = false;

    public void AddToQueue(CommandRequestData data)
    {
        // TODO : MakeCommand
        //_commandQueue.Enqueue(command);

        if (!_playingQueue)
            PlayFirstCommandFromQueue();
    }

    public void CommandExecutionComplete() // MoveToNextCommand
    {
        if (_commandQueue.Count > 0)
            PlayFirstCommandFromQueue();
        else
            _playingQueue = false;
    }

    private void PlayFirstCommandFromQueue()
    {
        _playingQueue = true;
        _commandQueue.Dequeue().Execute();
    }

    public bool CardDrawPending()
    {
        foreach (var c in _commandQueue)
        {
            // if (c is DrawACardCommand) return true;
        }
        return false;
    }
}
