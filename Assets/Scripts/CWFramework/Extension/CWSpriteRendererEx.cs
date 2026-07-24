using UnityEngine;

namespace CWFramework
{
    public static class CWSpriteRendererEx
    {
        private static readonly int FlashEnabledId = Shader.PropertyToID("_FlashEnabled");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashStrengthId = Shader.PropertyToID("_FlashStrength");
        private static readonly int FlashPhaseId = Shader.PropertyToID("_FlashPhase");
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

        public static MaterialPropertyBlock SetFlash(
            this SpriteRenderer renderer,
            MaterialPropertyBlock propertyBlock,
            bool enabled,
            Color color,
            float strength,
            float phase)
        {
            if (renderer == null)
            {
                return propertyBlock;
            }

            propertyBlock ??= new MaterialPropertyBlock();

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FlashEnabledId, enabled ? 1f : 0f);
            propertyBlock.SetColor(FlashColorId, color);
            propertyBlock.SetFloat(FlashStrengthId, strength);
            propertyBlock.SetFloat(FlashPhaseId, phase);
            renderer.SetPropertyBlock(propertyBlock);

            return propertyBlock;
        }

        public static MaterialPropertyBlock SetFlash(
            this SpriteRenderer renderer,
            MaterialPropertyBlock propertyBlock,
            Color color,
            float strength,
            float phase)
        {
            return renderer.SetFlash(propertyBlock, true, color, strength, phase);
        }

        public static MaterialPropertyBlock ClearFlash(
            this SpriteRenderer renderer,
            MaterialPropertyBlock propertyBlock)
        {
            return renderer.SetFlash(propertyBlock, false, Color.white, 0f, 0f);
        }

        public static MaterialPropertyBlock SetOutline(
            this SpriteRenderer renderer,
            MaterialPropertyBlock propertyBlock,
            bool enabled)
        {
            if (renderer == null)
            {
                return propertyBlock;
            }

            propertyBlock ??= new MaterialPropertyBlock();

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(OutlineEnabledId, enabled ? 1f : 0f);
            renderer.SetPropertyBlock(propertyBlock);

            return propertyBlock;
        }

        public static MaterialPropertyBlock SetOutline(
            this SpriteRenderer renderer,
            MaterialPropertyBlock propertyBlock,
            bool enabled,
            Color color)
        {
            if (renderer == null)
            {
                return propertyBlock;
            }

            propertyBlock ??= new MaterialPropertyBlock();

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(OutlineEnabledId, enabled ? 1f : 0f);
            propertyBlock.SetColor(OutlineColorId, color);
            renderer.SetPropertyBlock(propertyBlock);

            return propertyBlock;
        }
    }
}
