using UnityEngine;

/// <summary>
/// Controla a movimentação do jogador em terceira pessoa.
/// Requer: CharacterController, Animator
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float velocidadeCaminhada = 5f;
    public float velocidadeCorrida = 10f;
    public float velocidadeRotacao = 10f;
    public float gravidade = -9.81f;
    public float alturaJump = 1.5f;

    [Header("Câmera")]
    public Transform cameraPrincipal;

    [Header("Componentes")]
    private CharacterController _controller;
    private Animator _animator;
    private Vector3 _velocidadeVertical;
    private bool _estaNoChao;

    // Hash dos parâmetros do Animator para performance
    private static readonly int HashSpeed    = Animator.StringToHash("Speed");
    private static readonly int HashIsGround = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump     = Animator.StringToHash("Jump");

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator   = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        VerificarChao();
        Mover();
        Pular();
        AplicarGravidade();
    }

    void VerificarChao()
    {
        _estaNoChao = _controller.isGrounded;
        _animator?.SetBool(HashIsGround, _estaNoChao);

        if (_estaNoChao && _velocidadeVertical.y < 0)
            _velocidadeVertical.y = -2f;
    }

    void Mover()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        // Direção relativa à câmera
        Vector3 direcao = new Vector3(horizontal, 0f, vertical).normalized;

        bool correndo = Input.GetKey(KeyCode.LeftShift);
        float velocidade = correndo ? velocidadeCorrida : velocidadeCaminhada;

        if (direcao.magnitude >= 0.1f)
        {
            // Calcula ângulo baseado na câmera
            float angulo = Mathf.Atan2(direcao.x, direcao.z) * Mathf.Rad2Deg
                           + cameraPrincipal.eulerAngles.y;

            // Rotaciona o personagem suavemente
            float anguloSuave = Mathf.LerpAngle(
                transform.eulerAngles.y, angulo,
                velocidadeRotacao * Time.deltaTime
            );
            transform.rotation = Quaternion.Euler(0f, anguloSuave, 0f);

            // Move na direção calculada
            Vector3 direcaoMovimento = Quaternion.Euler(0f, angulo, 0f) * Vector3.forward;
            _controller.Move(direcaoMovimento.normalized * velocidade * Time.deltaTime);
        }

        // Atualiza Animator
        float speed = direcao.magnitude * (correndo ? 2f : 1f);
        _animator?.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);
    }

    void Pular()
    {
        if (Input.GetButtonDown("Jump") && _estaNoChao)
        {
            _velocidadeVertical.y = Mathf.Sqrt(alturaJump * -2f * gravidade);
            _animator?.SetTrigger(HashJump);
        }
    }

    void AplicarGravidade()
    {
        _velocidadeVertical.y += gravidade * Time.deltaTime;
        _controller.Move(_velocidadeVertical * Time.deltaTime);
    }
}
