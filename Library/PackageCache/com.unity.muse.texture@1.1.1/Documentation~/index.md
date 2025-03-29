---
uid: get-started
---

# Get started with Muse Texture

Muse Texture is a tool that uses machine learning to generate textures based on the prompt you enter. You can set the number of textures to generate. You can also apply the generated textures directly to objects in your **Scene** view.

## Launch the tool

1. Install the `com.unity.muse.texture` package if you haven't done so. For instructions, refer to [Install a package from a registry by name](https://docs.unity3d.com/Manual/upm-ui-quick.html).
1. From the menu, select **Muse** > **Muse Texture**.
1. Resize the **Muse Generator** window until you can see all the UI.

> [!TIP]
> When possible, dock the **Muse Generator** window. This keeps you from losing the window and launching it multiple times.

## Generate textures from a prompt

1. Enter a [prompt](xref:write-prompt).
1. To set the number of textures to generate, move the **Images** slider or enter a number in the **Images** field.
1. Select **Generate**. The textures appear in the **Generations** panel.
1. To convert the generated textures to [PBR materials](xref:view-as-pbr), right-click on it and select **View as PBR Material**.

## Apply generated textures

To apply the generated texture or PBR material directly in your project, drag it from the **Generations** panel onto the object in your **Scene** view.

## Additional resources

* [Unity AI FAQ](https://unity.com/ai/faq)
* [Muse feature setup](https://unity.com/products/muse/onboarding)
* [Muse Sprite](https://docs.unity3d.com/Packages/com.unity.muse.sprite@latest)