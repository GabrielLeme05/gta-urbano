using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// FUNCIONALIDADE 1: Sistema de Vida e Game Over
/// 
/// Gerencia a saúde do jogador, exibe a barra de vida no HUD
/// e mostra a tela de Game Over quando a vida chega a zero.
/// 
/// Requer: Slider (barraVida) e Canvas (telaGameOver) configurados no Inspector.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float vidaMaxima = 100f;
    public float vidaAtual;

    [Header("UI - Barra de Vida")]
    public Slider barraVida;
    public Image preenchimentoBarra;
    public Color corVidaAlta   = Color.green;
    public Color corVidaMedia  = new Color(1f, 0.6f, 0f); // laranja
    public Color corVidaBaixa  = Color.red;

    [Header("UI - Game Over")]
    public GameObject telaGameOver;
    public Text textoPontuacaoFinal;

    [Header("Efeitos")]
    public GameObject efetioDano;       // Partícula de dano (opcional)
    public AudioClip somDano;
    public AudioClip somMorte;

    private AudioSource _audio;
    private bool _morreu = false;

    void Start()
    {
        vidaAtual = vidaMaxima;
        _audio    = GetComponent<AudioSource>();

        AtualizarUI();

        if (telaGameOver != null)
            telaGameOver.SetActive(false);
    }

    /// <summary>
    /// Aplica dano ao jogador. Chamado por inimigos, colisões, etc.
    /// </summary>
    public void ReceberDano(float quantidade)
    {
        if (_morreu) return;

        vidaAtual -= quantidade;
        vidaAtual  = Mathf.Clamp(vidaAtual, 0f, vidaMaxima);

        // Feedback de dano
        if (_audio != null && somDano != null)
            _audio.PlayOneShot(somDano);

        if (efetioDano != null)
            Instantiate(efetioDano, transform.position, Quaternion.identity);

        AtualizarUI();

        if (vidaAtual <= 0f)
            StartCoroutine(Morrer());
    }

    /// <summary>
    /// Cura o jogador (coletáveis de kit médico, etc.)
    /// </summary>
    public void Curar(float quantidade)
    {
        if (_morreu) return;

        vidaAtual += quantidade;
        vidaAtual  = Mathf.Clamp(vidaAtual, 0f, vidaMaxima);
        AtualizarUI();
    }

    /// <summary>
    /// Atualiza o Slider e a cor da barra de vida no HUD.
    /// </summary>
    void AtualizarUI()
    {
        if (barraVida == null) return;

        float porcentagem = vidaAtual / vidaMaxima;
        barraVida.value = porcentagem;

        // Muda a cor conforme a vida
        if (preenchimentoBarra != null)
        {
            if      (porcentagem > 0.6f) preenchimentoBarra.color = corVidaAlta;
            else if (porcentagem > 0.3f) preenchimentoBarra.color = corVidaMedia;
            else                         preenchimentoBarra.color = corVidaBaixa;
        }
    }

    IEnumerator Morrer()
    {
        _morreu = true;

        if (_audio != null && somMorte != null)
            _audio.PlayOneShot(somMorte);

        // Desativa controle do jogador
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        // Pequena pausa dramática antes da tela de Game Over
        yield return new WaitForSeconds(1.5f);

        // Exibe tela de Game Over
        if (telaGameOver != null)
        {
            telaGameOver.SetActive(true);

            // Exibe pontuação final se existir o sistema
            var pontuacao = FindObjectOfType<PontuacaoManager>();
            if (pontuacao != null && textoPontuacaoFinal != null)
                textoPontuacaoFinal.text = "Pontuação: " + pontuacao.PontuacaoAtual;

            Time.timeScale = 0f; // Pausa o jogo
        }
    }

    // ─── Botões da tela de Game Over ────────────────────────────

    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // cena 0 = Menu Principal
    }
}
