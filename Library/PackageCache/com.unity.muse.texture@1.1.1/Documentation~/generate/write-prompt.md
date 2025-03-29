---
uid: write-prompt
---

# Write a prompt

The **Muse Texture Generator** tool generates textures, not sprites. A texture is a 2D image that you can use to create a [material](https://docs.unity3d.com/Manual/Materials.html). A [sprite](https://docs.unity3d.com/Manual/Sprites.html) is a 2D image that you can use as a 2D element in a game. For example, you can use a texture to create a material for a 3D model of a tree, but use a sprite to create a 2D image of a tree.

The main difference between the Texture and the [Sprite Generator](https://com.unity.muse.sprite) is that Texture Generator creates textures, while the Sprite Generator produces sprite images. For example, in the Texture Generator, a prompt of `cat` generates a seamless image of cat fur, whereas the Sprite Generator generates a cat image.

Consider the tips and best practices described on this page when you write the prompt.

## Use keywords rather than instructions

To describe the texture you want to generate, use keywords instead of instructions. For example, use `wood floor` instead of `generate a wood floor texture`.

To ensure the separation of concepts, enter multiple keywords separated by commas. For example, inputting `sandstone red fabric` generates a sandy red colored fabric, while `sandstone, red fabric` generates sandstone that is tinted a bit red.

<!--## Be specific with your prompt

Use precise keywords that describe the texture type and incorporate the material or style you want in the texture, such as `rustic wood floor` or `geometric pattern`. The more specific your prompt, the more likely the generated textures match your expectations.

If color is important, mention the color palette you're interested in, such as `warm-toned wood texture` or `pastel watercolor texture`.-->

## Exclude elements from the generated textures

To exclude specific elements from generated textures, in the **Negative Prompt**, enter keywords that describe the elements to exclude. The keyword can be a color, shape, or texture type. 

> [!IMPORTANT]
> To avoid double negative, don't use `no` in the **Negative Prompt**. For example, if you want to generate blue tiles and you don’t want white tiles in it, In the **Prompt**, enter `blue tiles`. In the **Negative Prompt**, enter `white tiles` not `no white tiles`.

To ensure the separation of concepts, enter multiple keywords separated by commas.

## Additional resources

* [Refine generated textures](xref:refine)
* [Tool reference](xref:tool-reference)