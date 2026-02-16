using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [Tooltip("InputReader handles player input")]
    [SerializeField]
    private InputReader _inputReader;
    private Animator _animator;

    private void Awake()
    {
        _inputReader=this.GetComponent<InputReader>();
        _animator=this.GetComponent<Animator>();
        toggle = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputReader.onAttackStart += AttackStart;
        _inputReader.onAttackPerformed += Attack;
        _inputReader.onBlockPerformed += BlockToggle;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private float attackTimerStart;
    private void AttackStart()
    {
        print("attack started");
        attackTimerStart = Time.time;
    }

    private void Attack()
    {
        var dif = Time.time - attackTimerStart;
        if (dif <= 0.3f)
            _animator.SetTrigger("Light Attack");
        else
            _animator.SetTrigger("Heavy Attack");
    }

    private bool toggle;
    private void BlockToggle(bool toggle)
    {
        this.toggle = toggle;
    }

    private void OnDestroy()
    {
        _inputReader.onAttackStart -= AttackStart;
        _inputReader.onAttackPerformed -= Attack;
        _inputReader.onBlockPerformed -= BlockToggle;
    }
}
