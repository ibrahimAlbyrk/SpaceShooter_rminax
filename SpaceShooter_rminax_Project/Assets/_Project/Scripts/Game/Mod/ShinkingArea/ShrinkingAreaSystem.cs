using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _Project.Scripts.Game.Mod.ShrinkingArea
{
    using Data;
    using Network;
    using Spaceship;

    public class ShrinkingAreaSystem : NetIdentity
    {
        public static ShrinkingAreaSystem Instance;

        #region Serialize Variables
        
        private MapGeneratorData _data;

        [SerializeField] private ShrinkingData[] _states;

        [SerializeField] private GameObject _zonePrefab;

        [Header("Outside Settings")] [SerializeField]
        private float _outSideDetectTime = 2;

        #endregion

        #region Private Variables

        private readonly List<SpaceshipController> _ships = new();

        private readonly Dictionary<SpaceshipController, float> _outsideShips = new();

        private float _areaRange;

        private int _stateIndex;

        private Coroutine _shrinkingCor;

        private GameObject _zoneObj;

        #endregion

        #region Base Methods

        private void Awake()
        {
            Instance = this;
        }
        
        [Server]
        private void Start()
        {
            _data = GameManager.Instance.GetData();
            
            _areaRange = _data.GameAreaRadius;

            var data = _states[_stateIndex];

            StartCoroutine(OnShrinking(data));
            
            SpawnZone();
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            var currentState = _states[_stateIndex];

            CheckShipDistance();

            foreach (var (outsideShip, time) in _outsideShips.ToDictionary(x => x.Key, x => x.Value))
            {
                if(time > Time.time) continue;

                _outsideShips[outsideShip] = Time.time + _outSideDetectTime;
                outsideShip.Health.Remove(currentState.Damage);
            }
            
            UpdateZone();
        }

        #endregion

        #region Zone Methods

        [Server]
        private void SpawnZone()
        {
            _zoneObj = Instantiate(_zonePrefab, Vector3.zero, Quaternion.identity, transform);
            
            NetworkServer.Spawn(_zoneObj);
            
            UpdateZone();
        }

        [Server]
        private void UpdateZone()
        {
            _zoneObj.transform.localScale = Vector3.one * _areaRange;
        }

        #endregion
        
        #region Shrinking Area Methods

        private IEnumerator OnShrinking(ShrinkingData data)
        {
            yield return new WaitForSeconds(data.CooldownTime);

            var rangeDistance = Mathf.Abs(_areaRange - data.Range);

            var startingTime = Time.time;

            var firstAreaRange = _areaRange;
            
            while (rangeDistance > .1f)
            {
                var timePassed = Time.time - startingTime;
                
                _areaRange = Mathf.Lerp(firstAreaRange, data.Range, timePassed / data.ShrinkingTime);

                rangeDistance = Mathf.Abs(_areaRange - data.Range);

                yield return new WaitForFixedUpdate();
            }

            if (_stateIndex >= _states.Length - 1) yield break;

            _stateIndex++;

            var newData = _states[_stateIndex];

            if (_shrinkingCor != null) StopCoroutine(_shrinkingCor);

            _shrinkingCor = StartCoroutine(OnShrinking(newData));
        }

        private void CheckShipDistance()
        {
            foreach (var ship in _ships)
            {
                var shipTransform = ship.transform;
                var shipDistance = Vector3.Distance(Vector3.zero, shipTransform.position);

                if (shipDistance > _areaRange)
                {
                    _outsideShips.TryAdd(ship, Time.time + _outSideDetectTime);
                }
                else
                {
                    if (_outsideShips.ContainsKey(ship))
                    {
                        _outsideShips.Remove(ship);
                    }
                }
            }
        }

        #endregion

        #region Player Methods

        [Command(requiresAuthority = false)]
        public void CMD_AddPlayer(SpaceshipController ship) => AddPlayer(ship);

        [Command(requiresAuthority = false)]
        public void CMD_RemovePlayer(SpaceshipController ship) => RemovePlayer(ship);

        [Server]
        private void AddPlayer(SpaceshipController ship)
        {
            if (CheckPlayer(ship)) return;

            _ships.Add(ship);
        }

        [Server]
        private void RemovePlayer(SpaceshipController ship)
        {
            if (!CheckPlayer(ship)) return;

            _ships.Remove(ship);
        }

        #endregion

        #region Utilities

        private bool CheckPlayer(SpaceshipController ship)
        {
            return _ships.Any(s => s == ship);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            
            Gizmos.DrawWireSphere(Vector3.zero, _areaRange);
        }
    }
}