using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    public int maxHp = 10;
    private int hp;

    void Awake()
    {
        hp = maxHp;
        Debug.Log("Base HP: " + hp);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        hp -= 1;
        Destroy(other.gameObject);

        Debug.Log("Base hit! HP now: " + hp);

        if (hp <= 0)
        {
            LoseGame();
        }
    }

    void LoseGame()
    {
        Debug.Log("YOU LOSE");
        Time.timeScale = 0f; // stop the game
    }
}
