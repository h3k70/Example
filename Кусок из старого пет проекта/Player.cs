using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [SerializeField] private float _maxHP = 100;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _takeDamageAudio;
    [SerializeField] private Move _move;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Skill[] _allSkills;
    [SerializeField] private SwordAttack _baseAttack;
    [SerializeField] private Shuriken _shurikenAttack;
    [SerializeField] private SmokeScreen _smokeScreen;
    [SerializeField] private Healing _healing;
    [SerializeField] private Transform _dieCameraPoint;
    [SerializeField] private Transform _nearCameraPoint;

    private Health _health = new();
    private PlayerInput _inputs;
    private bool _isSwipeEnded = true;
    private bool _isCanCast = false;
    private bool _isCanTakeDamage = true;
    private bool _isDead = false;
    private Vector2 _swipeStartPosition;
    private float _minSwipeDistance = 70;
    private float _currentCoins = 5968403;
    private float _safeReviveRadius = 20;

    public Health Health { get => _health; }
    public Move Move { get => _move; }
    public float Coins { get => _currentCoins; }
    public Transform DieCameraPoint { get => _dieCameraPoint; }
    public Skill BaseAttack { get => _baseAttack; }
    public Skill ShurikenSkill { get => _shurikenAttack; }
    public Skill SmokeSkill { get => _smokeScreen; }
    public Skill HealSkill { get => _healing; }
    public bool IsCanTakeDamage { get => _isCanTakeDamage; set => _isCanTakeDamage = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Skill[] AllSkills { get => _allSkills; }
    public bool IsCanCast { get => _isCanCast; set => _isCanCast = value; }
    public Transform NearCameraPoint { get => _nearCameraPoint; }
    public PlayerInput Inputs { get => _inputs; }
    public Animator Animator { get => _animator; }

    public Action Died;
    public Action Revived;
    public Action DamageTaked;
    public Action<float> CoinCountChanged;

    private void Awake()
    {
        _inputs = new PlayerInput();

        _inputs.Player.Attack.performed += context => OnAttack();
        _inputs.Player.Move.started += context => OnSwipeStarted(context.ReadValue<Vector2>());
        _inputs.Player.Move.performed += context => OnSwipe(context.ReadValue<Vector2>());
        _inputs.Player.Move.canceled += context => OnSwipeEnded(context.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        _inputs.Enable();
    }

    private void OnDisable()
    {
        _inputs.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Build>(out Build build))
        {
            _move.CloserBuild = build;
        }
    }

    public void Init(Transform runPoint, Build build = null)
    {
        _health.Init(_maxHP, _maxHP);
        _move.Init(runPoint, build);

        _health.Ended += Die;

        foreach (var item in _allSkills)
        {
            item.Init(this);
        }
    }

    public void TakeDamage(float value)
    {
        if (_isCanTakeDamage == false)
            return;

        _animator.SetTrigger(PlayerAnimHash.JumpStan);
        _takeDamageAudio.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        _takeDamageAudio.Play();

        _health.Take(value);

        DamageTaked?.Invoke();
    }

    public void AddCoin(int value)
    {
        _currentCoins += value;

        CoinCountChanged?.Invoke(_currentCoins);
    }

    public bool TrySpendCoin(float value)
    {
        if (_currentCoins >= value)
        {
            _currentCoins -= value;
            CoinCountChanged?.Invoke(_currentCoins);
            return true;
        }
        return false;
    }

    public void Revive()
    {
        _isDead = false;
        _animator.SetTrigger(PlayerAnimHash.Revive);
        _inputs.Player.Enable();
        _health.Add(_health.MaxValue);
        _move.StartRun();

        Collider[] colliders = Physics.OverlapSphere(transform.position, _safeReviveRadius, _enemyLayer);

        foreach (var item in colliders)
            item.GetComponent<Enemy>().TakeDamage();

        Revived?.Invoke();
    }

    public void SetHubState()
    {
        _isCanCast = false;
        _animator.SetTrigger(PlayerAnimHash.Idle);
        _isDead = false;
        _inputs.Player.Enable();
        _health.Add(_health.MaxValue);
        _move.StopRun();
    }

    private void Die()
    {
        _isDead = true;
        _inputs.Player.Disable();
        _move.StopRun();
        _animator.SetTrigger(PlayerAnimHash.Die);

        Died?.Invoke();
    }

    private void OnSwipeStarted(Vector2 dir)
    {
        _swipeStartPosition = dir;

        if (dir.y == 1 || dir.y == -1)
        {
            _move.JumpToSpline(dir.y);
        }
    }

    private void OnSwipe(Vector2 position)
    {
        Vector2 swipeDelta = position - _swipeStartPosition;

        if (_isSwipeEnded && (swipeDelta).magnitude > _minSwipeDistance)
        {
            if (swipeDelta.y > 0)
            {
                _move.JumpToSpline(1);
            }
            else
            {
                _move.JumpToSpline(-1);
            }
            _isSwipeEnded = false;
        }
    }

    private void OnSwipeEnded(Vector2 dir)
    {
        _swipeStartPosition = Vector2.zero;
        _isSwipeEnded = true;
    }

    private void OnAttack()
    {
        if (EventSystem.current.IsPointerOverGameObject() == false)
            _baseAttack.Cast();
    }
}