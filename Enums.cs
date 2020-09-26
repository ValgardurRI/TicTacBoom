namespace TicTacBoom
{
    public enum PlayerType
    {
        None,
        Noughts = 10,
        Crosses
    }

    public enum EndStatus
    {
        Ongoing,
        Tie,
        Noughts = 10,
        Crosses
    }

    public enum FieldState
    {
        Hidden,
        None,
        Noughts = 10,
        Crosses
    }
}