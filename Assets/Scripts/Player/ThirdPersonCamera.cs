using UnityEngine;

/// <summary>
/// Câmera em terceira pessoa que orbita ao redor do jogador.
/// Adicione este script a um GameObject vazio e configure o target.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    [Header("Configurações")]
    public float sensibilidade = 3f;
    public float distancia = 5f;
    public float distanciaMin = 1.5f;
    public float distanciaMax = 8f;
    public float suavizacao = 10f;

    [Header("Limites Verticais")]
    public float limiteVerticalMin = -20f;
    public float limiteVerticalMax = 60f;

    private float _rotacaoX;
    private float _rotacaoY;

    void Start()
    {
        // Inicializa com a rotação atual
        _rotacaoX = transform.eulerAngles.y;
        _rotacaoY = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Captura input do mouse
        _rotacaoX += Input.GetAxis("Mouse X") * sensibilidade;
        _rotacaoY -= Input.GetAxis("Mouse Y") * sensibilidade;
        _rotacaoY  = Mathf.Clamp(_rotacaoY, limiteVerticalMin, limiteVerticalMax);

        // Zoom com scroll
        distancia -= Input.GetAxis("Mouse ScrollWheel") * 2f;
        distancia  = Mathf.Clamp(distancia, distanciaMin, distanciaMax);

        // Calcula posição da câmera
        Quaternion rotacao = Quaternion.Euler(_rotacaoY, _rotacaoX, 0f);
        Vector3 posicaoDesejada = target.position + offset - (rotacao * Vector3.forward * distancia);

        // Verifica colisão com paredes
        RaycastHit hit;
        if (Physics.Linecast(target.position + offset, posicaoDesejada, out hit))
        {
            posicaoDesejada = hit.point + hit.normal * 0.2f;
        }

        // Aplica posição com suavização
        transform.position = Vector3.Lerp(transform.position, posicaoDesejada, suavizacao * Time.deltaTime);
        transform.LookAt(target.position + offset);
    }
}
