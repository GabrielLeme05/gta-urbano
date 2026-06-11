using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controla o Menu Principal com música de fundo,
/// animação de fade e navegação entre cenas.
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasFade;
    public Text        textoVersao;

    [Header("Painéis")]
    public GameObject painelMenu;
    public GameObject painelOpcoes;
    public GameObject painelCreditos;

    [Header("Música")]
    public AudioSource audioMusica;
    public AudioClip[] playlistMusicas; // shuffle de músicas no menu
    public float volumeMusica = 0.5f;

    [Header("Configurações")]
    public float duracaoFade = 1f;
    public int   indiceCenaJogo = 1; // Build index da cena do jogo

    private int _indexMusica = 0;

    void Start()
    {
        // Versão
        if (textoVersao != null)
            textoVersao.text = "v" + Application.version;

        // Garante que o tempo esteja normal (voltando do Game Over)
        Time.timeScale = 1f;

        // Inicia música
        if (audioMusica != null && playlistMusicas.Length > 0)
        {
            _indexMusica = Random.Range(0, playlistMusicas.Length);
            TocarMusica(_indexMusica);
        }

        // Começa com fade in
        if (canvasFade != null)
            StartCoroutine(FadeIn());

        // Exibe apenas o painel principal
        MostrarPainel(painelMenu);
    }

    void Update()
    {
        // Avança música quando terminar
        if (audioMusica != null && !audioMusica.isPlaying && playlistMusicas.Length > 1)
        {
            _indexMusica = (_indexMusica + 1) % playlistMusicas.Length;
            TocarMusica(_indexMusica);
        }
    }

    // ─── Navegação ─────────────────────────────────────────────

    public void BotaoJogar()
    {
        StartCoroutine(CarregarCena(indiceCenaJogo));
    }

    public void BotaoOpcoes()
    {
        MostrarPainel(painelOpcoes);
    }

    public void BotaoCreditos()
    {
        MostrarPainel(painelCreditos);
    }

    public void BotaoVoltar()
    {
        MostrarPainel(painelMenu);
    }

    public void BotaoSair()
    {
        StartCoroutine(SairJogo());
    }

    // ─── Helpers ───────────────────────────────────────────────

    void MostrarPainel(GameObject painel)
    {
        if (painelMenu     != null) painelMenu.SetActive(false);
        if (painelOpcoes   != null) painelOpcoes.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);

        if (painel != null) painel.SetActive(true);
    }

    void TocarMusica(int index)
    {
        audioMusica.clip   = playlistMusicas[index];
        audioMusica.volume = volumeMusica;
        audioMusica.Play();
    }

    // ─── Slider de Volume (Opções) ─────────────────────────────

    public void AlterarVolume(float valor)
    {
        if (audioMusica != null)
            audioMusica.volume = valor;
        AudioListener.volume = valor;
    }

    // ─── Coroutines ────────────────────────────────────────────

    IEnumerator CarregarCena(int index)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(index);
    }

    IEnumerator SairJogo()
    {
        yield return StartCoroutine(FadeOut());
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FadeIn()
    {
        canvasFade.alpha = 1f;
        float t = 0f;
        while (t < duracaoFade)
        {
            t += Time.deltaTime;
            canvasFade.alpha = 1f - (t / duracaoFade);
            yield return null;
        }
        canvasFade.alpha = 0f;
    }

    IEnumerator FadeOut()
    {
        canvasFade.alpha = 0f;
        float t = 0f;
        while (t < duracaoFade)
        {
            t += Time.deltaTime;
            canvasFade.alpha = t / duracaoFade;
            yield return null;
        }
        canvasFade.alpha = 1f;
    }
}
