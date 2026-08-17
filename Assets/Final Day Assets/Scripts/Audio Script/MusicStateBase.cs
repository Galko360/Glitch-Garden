public abstract class MusicStateBase : IMusicState
{
    protected readonly GameMusicManager Manager;
    protected readonly IMusicPlayback Playback;
    protected readonly MusicTrackLibrary Tracks;

    public abstract string Name { get; }

    protected MusicStateBase(
        GameMusicManager manager,
        IMusicPlayback playback,
        MusicTrackLibrary tracks)
    {
        Manager = manager;
        Playback = playback;
        Tracks = tracks;
    }

    public abstract void Enter();

    public virtual void Exit()
    {
        Playback.Stop();
    }
}