using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;

        [Range(0f, 1f)]
        public float parallaxFactor = 0.1f;

        [HideInInspector] public Transform[] tiles;
        [HideInInspector] public float tileWidth;
    }

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Parallax Layers")]
    [SerializeField] private ParallaxLayer[] layers;

    private Vector3 previousCameraPosition;

    private void Start()
    {
        // Busca la cámara de Core automáticamente
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("ParallaxBackground: No se encontró la cámara.");
            enabled = false;
            return;
        }

        previousCameraPosition = cameraTransform.position;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layer == null)
                continue;

            Renderer renderer = layer.layer.GetComponentInChildren<Renderer>();

            if (renderer == null)
            {
                Debug.LogWarning("La capa " + layer.layer.name + " no tiene ningún Renderer.");
                continue;
            }

            layer.tileWidth = renderer.bounds.size.x;

            if (layer.tileWidth <= 0f)
                continue;

            layer.tiles = new Transform[3];
            layer.tiles[1] = layer.layer;

            for (int i = 0; i < 2; i++)
            {
                GameObject copy = Instantiate(layer.layer.gameObject, layer.layer.parent);
                copy.name = layer.layer.name + "_Tile_" + i;
                layer.tiles[i == 0 ? 0 : 2] = copy.transform;
            }

            Vector3 center = layer.tiles[1].position;

            layer.tiles[0].position = center + Vector3.left * layer.tileWidth;
            layer.tiles[2].position = center + Vector3.right * layer.tileWidth;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null || Camera.main == null)
            return;

        Vector3 cameraDelta = cameraTransform.position - previousCameraPosition;

        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        float cameraLeft = cameraTransform.position.x - halfWidth;
        float cameraRight = cameraTransform.position.x + halfWidth;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.tiles == null || layer.tileWidth <= 0f)
                continue;

            // Movimiento parallax
            foreach (Transform tile in layer.tiles)
            {
                if (tile == null) continue;

                tile.position += new Vector3(
                    cameraDelta.x * layer.parallaxFactor,
                    cameraDelta.y * layer.parallaxFactor,
                    0f
                );
            }

            // Reciclado de tiles (infinite scrolling)
            foreach (Transform tile in layer.tiles)
            {
                if (tile == null) continue;

                Renderer renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer == null) continue;

                if (renderer.bounds.max.x < cameraLeft)
                {
                    tile.position += Vector3.right * layer.tileWidth * 3f;
                }
                else if (renderer.bounds.min.x > cameraRight)
                {
                    tile.position += Vector3.left * layer.tileWidth * 3f;
                }
            }
        }

        previousCameraPosition = cameraTransform.position;
    }
}