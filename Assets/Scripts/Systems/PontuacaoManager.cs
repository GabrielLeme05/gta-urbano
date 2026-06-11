using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia a pontuação do jogador durante a partida.
/// </summary>
public class PontuacaoManager : MonoBehaviour
{
    public static PontuacaoManager Instance { get; private set; }

    [Header("UI")]
    public Text textoPontuacao;

    public int PontuacaoAtual { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AdicionarPontos(int pontos)
    {
        PontuacaoAtual += pontos;
        if (textoPontuacao != null)
            textoPontuacao.text = "Pontos: " + PontuacaoAtual.ToString("N0");
    }
}
