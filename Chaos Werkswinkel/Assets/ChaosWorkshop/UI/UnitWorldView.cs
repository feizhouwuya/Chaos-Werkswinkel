using UnityEngine;

namespace ChaosWorkshop
{
    public class UnitWorldView : MonoBehaviour
    {
        public CombatUnit unit;
        public BattleManager battleManager;
        public Vector3 leftWorld = new Vector3(-8f, 0f, 0f);
        public Vector3 rightWorld = new Vector3(8f, 0f, 0f);

        [Header("Artwork")]
        public UIUnitSpriteRendererBinding spriteRendererBinding = new UIUnitSpriteRendererBinding();
        public UIUnitTextureRendererBinding textureRendererBinding = new UIUnitTextureRendererBinding();

        private void Awake()
        {
            AutoBindRenderers();
            ApplyArtwork();
        }

        private void OnValidate()
        {
            AutoBindRenderers();
        }

        private void LateUpdate()
        {
            if (unit == null || battleManager == null)
            {
                return;
            }

            float t = Mathf.InverseLerp(battleManager.arenaMin, battleManager.arenaMax, unit.ArenaPosition);
            transform.position = Vector3.Lerp(leftWorld, rightWorld, t);
        }

        private void OnMouseDown()
        {
            if (battleManager != null && unit != null)
            {
                battleManager.SelectTarget(unit);
            }
        }

        public void ApplyArtwork()
        {
            if (spriteRendererBinding != null)
            {
                spriteRendererBinding.Apply(unit);
            }

            if (textureRendererBinding != null)
            {
                textureRendererBinding.Apply(unit);
            }
        }

        private void AutoBindRenderers()
        {
            if (spriteRendererBinding != null && spriteRendererBinding.targetRenderer == null)
            {
                spriteRendererBinding.targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (textureRendererBinding != null && textureRendererBinding.targetRenderer == null)
            {
                textureRendererBinding.targetRenderer = GetComponent<Renderer>();
            }
        }
    }
}
