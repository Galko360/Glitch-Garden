public sealed class MainMenuMusicState : MusicStateBase
{
    public override string Name => "Main Menu Loop";

    public MainMenuMusicState(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
        : base(manager, playback, tracks)
    {
    }

    public override void Enter()
    {
        Playback.PlayLoop(Tracks.MainMenuLoop);
    }
}