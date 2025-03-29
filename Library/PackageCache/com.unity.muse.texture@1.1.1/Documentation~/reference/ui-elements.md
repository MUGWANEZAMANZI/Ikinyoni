---
uid: ui-elements
---

# UI elements

This page describes the UI elements of the **Muse Texture** tool.

## Muse Generator window

| **Property** | **Description** |
| --- | --- |
| **Images** | Sets the number of images to be generated. |
| **Generate** | Starts generating textures. Generated textures appear in the **Generations** panel. |
| **Prompt** | Text input used to generate textures. |
| **Negative Prompt** | Text that describes the elements to exclude from the generated textures. |
| **Generations** | Previews and provides actions for the generated textures. |
| **Scale slider** | Makes the generated images bigger and smaller. |

## Refine panel

| **Property** | **Description** |
| --- | --- |
| **Mask Image** | Shows the masked area. This option is only available when you select **Paint**. |
| **Hand** | Deselects a refine tool. |
| **Paint** | Activates the painting brush to create and edit a mask. |
| **Erase** | Activates the eraser to erase the mask.|
| **Clear** | Clears the canvas. |
| **Radius** | Sets the size of the brush or eraser. |

## Input Image panel

| **Property** | **Description** |
| --- | --- |
| **Import** | Imports an image to use as the reference image. |
| **Strength** | Determines how closely the generated textures follow the reference. Higher values result in closer matching.|
| **Patterns** | Selects a pattern to use as the reference pattern. |

## Image context menu items

The following table describes the context menu (&#8230;) items of a generated image:

| **Property**             | **Description** |
|--------------------------| --- |
| **Generation Settings**  | Displays and reuses the prompt, negative prompt, the mask, and reference images used in the generation. |
| **Export**               | Exports the sprite to the project's `Assets` folder. |
| **Upscale**              | [Converts the selected texture to a 4x resolution](xref:upscale) of `2048` x `2048` pixels.|
| **View as PBR Material** | [Converts the generated texture to PBR material](xref:view-as-pbr). |
| **View as Image**        | Converts the PBR material back to a texture. |
| **Create Variations**    | [Creates variations](xref:create-variations) of the generated texture. |
| **Set as Reference**     | [Sets the generated texture as a reference](xref:set-as-reference) for further generations. |

## Unity Muse bar

| **Property** | **Description** |
| --- | --- |
| **Muse Points Used** | Displays the number of [Muse points you have used and the total points you have](https://unity.com/ai/faq). There are no point deductions for unsuccessful generations. |
| **Go to Muse account** | Opens the [Muse account](https://id.unity.com/en/account/edit) page in your browser. |

## Additional resources

* [Generate textures](xref:generate)
* [Refine generated textures](xref:refine)
* [Keyboard shortcuts](xref:keyboard-shortcuts)