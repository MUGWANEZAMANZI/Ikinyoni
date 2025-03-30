---
uid: manage-textures
---

# Manage generated textures

You can manage the generated images in the **Generations** panel.

## Reuse generation settings

You can reuse prompt, negative prompt, reference image, and mask image from a previous generation to generate similar images.

1. Right-click the image whose settings you want to reuse.
1. Select **Generation Settings**. The **Generation Settings** window appears.
1. Do the following:

   * To reuse all previous settings, select **Use All**. This copies all settings to the **Generation** area.
   * To reuse only some settings, select **Use**. This copies only the selected settings to the **Generation** area.
1. Select **Generate**.

## Star

To mark an image as a favorite, select the **Star** icon. To remove the star, select the **Star** icon again.

## Filter

To filter the generated images, enter a keyword in the **Search** box. The keyword can be a full or partial match to the prompt you used to generate the textures.

To show only starred images, select the **Star** icon next to the **Search** box.

## Scale

To make the generated images bigger or smaller, move the slider at the bottom of the **Generations** panel.

## Export

You can export a texture as a .png file in your project and use it in your project like any other texture.

A [PBR material](xref:view-as-pbr) is exported as a .mat file with a matching folder containing the PBR maps in your project. You can use it in your project like any other [material](https://docs.unity3d.com/Manual/Materials.html).

To export one or more generated images:

1. Select the generated images you want to save. To select multiple images, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the images.
1. Do one of the following:

    * Drag the images to the `Assets` folder in the Project window.
    * Right-click a selected image, and select **Export**.

> [!TIP]
> When you close the **Muse Generator** window, it saves your **Muse Generator** thread with all images in it as a generator asset in your project. You can find the generator asset in the `Assets` folder in the Project window. You can reuse the generator asset to generate more images.

## Delete

To delete one or more generated images:

1. Select the generated image you want to delete. To select multiple images, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the images.
1. Right-click the image and select **Delete** (or press <kbd>Delete</kbd>).

## Additional resources

* [Write a prompt](xref:write-prompt)
* [Refine generated textures](xref:refine)
* [Generate textures](xref:generate)