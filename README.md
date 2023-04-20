# MakeSKN
Created by Katzsmile, fork by The Hostile Negotiator

Tool for mass sorting extracted CnC W3X Files from Bibbers Asset Extracter and Darth Jane's Assimilator tools.

Individual mesh files and collision boxes belonging to a container will be merged into a single container file, additionally any Animation and Skeleton files 
that match the ID of the Container will also be merged into the singular file.

Files will be placed into the sub folders, same format that is used for art files with the Mod tools, with additional support with sorting textures.

Tool will sort different Level of Detail (LOD) versions if present

## How to Use
Extract the art you wish to use and place into a folder. If using Low LOD or Medium LOD then inside the folder, create a new folder and name it "LowLOD" and 
"MediumLOD" respectively place the LOD version of files into it. Using a command line, type MakeSKN.exe and type the location of the Folder. The tool will sort 
through all the W3X files. Once completed, a folder called "Compiled" is created and the final files will be located in there, sorted into subfolders.

## Special Cases
Bibbers extractor does not add "_CTR" to container files that end with SKN, however there are a few of examples where it would clash with animations with 
the same ID. It is recommended to extract the container first, manually add "_CTR" to the end of the file name and then extract the animation.
* GBMEDBAYD2_SKN: In Kane's Wrath, the Low LOD version has an animation matching ID
* AUHEALTHTENT_SKN: In Red Alert 3 and Uprising, the model has an animation matching ID
* BB_TUNA_SKN: In Red Alert 3 and Uprising, the model has an animation matching ID