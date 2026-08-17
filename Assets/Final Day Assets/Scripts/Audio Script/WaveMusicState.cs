public sealed class WaveMusicState : MusicStateBase
{
    public override string Name => "Wave";

    public WaveMusicState(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
        : base(manager, playback, tracks)
    {
    }

    public override void Enter()
    {
        Playback.PlayOneShot(
            Tracks.WaveStarted,
            () => Manager.HandleWaveStartPresentationFinished(this)
        );
    }

    public void StartGameLoop()
    {
        Playback.PlayLoop(Tracks.GameLoop);
    }
}