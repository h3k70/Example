using System;
using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Spawner _spawner;
    [SerializeField] private Player _player;
    [SerializeField] private CameraFollow _cameraFollow;
    [SerializeField] private Transform _cameraGradeZone;

    [SerializeField] private MenuUI _menuUI;
    [SerializeField] private BarUI _HPBar;
    [SerializeField] private SourceUI _sourceUI;
    [SerializeField] private BloodFrameUI _bloodFrameUI;
    [SerializeField] private AbilityUI _baseAbilityUI;
    [SerializeField] private AbilityUI _firstAbilityUI;
    [SerializeField] private AbilityUI _secondAbilityUI;
    [SerializeField] private AbilityUI _thirdAbilityUI;

    private float _deleyForSourcePointAdd = 1;
    private float _pointCountForSourceAdd = 5;
    private Coroutine _sourcePointAddCoroutine;
    private Source _source = new();
    private float _bestSource;
    public Player Player { get => _player; }

    public Action RunEnded;
    public Action<float> BestSourceChanged;

    private void Start()
    {
        _player.Init(_spawner.StartChunk.RunPoint);
        _player.SetHubState();
        _source.Init(7);
        _spawner.Init(_player, _source);

        _player.DamageTaked += _bloodFrameUI.StartHitAnim;
        _player.Died += OnPlayerDied;

        _HPBar.Init(_player.Health);
        _sourceUI.Init(_spawner.Source, _player);
        _baseAbilityUI.Init(_player.BaseAttack);
        _firstAbilityUI.Init(_player.ShurikenSkill);
        _secondAbilityUI.Init(_player.SmokeSkill);
        _thirdAbilityUI.Init(_player.HealSkill);

        _cameraFollow.TargetMoveTransform = _player.NearCameraPoint;

        _menuUI.Init();
    }

    public void StartRun()
    {
        _source.ResetMe(7);
        _sourcePointAddCoroutine = StartCoroutine(AddSourcePointJob());

        _spawner.StartSpawnChunks();
        _player.Move.StartRun();
        _player.IsCanCast = true;
        _cameraFollow.TargetMoveTransform = null;
    }

    public void ContinueRun()
    {
        _sourcePointAddCoroutine = StartCoroutine(AddSourcePointJob());
        _player.Revive();
        _cameraFollow.TargetMoveTransform = null;
    }

    public void GoHome()
    {
        _player.SetHubState();
    }

    public void ShowGradeZone()
    {
        _cameraFollow.TargetMoveTransform = _cameraGradeZone;
    }
    
    public void ShowPlayerNear()
    {
        _cameraFollow.TargetMoveTransform = _player.NearCameraPoint;
    }

    private void OnPlayerDied()
    {
        StopCoroutine(_sourcePointAddCoroutine);

        if (_source.Total > _bestSource)
        {
            _bestSource = _source.Total;
            BestSourceChanged?.Invoke(_bestSource);
        }
        StartCoroutine(PlayerDiedJob());
    }

    private IEnumerator PlayerDiedJob()
    {
        _cameraFollow.TargetMoveTransform = _player.DieCameraPoint;
        yield return new WaitForSecondsRealtime(1.5f);
        RunEnded?.Invoke();
    }

    private IEnumerator AddSourcePointJob()
    {
        while (true)
        {
            yield return new WaitForSeconds(_deleyForSourcePointAdd);
            _source.Add(_pointCountForSourceAdd);
        }
    }
}
