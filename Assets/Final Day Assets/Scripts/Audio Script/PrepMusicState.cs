public sealed class PrepMusicState : MusicStateBase
{
    public override string Name => "Prep For Wave";

    public PrepMusicState(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
        : base(manager, playback, tracks)
    {
    }

    public override void Enter()
    {
        Playback.PlayLoop(Tracks.PrepForWave);
    }
}