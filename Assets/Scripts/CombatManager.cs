using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [Header("Player 1")]
    public Animator p1Animator;
    public int p1MaxHealth = 100;
    public int p1Health = 100;
    public Image p1HealthBar;
    public TextMeshProUGUI p1HealthText;

    [Header("Enemy")]
    public Animator enemyAnimator;
    public int enemyMaxHealth = 100;
    public int enemyHealth = 100;
    public Image enemyHealthBar;
    public TextMeshProUGUI enemyHealthText;

    [Header("Damage")]
    public int attack1Damage = 10;
    public int attack2Damage = 20;
    public int enemyDamage = 12;

    [Header("Particle Effects")]
    public GameObject attack1Effect;
    public GameObject attack2Effect;
    public GameObject enemyDeathEffect;
    public GameObject p1HitEffect;

    [Header("UI")]
    public Button attack1Button;
    public Button attack2Button;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI gameOverSubtitle;

    [Header("Timing")]
    public float p1HitDelay = 0.3f;
    public float enemyTurnDelay = 0.7f;
    public float enemyHitDelay = 0.5f;
    public float gameOverDelay = 1.0f;

    bool isBusy = false;
    bool isGameOver = false;

    void Start()
    {
        p1Health = p1MaxHealth;
        enemyHealth = enemyMaxHealth;
        gameOverPanel.SetActive(false);
        UpdateHealthUI();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // 1 and 2 also attack, R restarts after game over
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Attack1();
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Attack2();
        }
        else if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (isGameOver)
            {
                Restart();
            }
        }
    }

    // ----- Buttons -----

    public void Attack1()
    {
        if (isBusy == false && isGameOver == false)
        {
            StartCoroutine(PlayerTurn("Attack1", attack1Damage, attack1Effect));
        }
    }

    public void Attack2()
    {
        if (isBusy == false && isGameOver == false)
        {
            StartCoroutine(PlayerTurn("Attack2", attack2Damage, attack2Effect));
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ----- Damage -----

    public void DamageEnemy(int damage)
    {
        enemyHealth = enemyHealth - damage;

        if (enemyHealth <= 0)
        {
            enemyHealth = 0;
            enemyAnimator.SetBool("Death", true);
            PlayEffect(enemyDeathEffect);
            GameOver(true);
        }
        else
        {
            enemyAnimator.SetTrigger("Hurt");
        }

        UpdateHealthUI();
    }

    public void DamagePlayer(int damage)
    {
        p1Health = p1Health - damage;

        if (p1Health <= 0)
        {
            p1Health = 0;
            p1Animator.SetBool("Death", true);
            GameOver(false);
        }
        else
        {
            p1Animator.SetTrigger("Hurt");
        }

        UpdateHealthUI();
    }

    // ----- UI -----

    public void UpdateHealthUI()
    {
        enemyHealthText.text = "Enemy Health: " + enemyHealth;
        p1HealthText.text = "P1 Health: " + p1Health;

        enemyHealthBar.fillAmount = (float)enemyHealth / enemyMaxHealth;
        p1HealthBar.fillAmount = (float)p1Health / p1MaxHealth;

        enemyHealthBar.color = GetBarColor(enemyHealthBar.fillAmount);
        p1HealthBar.color = GetBarColor(p1HealthBar.fillAmount);
    }

    public Color GetBarColor(float amount)
    {
        if (amount > 0.5f)
        {
            return Color.green;
        }
        else if (amount > 0.25f)
        {
            return Color.yellow;
        }
        else
        {
            return Color.red;
        }
    }

    public void SetButtons(bool on)
    {
        attack1Button.interactable = on;
        attack2Button.interactable = on;
    }

    public void GameOver(bool playerWon)
    {
        isGameOver = true;
        SetButtons(false);

        gameOverTitle.text = "GAME OVER";

        if (playerWon)
        {
            gameOverSubtitle.text = "You Win!";
            gameOverSubtitle.color = Color.yellow;
        }
        else
        {
            gameOverSubtitle.text = "You Lose...";
            gameOverSubtitle.color = Color.red;
        }

        StartCoroutine(ShowGameOver());
    }

    public void PlayEffect(GameObject effect)
    {
        if (effect != null)
        {
            // turning it off and on again restarts the effect
            effect.SetActive(false);
            effect.SetActive(true);
        }
    }

    // ----- Turns -----

    IEnumerator PlayerTurn(string attackName, int damage, GameObject effect)
    {
        isBusy = true;
        SetButtons(false);

        // P1 attacks
        p1Animator.SetTrigger(attackName);
        yield return new WaitForSeconds(p1HitDelay);
        PlayEffect(effect);
        DamageEnemy(damage);

        if (isGameOver)
        {
            yield break;
        }

        // enemy attacks back
        yield return new WaitForSeconds(enemyTurnDelay);
        enemyAnimator.SetTrigger("Attack");
        yield return new WaitForSeconds(enemyHitDelay);
        PlayEffect(p1HitEffect);
        DamagePlayer(enemyDamage);

        if (isGameOver)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.4f);
        isBusy = false;
        SetButtons(true);
    }

    IEnumerator ShowGameOver()
    {
        // wait so the death animation can play
        yield return new WaitForSeconds(gameOverDelay);

        gameOverPanel.SetActive(true);
        CanvasGroup panel = gameOverPanel.GetComponent<CanvasGroup>();

        // fade in
        panel.alpha = 0f;
        while (panel.alpha < 1f)
        {
            panel.alpha = panel.alpha + Time.deltaTime * 2f;
            yield return null;
        }
        panel.alpha = 1f;
    }
}
