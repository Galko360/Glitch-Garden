public sealed class GameOverMusicState : MusicStateBase
{
    public override string Name => "Game Over";

    public GameOverMusicState(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
        : base(manager, playback, tracks)
    {
    }

    public override void Enter()
    {
        Playback.PlayOneShot(Tracks.GameOver, null);
    }
}