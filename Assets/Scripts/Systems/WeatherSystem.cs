using UnityEngine;
using System.Collections;

/// <summary>
/// FUNCIONALIDADE 2: Sistema de Chuva / Clima Dinâmico
///
/// Alterna aleatoriamente entre tempo limpo e chuvoso.
/// Controla sistema de partículas de chuva, áudio ambiente,
/// intensidade da luz e neblina para maior imersividade.
///
/// Requer: ParticleSystem (chuva), AudioSource (chuva), Light (luz direcional)
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    // ─── Componentes ───────────────────────────────────────────
    [Header("Partículas de Chuva")]
    public ParticleSystem particulasChuva;
    public ParticleSystem particulasSalpico; // impacto no chão (opcional)

    [Header("Áudio")]
    public AudioSource audioChuvaPrincipal;
    public AudioClip   somChuvaFraca;
    public AudioClip   somChuvaForte;

    [Header("Iluminação")]
    public Light luzDirecional;
    public Color corLuzSol   = new Color(1f,   0.95f, 0.84f);
    public Color corLuzChuva = new Color(0.6f, 0.65f, 0.75f);
    public float intensidadeSol   = 1.2f;
    public float intensidadeChuva = 0.5f;

    [Header("Neblina")]
    public bool usarNeblina = true;
    public Color corNeblinaChuva = new Color(0.6f, 0.65f, 0.70f);
    public float densidadeNeblina = 0.03f;

    [Header("Temporização Aleatória")]
    public float tempoMinimoBom   = 30f; // segundos de sol
    public float tempoMaximoBom   = 90f;
    public float tempoMinimoChuva = 20f; // segundos de chuva
    public float tempoMaximoChuva = 60f;
    public float duracaoTransicao = 5f;  // fade entre estados

    // ─── Estado interno ────────────────────────────────────────
    public enum EstadoClima { Ensolarado, Nublado, Chuva }
    public EstadoClima climaAtual { get; private set; } = EstadoClima.Ensolarado;

    private Color _corNeblinaOriginal;
    private float _densidadeNeblinaOriginal;

    void Start()
    {
        // Salva configurações originais de neblina
        _corNeblinaOriginal      = RenderSettings.fogColor;
        _densidadeNeblinaOriginal = RenderSettings.fogDensity;

        // Garante que chuva começa desativada
        if (particulasChuva != null)      particulasChuva.Stop();
        if (particulasSalpico != null)    particulasSalpico.Stop();
        if (audioChuvaPrincipal != null)  audioChuvaPrincipal.Stop();

        // Inicia o ciclo climático
        StartCoroutine(CicloClimatico());
    }

    // ─── Ciclo principal ───────────────────────────────────────

    IEnumerator CicloClimatico()
    {
        while (true)
        {
            // Tempo bom
            float tempoBom = Random.Range(tempoMinimoBom, tempoMaximoBom);
            yield return new WaitForSeconds(tempoBom);

            // Inicia chuva
            yield return StartCoroutine(TransicionarPara(EstadoClima.Chuva));

            float tempoChuva = Random.Range(tempoMinimoChuva, tempoMaximoChuva);
            yield return new WaitForSeconds(tempoChuva);

            // Volta ao sol
            yield return StartCoroutine(TransicionarPara(EstadoClima.Ensolarado));
        }
    }

    IEnumerator TransicionarPara(EstadoClima novoEstado)
    {
        climaAtual = novoEstado;
        float tempo = 0f;

        bool chovendo = (novoEstado == EstadoClima.Chuva);

        // Liga partículas e áudio na transição para chuva
        if (chovendo)
        {
            if (particulasChuva != null)   particulasChuva.Play();
            if (particulasSalpico != null) particulasSalpico.Play();
            IniciarAudioChuva();
        }

        Color   corLuzInicial       = luzDirecional ? luzDirecional.color     : Color.white;
        Color   corLuzFinal         = chovendo ? corLuzChuva : corLuzSol;
        float   intensidadeInicial  = luzDirecional ? luzDirecional.intensity : 1f;
        float   intensidadeFinal    = chovendo ? intensidadeChuva : intensidadeSol;
        Color   corNeblinaInicial   = RenderSettings.fogColor;
        float   densidadeInicial    = RenderSettings.fogDensity;

        // Faz a transição gradual
        while (tempo < duracaoTransicao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoTransicao;

            // Interpola luz
            if (luzDirecional != null)
            {
                luzDirecional.color     = Color.Lerp(corLuzInicial, corLuzFinal, t);
                luzDirecional.intensity = Mathf.Lerp(intensidadeInicial, intensidadeFinal, t);
            }

            // Interpola neblina
            if (usarNeblina)
            {
                RenderSettings.fog         = true;
                RenderSettings.fogColor    = Color.Lerp(corNeblinaInicial, chovendo ? corNeblinaChuva : _corNeblinaOriginal, t);
                RenderSettings.fogDensity  = Mathf.Lerp(densidadeInicial, chovendo ? densidadeNeblina : _densidadeNeblinaOriginal, t);
            }

            // Fade no volume do áudio
            if (audioChuvaPrincipal != null && audioChuvaPrincipal.isPlaying)
                audioChuvaPrincipal.volume = chovendo ? Mathf.Lerp(0f, 1f, t) : Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Para partículas e áudio ao fim da chuva
        if (!chovendo)
        {
            if (particulasChuva != null)   particulasChuva.Stop();
            if (particulasSalpico != null) particulasSalpico.Stop();
            if (audioChuvaPrincipal != null) audioChuvaPrincipal.Stop();
        }
    }

    void IniciarAudioChuva()
    {
        if (audioChuvaPrincipal == null) return;
        audioChuvaPrincipal.clip   = somChuvaFraca != null ? somChuvaFraca : somChuvaForte;
        audioChuvaPrincipal.loop   = true;
        audioChuvaPrincipal.volume = 0f;
        audioChuvaPrincipal.Play();
    }

    // ─── API pública ───────────────────────────────────────────

    /// <summary>Força a chuva imediatamente (debug / cutscene).</summary>
    public void ForcarChuva()
    {
        StopAllCoroutines();
        StartCoroutine(TransicionarPara(EstadoClima.Chuva));
        StartCoroutine(RetomarCicloApos(Random.Range(tempoMinimoChuva, tempoMaximoChuva)));
    }

    IEnumerator RetomarCicloApos(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        StartCoroutine(CicloClimatico());
    }
}
