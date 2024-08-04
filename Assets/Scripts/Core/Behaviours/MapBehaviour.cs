using System;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace MageTest.Core.Behaviours
{
    public class MapBehaviour : MonoBehaviour
    {
        [Inject]
        private readonly GameSettings _gameSettings;
        
        [SerializeField]
        private Tilemap _ground;
        [SerializeField]
        private TileBase _groundTile;
        [SerializeField]
        private TileBase _wallTile;

        private void Start()
        {
            int size = _gameSettings._mapSize;
            int halfSize = size / 2;

            // _ground.transform.position = new Vector3(
            //     _ground.size.x * -0.5f,
            //     _ground.size.y * -0.5f,
            //     0);
            
            _ground.ClearAllTiles();
            _ground.origin = new Vector3Int(-halfSize, -halfSize, 0);
            _ground.size = new Vector3Int(size, size, 1);
            _ground.ResizeBounds();
            
            _ground.BoxFill(Vector3Int.zero,
                _groundTile, 
                -halfSize, -halfSize,
                halfSize, halfSize);

            for (int i = -halfSize - 1; i <= halfSize; ++i)
            {
                _ground.SetTile(new Vector3Int(i, -halfSize - 1, 0), _wallTile);
                _ground.SetTile(new Vector3Int(i, halfSize, 0), _wallTile);
                _ground.SetTile(new Vector3Int(-halfSize - 1, i, 0), _wallTile);
                _ground.SetTile(new Vector3Int(halfSize, i, 0), _wallTile);
            }
        }
    }
}