using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    [Serializable]
    public struct StringSpriteEntry
    {
        public string key;
        public Sprite sprite;
    }

    [Serializable]
    public struct CharacterSpriteEntry
    {
        public CharacterArchetype archetype;
        public Sprite sprite;
    }

    [Serializable]
    public struct WeaponSpriteEntry
    {
        public WeaponKind weaponKind;
        public Sprite sprite;
    }

    [Serializable]
    public struct StringTextureEntry
    {
        public string key;
        public Texture texture;
    }

    [Serializable]
    public struct CharacterTextureEntry
    {
        public CharacterArchetype archetype;
        public Texture texture;
    }

    [Serializable]
    public struct WeaponTextureEntry
    {
        public WeaponKind weaponKind;
        public Texture texture;
    }

    [Serializable]
    public class UIImageArtworkSlot
    {
        public string slotName = "UI Image";
        public Image target;
        public Sprite sprite;
        public bool preserveAspect = true;
        public bool setNativeSize;
        public bool disableTargetWhenSpriteMissing;
        public bool overrideColor;
        public Color color = Color.white;

        public void Apply()
        {
            if (target == null)
            {
                return;
            }

            target.sprite = sprite;
            target.preserveAspect = preserveAspect;

            if (overrideColor)
            {
                target.color = color;
            }

            bool hasSprite = sprite != null;
            target.enabled = hasSprite || !disableTargetWhenSpriteMissing;
            if (hasSprite && setNativeSize)
            {
                target.SetNativeSize();
            }
        }
    }

    [Serializable]
    public class UIUnitArtworkLibrary
    {
        public Sprite defaultSprite;
        public StringSpriteEntry[] spritesByDisplayName = new StringSpriteEntry[0];
        public CharacterSpriteEntry[] spritesByArchetype = new CharacterSpriteEntry[0];
        public WeaponSpriteEntry[] spritesByWeapon = new WeaponSpriteEntry[0];

        public Sprite Resolve(CombatUnit unit)
        {
            if (unit == null)
            {
                return defaultSprite;
            }

            Sprite resolved = UIArtworkUtility.FindSprite(spritesByDisplayName, unit.displayName);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = UIArtworkUtility.FindSprite(spritesByArchetype, unit.archetype);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = UIArtworkUtility.FindSprite(spritesByWeapon, unit.weaponKind);
            return resolved != null ? resolved : defaultSprite;
        }
    }

    [Serializable]
    public class UICardArtworkLibrary
    {
        public Sprite defaultSprite;
        public StringSpriteEntry[] spritesByCardId = new StringSpriteEntry[0];
        public StringSpriteEntry[] spritesByDisplayName = new StringSpriteEntry[0];

        public Sprite Resolve(CardDefinition card)
        {
            if (card == null)
            {
                return defaultSprite;
            }

            Sprite resolved = UIArtworkUtility.FindSprite(spritesByCardId, card.cardId);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = UIArtworkUtility.FindSprite(spritesByDisplayName, card.displayName);
            return resolved != null ? resolved : defaultSprite;
        }
    }

    [Serializable]
    public class UIUnitImageBinding
    {
        public string slotName = "Unit Portrait";
        public Image targetImage;
        public UIUnitArtworkLibrary artwork = new UIUnitArtworkLibrary();
        public bool preserveAspect = true;
        public bool hideTargetWhenSpriteMissing = true;
        public bool overrideColor;
        public Color color = Color.white;

        public void Apply(CombatUnit unit)
        {
            if (targetImage == null)
            {
                return;
            }

            Sprite sprite = artwork != null ? artwork.Resolve(unit) : null;
            targetImage.sprite = sprite;
            targetImage.preserveAspect = preserveAspect;

            if (overrideColor)
            {
                targetImage.color = color;
            }

            targetImage.enabled = sprite != null || !hideTargetWhenSpriteMissing;
        }
    }

    [Serializable]
    public class UICardImageBinding
    {
        public string slotName = "Card Illustration";
        public Image targetImage;
        public UICardArtworkLibrary artwork = new UICardArtworkLibrary();
        public bool preserveAspect = true;
        public bool hideTargetWhenSpriteMissing;
        public bool overrideColor;
        public Color color = Color.white;

        public void Apply(CardDefinition card, UICardArtworkLibrary runtimeOverride = null)
        {
            if (targetImage == null)
            {
                return;
            }

            UICardArtworkLibrary library = runtimeOverride ?? artwork;
            Sprite sprite = library != null ? library.Resolve(card) : null;
            targetImage.sprite = sprite;
            targetImage.preserveAspect = preserveAspect;

            if (overrideColor)
            {
                targetImage.color = color;
            }

            targetImage.enabled = sprite != null || !hideTargetWhenSpriteMissing;
        }
    }

    [Serializable]
    public class UIUnitSpriteRendererBinding
    {
        public string slotName = "Unit Sprite Renderer";
        public SpriteRenderer targetRenderer;
        public UIUnitArtworkLibrary artwork = new UIUnitArtworkLibrary();
        public bool hideRendererWhenSpriteMissing;
        public bool overrideColor;
        public Color color = Color.white;

        public void Apply(CombatUnit unit)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Sprite sprite = artwork != null ? artwork.Resolve(unit) : null;
            targetRenderer.sprite = sprite;

            if (overrideColor)
            {
                targetRenderer.color = color;
            }

            targetRenderer.enabled = sprite != null || !hideRendererWhenSpriteMissing;
        }
    }

    [Serializable]
    public class UIUnitTextureRendererBinding
    {
        public string slotName = "Unit Texture Renderer";
        public Renderer targetRenderer;
        public string textureProperty = "_MainTex";
        public string colorProperty = "_Color";
        public Texture defaultTexture;
        public StringTextureEntry[] texturesByDisplayName = new StringTextureEntry[0];
        public CharacterTextureEntry[] texturesByArchetype = new CharacterTextureEntry[0];
        public WeaponTextureEntry[] texturesByWeapon = new WeaponTextureEntry[0];
        public bool hideRendererWhenTextureMissing;
        public bool overrideColor;
        public Color color = Color.white;

        public void Apply(CombatUnit unit)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Texture texture = Resolve(unit);
            targetRenderer.enabled = texture != null || !hideRendererWhenTextureMissing;
            if (texture == null)
            {
                return;
            }

            Material material = targetRenderer.material;
            if (material != null && !string.IsNullOrEmpty(textureProperty) && material.HasProperty(textureProperty))
            {
                material.SetTexture(textureProperty, texture);
            }

            if (material != null && overrideColor && !string.IsNullOrEmpty(colorProperty) && material.HasProperty(colorProperty))
            {
                material.SetColor(colorProperty, color);
            }
        }

        private Texture Resolve(CombatUnit unit)
        {
            if (unit == null)
            {
                return defaultTexture;
            }

            Texture resolved = UIArtworkUtility.FindTexture(texturesByDisplayName, unit.displayName);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = UIArtworkUtility.FindTexture(texturesByArchetype, unit.archetype);
            if (resolved != null)
            {
                return resolved;
            }

            resolved = UIArtworkUtility.FindTexture(texturesByWeapon, unit.weaponKind);
            return resolved != null ? resolved : defaultTexture;
        }
    }

    public static class UIArtworkUtility
    {
        public static void ApplySlots(UIImageArtworkSlot[] slots)
        {
            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].Apply();
                }
            }
        }

        public static Sprite FindSprite(StringSpriteEntry[] entries, string key)
        {
            if (entries == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (string.Equals(entries[i].key, key, StringComparison.OrdinalIgnoreCase) && entries[i].sprite != null)
                {
                    return entries[i].sprite;
                }
            }

            return null;
        }

        public static Sprite FindSprite(CharacterSpriteEntry[] entries, CharacterArchetype archetype)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].archetype == archetype && entries[i].sprite != null)
                {
                    return entries[i].sprite;
                }
            }

            return null;
        }

        public static Sprite FindSprite(WeaponSpriteEntry[] entries, WeaponKind weaponKind)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].weaponKind == weaponKind && entries[i].sprite != null)
                {
                    return entries[i].sprite;
                }
            }

            return null;
        }

        public static Texture FindTexture(StringTextureEntry[] entries, string key)
        {
            if (entries == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (string.Equals(entries[i].key, key, StringComparison.OrdinalIgnoreCase) && entries[i].texture != null)
                {
                    return entries[i].texture;
                }
            }

            return null;
        }

        public static Texture FindTexture(CharacterTextureEntry[] entries, CharacterArchetype archetype)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].archetype == archetype && entries[i].texture != null)
                {
                    return entries[i].texture;
                }
            }

            return null;
        }

        public static Texture FindTexture(WeaponTextureEntry[] entries, WeaponKind weaponKind)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].weaponKind == weaponKind && entries[i].texture != null)
                {
                    return entries[i].texture;
                }
            }

            return null;
        }
    }
}
