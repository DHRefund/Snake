namespace SnakeApple.Domain.Rules
{
    public enum MoveRejectionReason
    {
        None,
        SessionNotPlaying,
        OutsideBoard,
        Wall,
        SnakeBody,
        ClosedDoor,
        MovableBlocked,
        TeleportBlocked
    }
}
