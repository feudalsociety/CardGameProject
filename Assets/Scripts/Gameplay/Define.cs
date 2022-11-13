using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    public const string JoinKey = "j";
    public enum CommandType
    {
        // UnitAttackCommand,
        // UnitDieCommand,
        // DealDamageCommand,
        // DelayCommand,
        DrawOpeningHandCommand,
        DrawACardCommand,
        // GameOverCommand,
        StartATurnCommand
        // ShowTurnCommand,
        // UpdateManaCommand
    }

    public enum UnitState
    { 
        Idle,
        Move,
        Attack
    }

    public enum Scene
    {
        Unknown,
        Bootstrap,
        Auth,
        Lobby,
        MainMenu,
        Deckbuilder,
        GamePlay,
    }

    public enum CardType
    {
        Unit,
        Spell,
    }

    public enum MouseEvent
    {
        Press, // 계속 누르는 상태
        Down,
        Up,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        BeginDrag,
        EndDrag,
    }

    public enum MouseButton
    {
        Left,
        Right,
    }
}
