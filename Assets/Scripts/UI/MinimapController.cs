using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FUNCIONALIDADE 3: Mini-Mapa no HUD
///
/// Renderiza uma câmera ortográfica de cima para baixo em uma
/// RenderTexture exibida como imagem circular no HUD.
/// O ícone do jogador e marcadores de pontos de interesse
/// são exibidos sobre o mapa.
///
/// Setup:
///   1. Crie um GameObject "MinimapCamera" filho do Player
///   2. Adicione Camera, aponte para baixo (rot X=90), Orthographic
///   3. Crie uma RenderTexture (ex: 256x256) e atribua à câmera
///   4. No Canvas, crie um RawImage circular (com máscara) e atribua aqui
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("Referências")]
    public Camera    cameraMinimap;         // câmera ortográfica apontando para baixo
    public Transform jogador;               // Transform do jogador
    public RawImage  imagemMinimap;         // RawImage no Canvas HUD
    public RectTransform iconeJogador;      // Seta/ícone do jogador no mapa

    [Header("Configurações")]
    public float alturaCamera  = 30f;       // altura da câmera acima do jogador
    public float tamanhoOrto   = 50f;       // área coberta pelo mapa (orthographicSize)
    public bool  rotacionarMapa = true;     // mapa gira com o jogador?

    [Header("Marcadores")]
    public GameObject prefabMarcador;       // ícone de POI no mapa
    public Transform[] pontosDePonto;       // pontos de interesse na cena

    private RectTransform[] _marcadoresUI;

    void Start()
    {
        if (cameraMinimap == null)
        {
            Debug.LogWarning("[Minimap] Câmera do minimap não atribuída!");
            return;
        }

        cameraMinimap.orthographic     = true;
        cameraMinimap.orthographicSize = tamanhoOrto;

        // Cria marcadores para cada ponto de interesse
        if (prefabMarcador != null && pontosDePonto != null)
        {
            _marcadoresUI = new RectTransform[pontosDePonto.Length];
            for (int i = 0; i < pontosDePonto.Length; i++)
            {
                if (pontosDePonto[i] == null) continue;
                var go = Instantiate(prefabMarcador, imagemMinimap.transform);
                _marcadoresUI[i] = go.GetComponent<RectTransform>();
            }
        }
    }

    void LateUpdate()
    {
        if (cameraMinimap == null || jogador == null) return;

        // Posiciona câmera sempre acima do jogador
        Vector3 posCamera = jogador.position;
        posCamera.y += alturaCamera;
        cameraMinimap.transform.position = posCamera;

        // Rotação do mapa
        if (rotacionarMapa)
        {
            // Câmera gira com o jogador (norte relativo)
            cameraMinimap.transform.rotation = Quaternion.Euler(90f, jogador.eulerAngles.y, 0f);

            // Ícone do jogador aponta sempre para cima (estático)
            if (iconeJogador != null)
                iconeJogador.localRotation = Quaternion.identity;
        }
        else
        {
            // Câmera sempre aponta para o norte do mundo
            cameraMinimap.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Ícone gira para mostrar direção do jogador
            if (iconeJogador != null)
                iconeJogador.localRotation = Quaternion.Euler(0f, 0f, -jogador.eulerAngles.y);
        }

        // Atualiza posição dos marcadores de POI no mapa
        AtualizarMarcadores();
    }

    void AtualizarMarcadores()
    {
        if (_marcadoresUI == null) return;

        Rect rectMapa = imagemMinimap.rectTransform.rect;
        float escala  = rectMapa.width / (tamanhoOrto * 2f); // pixels por unidade do mundo

        for (int i = 0; i < pontosDePonto.Length; i++)
        {
            if (pontosDePonto[i] == null || _marcadoresUI[i] == null) continue;

            // Diferença de posição no mundo
            Vector3 delta = pontosDePonto[i].position - jogador.position;

            // Converte para coordenadas do mapa levando em conta a rotação
            float angulo  = rotacionarMapa ? -jogador.eulerAngles.y * Mathf.Deg2Rad : 0f;
            float mx =  delta.x * Mathf.Cos(angulo) - delta.z * Mathf.Sin(angulo);
            float mz =  delta.x * Mathf.Sin(angulo) + delta.z * Mathf.Cos(angulo);

            // Aplica na UI (com clamp para não sair do mapa)
            float px = Mathf.Clamp(mx * escala, -rectMapa.width  * 0.45f, rectMapa.width  * 0.45f);
            float py = Mathf.Clamp(mz * escala, -rectMapa.height * 0.45f, rectMapa.height * 0.45f);

            _marcadoresUI[i].anchoredPosition = new Vector2(px, py);

            // Oculta marcador se estiver fora do alcance do mapa
            bool visivel = Mathf.Abs(delta.x) < tamanhoOrto && Mathf.Abs(delta.z) < tamanhoOrto;
            _marcadoresUI[i].gameObject.SetActive(visivel);
        }
    }
}
