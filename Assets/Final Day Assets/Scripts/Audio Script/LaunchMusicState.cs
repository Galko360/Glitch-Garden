public sealed class LaunchMusicState : MusicStateBase
{
    public override string Name => "Game Launched";

    public LaunchMusicState(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
        : base(manager, playback, tracks)
    {
    }

    public override void Enter()
    {
        Playback.PlayOneShot(
            Tracks.GameLaunched,
            () => Manager.HandleLaunchFinished(this)
        );
    }
}