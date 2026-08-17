public interface IMusicState
{
    string Name { get; }

    void Enter();

    void Exit();
}