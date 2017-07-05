# Project Description

Sprite Generator makes it easier for web developers to create sprite images. It's developed in C#.
This application creates a nearly optimal insertions of images into a CSS Sprite. The major algorithms used rest on O-tree based rectangle packing.

This application was originally developed by [csigusz](https://spritegenerator.codeplex.com/) and it's code was located at his [CodePlex](https://spritegenerator.codeplex.com/) repository. The project was abandoned, so I decided to forge and add some extra stuff.

# How to use:

The application contains a list of self-descriptive configuration. You will be able to configure the input and output path, the file name and the layout for the generated CSS file.

Paths:

- Images directory: Only JPG/JPEG,  PNG, GIF image formats are allowed.
- Output path: The directory where the generated CSS file will be stored. 
- File name: The generated file name. 

Layout:

- Automatic: image layout based on the algorithms mentioned above.
- Horizontal/ Vertical: simple horizontal/vertical image layout only for images with the same height/width.
- Rectangular: rectangular layout for images with the same height and the same width.

Distances:

- Distance between images: distance in pixels. This option does not set margin width for the sprite.
- Margin width: The margin between each image in pixels.

CSS

- Prefix: a prefix for all the CSS classes.

# Reference:

Pei-Ning Guo, Toshihiko Takahashi, Chung-Kuan Cheng -"Floorplanning Using a Tree Representation",  IEEE Transaction On Computer-aided Design Of Integrated Circuits And System, Vol. 20, No. 2, 2001. [PDF](http://users.ece.gatech.edu/limsk/www/pdfs/0281guo.pdf)
