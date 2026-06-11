using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC inimigo simples com NavMesh.
/// Persegue o jogador quando entra no raio de detecção e aplica dano.
/// Requer: NavMeshAgent, Animator (parâmetros: Speed, Attack)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detecção")]
    public float raioDeteccao  = 15f;
    public float raioAtaque    = 2f;
    public LayerMask layerJogador;

    [Header("Combate")]
    public float dano          = 10f;
    public float intervaloAtaque = 1.5f;

    [Header("Componentes")]
    private NavMeshAgent _agent;
    private Animator     _animator;
    private Transform    _jogador;
    private PlayerHealth _playerHealth;
    private float        _timerAtaque;

    private static readonly int HashSpeed  = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");

    void Start()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        var jogadorGO = GameObject.FindGameObjectWithTag("Player");
        if (jogadorGO != null)
        {
            _jogador      = jogadorGO.transform;
            _playerHealth = jogadorGO.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (_jogador == null) return;

        float distancia = Vector3.Distance(transform.position, _jogador.position);

        if (distancia <= raioAtaque)
        {
            _agent.isStopped = true;
            _timerAtaque    -= Time.deltaTime;

            if (_timerAtaque <= 0f)
            {
                Atacar();
                _timerAtaque = intervaloAtaque;
            }
        }
        else if (distancia <= raioDeteccao)
        {
            _agent.isStopped     = false;
            _agent.SetDestination(_jogador.position);
        }
        else
        {
            _agent.isStopped = true;
        }

        _animator?.SetFloat(HashSpeed, _agent.velocity.magnitude);
    }

    void Atacar()
    {
        _animator?.SetTrigger(HashAttack);
        _playerHealth?.ReceberDano(dano);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioDeteccao);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioAtaque);
    }
}
