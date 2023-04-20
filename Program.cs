using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;

enum RenderObjectType
{
    Mesh,
    CollisionBox
};

enum LevelOfDetail
{
    High,
    Medium,
    Low
};

namespace makeskn
{
    class Program
    {
        static int errors = 0;

        static void EmitError(string e)
        {
            Console.WriteLine("MakeSKN: Error: {0}", e);
            ++errors;
        }

        static bool checkLOD(string w3xfile, string fPath, LevelOfDetail LODType)
        {
            string fPathLowLOD = "";
            string LODVersion = "";
            switch (LODType)
            {
                case LevelOfDetail.Medium:
                    fPathLowLOD = Path.Combine(fPath, "MediumLOD");
                    LODVersion = "Medium";
                    break;
                case LevelOfDetail.Low:
                    fPathLowLOD = Path.Combine(fPath, "LowLOD");
                    LODVersion = "Low";
                    break;
            }
            bool IsLowLOD = false;
            string fileName = Path.GetFileName(w3xfile);

            if (File.Exists(w3xfile))
            {
                try
                {
                    string Skeleton = "";
                    string Container = "";
                    ArrayList SubObjects = new ArrayList();
                    ArrayList SubObjectTypes = new ArrayList();
                    ArrayList TexturesList = new ArrayList();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(w3xfile);
                    string nXML = w3xfile;
                    XmlNodeList AssetDeclarations = xDoc.GetElementsByTagName("AssetDeclaration");
                    XmlNodeList W3DContainers = xDoc.GetElementsByTagName("W3DContainer");
                    XmlNodeList W3DMeshes = xDoc.GetElementsByTagName("W3DMesh");
                    XmlNodeList W3DCollisionBoxes = xDoc.GetElementsByTagName("W3DCollisionBox");
                    XmlElement newIncludes = xDoc.CreateElement("Includes", "uri:ea.com:eala:asset");
                        
                    if (W3DContainers.Count != 0)
                    {
                        // Check if LowLOD Container
                        if (File.Exists(Path.Combine(fPathLowLOD, $"{fileName}")))
                        {
                            Console.Write($"\nHas {LODVersion} LOD Container");
                            return true;
                        }
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            foreach (XmlNode W3DContainer in W3DContainers)
                            {
                                XmlNamedNodeMap mapAttributes = W3DContainer.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        xnodAttribute.Value = xnodAttribute.Value.ToUpperInvariant();
                                        Skeleton = xnodAttribute.Value;
                                    }
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Container = xnodAttribute.Value;
                                    }
                                }

                                if (!String.IsNullOrEmpty(Skeleton))
                                {
                                    // If Container ID and Skeleton ID is the same, should be in same file
                                    if (Container.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                                    {
                                        if (File.Exists(Path.Combine(fPathLowLOD, $"{Skeleton}_HRC.w3x")))
                                        {
                                            Console.Write($"\nHas {LODVersion} LOD Skeleton");
                                            // Create LowLOD container in LowLOD folder
                                            File.Copy(Path.Combine(fPath, $"{fileName}"), Path.Combine(fPathLowLOD, $"{fileName}"), true);
                                            return true;
                                        }
                                    }
                                }

                                // Search for animation files that have the same ID as the Container
                                if (File.Exists(Path.Combine(fPath, $"{Container}.w3x")))
                                {
                                    if (File.Exists(Path.Combine(fPathLowLOD, $"{Container}.w3x")))
                                    {
                                        // Check that it is Animation
                                        string AnimationFile = Path.Combine(fPathLowLOD, $"{Container}.w3x");

                                        XmlTextReader reader = new XmlTextReader(AnimationFile);
                                        // Check that it is an Animation File
                                        if (reader.ReadToFollowing("W3DAnimation"))
                                        {
                                            reader.Close();
                                            Console.Write($"\nHas {LODVersion} LOD Animation");
                                            // Create LowLOD container in LowLOD folder
                                            File.Copy(Path.Combine(fPath, $"{fileName}"), Path.Combine(fPathLowLOD, $"{fileName}"), true);
                                            return true;
                                        }
                                        else
                                        {
                                            reader.Close();
                                        }
                                    }
                                }

                                XmlNodeList RenderObjects = xDoc.GetElementsByTagName("RenderObject");
                                foreach (XmlNode RenderObject in RenderObjects)
                                {
                                    string strValue = (string)RenderObject.FirstChild.InnerText;
                                    if (RenderObject.FirstChild.Name == "Mesh")
                                    {
                                        SubObjectTypes.Add(RenderObjectType.Mesh);
                                    }
                                    else if (RenderObject.FirstChild.Name == "CollisionBox")
                                    {
                                        SubObjectTypes.Add(RenderObjectType.CollisionBox);
                                    }
                                    SubObjects.Add(strValue);
                                }

                                if (SubObjects.Count > 0)
                                {
                                    for (int i = 0; i < SubObjects.Count; i++)
                                    {
                                        if (File.Exists(Path.Combine(fPathLowLOD, $"{SubObjects[i]}.w3x")))
                                        {
                                            Console.Write($"\nHas {LODVersion} LOD RenderObject");
                                            // Create LowLOD container in LowLOD folder
                                            File.Copy(Path.Combine(fPath, $"{fileName}"), Path.Combine(fPathLowLOD, $"{fileName}"), true);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write("[ERROR]\n");
                    EmitError("Failed with exception: {0}" + Convert.ToString(e));
                }
            }
            return IsLowLOD;

        }

        static void buildfile(string w3xfile, string fPath, LevelOfDetail LODType)
        {
            if (File.Exists(w3xfile))
            {
                try
                {
                    string Skeleton = "";
                    string Container = "";
                    ArrayList SubObjects = new ArrayList();
                    ArrayList SubObjectTypes = new ArrayList();
                    ArrayList TexturesList = new ArrayList();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(w3xfile);
                    string nXML = w3xfile;
                    XmlNodeList AssetDeclarations = xDoc.GetElementsByTagName("AssetDeclaration");
                    XmlNodeList W3DContainers = xDoc.GetElementsByTagName("W3DContainer");
                    XmlNodeList W3DSkeletons = xDoc.GetElementsByTagName("W3DHierarchy");
                    XmlNodeList W3DAnimations = xDoc.GetElementsByTagName("W3DAnimation");
                    XmlNodeList W3DMeshes = xDoc.GetElementsByTagName("W3DMesh");
                    XmlNodeList W3DCollisionBoxes = xDoc.GetElementsByTagName("W3DCollisionBox");
                    XmlElement newIncludes = xDoc.CreateElement("Includes", "uri:ea.com:eala:asset");

                    string fPathLowLOD = "";
                    string LODPostFix = "";
                    string LODVersion = "";
                    switch (LODType)
                    {
                        case LevelOfDetail.Medium:
                            fPathLowLOD = Path.Combine(fPath, "MediumLOD");
                            LODPostFix = "_M";
                            LODVersion = "Medium";
                            break;
                        case LevelOfDetail.Low:
                            fPathLowLOD = Path.Combine(fPath, "LowLOD");
                            LODPostFix = "_L";
                            LODVersion = "Low";
                            break;
                    }

                    bool islowLOD = false;

                    if (LODType == LevelOfDetail.Medium || LODType == LevelOfDetail.Low)
                    {
                        islowLOD = true;
                    }

                    if (W3DContainers.Count != 0)
                    {
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            foreach (XmlNode W3DContainer in W3DContainers)
                            {
                                XmlNamedNodeMap mapAttributes = W3DContainer.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        xnodAttribute.Value = xnodAttribute.Value.ToUpperInvariant();
                                        Skeleton = xnodAttribute.Value;
                                    }
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Container = xnodAttribute.Value;
                                    }
                                }

                                if (islowLOD)
                                {
                                    Console.Write($"\nProcessing {LODVersion} LOD W3X Container: {Container}");
                                }
                                else
                                {
                                    Console.Write($"\nProcessing W3X Container: {Container}");
                                }

                                if (!String.IsNullOrEmpty(Skeleton))
                                {
                                    // If Container ID and Skeleton ID is the same, should be in same file
                                    if (Container.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                                    {
                                        string SkeletonFile;

                                        bool SkeletonLowLOD;

                                        // Bibber's skeleton extension "_HRC". Not applied to SKL but normally the containers won't match the name in this case
                                        if (File.Exists(Path.Combine(fPathLowLOD, $"{Skeleton}_HRC.w3x")) && islowLOD)
                                        {
                                            SkeletonFile = Path.Combine(fPathLowLOD, $"{Skeleton}_HRC.w3x");
                                            SkeletonLowLOD = true;
                                        }
                                        else
                                        {
                                            SkeletonFile = Path.Combine(fPath, $"{Skeleton}_HRC.w3x");
                                            SkeletonLowLOD = false;
                                        }
                                        XmlTextReader reader = new XmlTextReader(SkeletonFile);
                                        while (reader.Read())
                                        {
                                            reader.ReadToFollowing("W3DHierarchy");
                                            string strInner = reader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                xmlReader.WhitespaceHandling = WhitespaceHandling.None;
                                                XmlNode SkeletonNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(SkeletonNode, W3DContainer);
                                                xDoc.Save(nXML);
                                                Console.Write($"\n   Adding W3X Hierarchy: {Skeleton}");
                                            }
                                        }
                                        // Delete Skeleton File
                                        reader.Close();
                                        if (SkeletonLowLOD || !islowLOD)
                                        {
                                            File.Delete(SkeletonFile);
                                        }
                                    }
                                    // Otherwise reference them to include
                                    else
                                    {
                                        XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                                        Include.SetAttribute("type", "all");
                                        Skeleton = Skeleton.ToLower();
                                        Include.SetAttribute("source", $"ART:{Skeleton}.w3x");
                                        newIncludes.AppendChild(Include);
                                        xDoc.DocumentElement.InsertBefore(newIncludes, W3DContainer);
                                        xDoc.Save(nXML);
                                    }
                                }
                                // Search for animation files that have the same ID as the Container
                                if (File.Exists(Path.Combine(fPath, $"{Container}.w3x")))
                                {
                                    string AnimationFile;

                                    bool AnimationLowLOD;

                                    if (File.Exists(Path.Combine(fPathLowLOD, $"{Container}.w3x")) && islowLOD)
                                    {
                                        AnimationFile = Path.Combine(fPathLowLOD, $"{Container}.w3x");
                                        AnimationLowLOD = true;
                                    }
                                    else
                                    {
                                        AnimationFile = Path.Combine(fPath, $"{Container}.w3x");
                                        AnimationLowLOD = false;
                                    }
                                    XmlTextReader reader = new XmlTextReader(AnimationFile);
                                    // Check that it is an Animation File
                                    if (reader.ReadToFollowing("W3DAnimation"))
                                    {
                                        reader.Close();
                                        reader = new XmlTextReader(AnimationFile);
                                        while (reader.Read())
                                        {
                                            reader.ReadToFollowing("W3DAnimation");
                                            string strInner = reader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                xmlReader.WhitespaceHandling = WhitespaceHandling.None;
                                                XmlNode AnimationNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(AnimationNode, W3DContainer);
                                                xDoc.Save(nXML);
                                            }
                                        }
                                        // Delete Animation File
                                        reader.Close();
                                        if (AnimationLowLOD || !islowLOD)
                                        {
                                            File.Delete(AnimationFile);
                                        }
                                        Console.Write($"\n   Adding W3X Animation: {Container}");
                                    }
                                    else
                                    {
                                        reader.Close();
                                    }
                                }

                                XmlNodeList RenderObjects = xDoc.GetElementsByTagName("RenderObject");
                                foreach (XmlNode RenderObject in RenderObjects)
                                {
                                    string strValue = (string)RenderObject.FirstChild.InnerText;
                                    if (RenderObject.FirstChild.Name == "Mesh")
                                    {
                                        SubObjectTypes.Add(RenderObjectType.Mesh);
                                    }
                                    else if (RenderObject.FirstChild.Name == "CollisionBox")
                                    {
                                        SubObjectTypes.Add(RenderObjectType.CollisionBox);
                                    }
                                    SubObjects.Add(strValue);
                                }

                                if (SubObjects.Count > 0)
                                {
                                    for (int i = 0; i < SubObjects.Count; i++)
                                    {
                                        string SubObjectFile;

                                        bool RenderLowLOD;

                                        if (File.Exists(Path.Combine(fPathLowLOD, $"{SubObjects[i]}.w3x")) && islowLOD)
                                        {
                                            SubObjectFile = Path.Combine(fPathLowLOD, $"{SubObjects[i]}.w3x");
                                            RenderLowLOD = true;
                                        }
                                        else
                                        {
                                            SubObjectFile = Path.Combine(fPath, $"{SubObjects[i]}.w3x");
                                            RenderLowLOD = false;
                                        }
                                        XmlTextReader reader = new XmlTextReader(SubObjectFile);
                                        while (reader.Read())
                                        {
                                            if ((RenderObjectType)SubObjectTypes[i] == RenderObjectType.Mesh)
                                            {
                                                reader.ReadToFollowing("W3DMesh");
                                            }
                                            else if ((RenderObjectType)SubObjectTypes[i] == RenderObjectType.CollisionBox)
                                            {
                                                reader.ReadToFollowing("W3DCollisionBox");
                                            }
                                            string strInner = reader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                xmlReader.WhitespaceHandling = WhitespaceHandling.None;
                                                XmlNode SubObjectNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(SubObjectNode, W3DContainer);
                                                xDoc.Save(nXML);
                                            }
                                        }
                                        reader.Close();
                                        if (RenderLowLOD || !islowLOD)
                                        {
                                            File.Delete(SubObjectFile);
                                        }
                                        Console.Write($"\n   Adding W3X Render Object: {SubObjects[i]}");
                                    }
                                }
                            }
                        }
                        XmlNodeList Textures = xDoc.GetElementsByTagName("Texture");
                        foreach (XmlNode Texture in Textures)
                        {
                            string strTexture = (string)Texture.InnerText;
                            strTexture = strTexture.Trim();
                            if (!TexturesList.Contains(strTexture))
                            {
                                TexturesList.Add(strTexture);
                            }
                        }
                        // Insert Includes if Skeleton is in file
                        if (Container.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                        {
                            foreach (XmlNode W3DHierarchy in W3DSkeletons)
                            {
                                xDoc.DocumentElement.InsertBefore(newIncludes, W3DHierarchy);

                                // Remove redundant namespace
                                XmlAttributeCollection mapAttributes = W3DHierarchy.Attributes;
                                mapAttributes.Remove(mapAttributes["xmlns"]);

                                XmlNodeList FixupMatrices = xDoc.GetElementsByTagName("FixupMatrix");
                                foreach (XmlNode FixupMatrix in FixupMatrices)
                                {
                                    XmlAttributeCollection FixupMatrixAttributes = FixupMatrix.Attributes;
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M03"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M13"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M23"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M33"]);
                                }

                                XmlNodeList W3DBones = xDoc.GetElementsByTagName("Pivot");
                                foreach (XmlNode Pivot in W3DBones)
                                {
                                    XmlAttributeCollection PivotAttributes = Pivot.Attributes;
                                    foreach (XmlNode xnodAttribure in PivotAttributes)
                                    {
                                        if (xnodAttribure.Name == "Name")
                                        {
                                            xnodAttribure.Value = xnodAttribure.Value.ToUpperInvariant();
                                        }
                                    }
                                }

                                xDoc.Save(nXML);
                            }
                        }
                        foreach (string strTex in TexturesList)
                        {
                            XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                            Include.SetAttribute("type", "all");
                            string strTex2 = strTex.ToLower();
                            Include.SetAttribute("source", $"ART:{strTex2}.xml");
                            newIncludes.AppendChild(Include);
                            xDoc.Save(nXML);
                        }
                        // Remove Comments
                        XmlNodeList Comments = xDoc.SelectNodes("//comment()");
                        foreach (XmlNode Comment in Comments)
                        {
                            xDoc.PreserveWhitespace = false;
                            Comment.ParentNode.RemoveChild(Comment);
                            xDoc.Save(nXML);
                        }

                        // Remove Namespace in each W3D element
                        foreach (XmlNode W3DAnimation in W3DAnimations)
                        {
                            XmlAttributeCollection mapAttributes = W3DAnimation.Attributes;
                            mapAttributes.Remove(mapAttributes["xmlns"]);

                            foreach (XmlNode xnodAttribute in mapAttributes)
                            {
                                if (xnodAttribute.Name == "Hierarchy")
                                {
                                    xnodAttribute.Value = xnodAttribute.Value.ToUpperInvariant();
                                    Skeleton = xnodAttribute.Value;
                                }
                            }

                            xDoc.Save(nXML);
                        }
                        foreach (XmlNode W3DMesh in W3DMeshes)
                        {
                            XmlAttributeCollection mapAttributes = W3DMesh.Attributes;
                            mapAttributes.Remove(mapAttributes["xmlns"]);
                            xDoc.Save(nXML);
                        }
                        foreach (XmlNode W3DCollisionBox in W3DCollisionBoxes)
                        {
                            XmlAttributeCollection mapAttributes = W3DCollisionBox.Attributes;
                            mapAttributes.Remove(mapAttributes["xmlns"]);

                            bool JoypadPickingDefault = false;
                            XmlNamedNodeMap AttributeList = W3DCollisionBox.Attributes;
                            foreach (XmlNode xnodAttribute in mapAttributes)
                            {
                                if (xnodAttribute.Name == "JoypadPickingOnly")
                                {
                                    if (xnodAttribute.Value == "false")
                                    {
                                        JoypadPickingDefault = true;
                                    }
                                }
                            }
                            if (JoypadPickingDefault)
                            {
                                mapAttributes.Remove(mapAttributes["JoypadPickingOnly"]);
                            }

                            xDoc.Save(nXML);
                        }

                        Console.Write("\n[SUCCESS]\n");

                        // Set up to sort into sub folders
                        string SubFolderName = Container.Substring(0, 2);
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory(SubFolderName);

                        if (islowLOD)
                        {
                            if (File.Exists(Path.Combine(fPathLowLOD, $"{Container}.w3x")))
                            {
                                File.Copy(Path.Combine(fPathLowLOD, $"{Container}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Container}{LODPostFix}.w3x"), true);
                                File.Delete(Path.Combine(fPathLowLOD, $"{Container}.w3x"));
                            }
                            else
                            {
                                File.Copy(Path.Combine(fPathLowLOD, $"{Container}_CTR.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Container}{LODPostFix}.w3x"), true);
                                File.Delete(Path.Combine(fPathLowLOD, $"{Container}_CTR.w3x"));
                            }
                        }
                        else
                        {
                            if (File.Exists(Path.Combine(fPath, $"{Container}.w3x")))
                            {
                                File.Copy(Path.Combine(fPath, $"{Container}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Container}.w3x"), true);
                                File.Delete(Path.Combine(fPath, $"{Container}.w3x"));
                            }
                            else
                            {
                                File.Copy(Path.Combine(fPath, $"{Container}_CTR.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Container}.w3x"), true);
                                File.Delete(Path.Combine(fPath, $"{Container}_CTR.w3x"));
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.Write("\n[ERROR]\n");
                    EmitError("Failed with exception: {0}" + Convert.ToString(e));
                }
            }
        }
        static void movefiles(string w3xfile, string fPath, LevelOfDetail LODType)
        {
            if (File.Exists(w3xfile))
            {
                try
                {
                    string Animation = "";
                    string Skeleton = "";
                    string Mesh = "";
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(w3xfile);
                    string nXML = w3xfile;
                    XmlNodeList AssetDeclarations = xDoc.GetElementsByTagName("AssetDeclaration");
                    XmlNodeList W3DAnimations = xDoc.GetElementsByTagName("W3DAnimation");
                    XmlNodeList W3DSkeletons = xDoc.GetElementsByTagName("W3DHierarchy");
                    XmlNodeList W3DMeshes = xDoc.GetElementsByTagName("W3DMesh");
                    XmlNodeList Includes = xDoc.GetElementsByTagName("Includes");
                    XmlElement newIncludes = xDoc.CreateElement("Includes", "uri:ea.com:eala:asset");

                    string fPathLowLOD = "";
                    string LODPostFix = "";
                    switch (LODType)
                    {
                        case LevelOfDetail.Medium:
                            fPathLowLOD = Path.Combine(fPath, "MediumLOD");
                            LODPostFix = "_M";
                            break;
                        case LevelOfDetail.Low:
                            fPathLowLOD = Path.Combine(fPath, "LowLOD");
                            LODPostFix = "_L";
                            break;
                    }

                    bool islowLOD = false;

                    if (LODType == LevelOfDetail.Medium || LODType == LevelOfDetail.Low)
                    {
                        islowLOD = true;
                    }


                    if (W3DAnimations.Count != 0)
                    {
                        Console.Write($"\nProcessing W3X Animation: {w3xfile}");
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            XmlNamedNodeMap DeclarationAttributes = AssetDeclaration.Attributes;
                            bool FullDeclaration = false;
                            foreach (XmlNode ADAttributes in DeclarationAttributes)
                            {
                                if (ADAttributes.Name == "xmlns:xsi")
                                {
                                    FullDeclaration = true;
                                }
                            }
                            if (!FullDeclaration)
                            {
                                XmlAttribute XSIDeclaration = xDoc.CreateAttribute("xmlns:xsi");
                                XSIDeclaration.Value = "http://www.w3.org/2001/XMLSchema-instance";
                                AssetDeclaration.Attributes.Append(XSIDeclaration);
                                xDoc.Save(nXML);
                            }
                            foreach (XmlNode W3DAnimation in W3DAnimations)
                            {
                                XmlNamedNodeMap mapAttributes = W3DAnimation.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Animation = xnodAttribute.Value;
                                    }
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        xnodAttribute.Value = xnodAttribute.Value.ToUpperInvariant();
                                        Skeleton = xnodAttribute.Value;
                                    }
                                }

                                if (Includes.Count == 0)
                                {
                                    XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                                    Include.SetAttribute("type", "all");
                                    Skeleton = Skeleton.ToLower();
                                    Include.SetAttribute("source", $"ART:{Skeleton}.w3x");
                                    newIncludes.AppendChild(Include);
                                    xDoc.DocumentElement.InsertBefore(newIncludes, W3DAnimation);
                                    xDoc.Save(nXML);
                                }

                            }
                            // Remove Comments
                            XmlNodeList Comments = xDoc.SelectNodes("//comment()");
                            foreach (XmlNode Comment in Comments)
                            {
                                xDoc.PreserveWhitespace = false;
                                Comment.ParentNode.RemoveChild(Comment);
                                xDoc.Save(nXML);
                            }

                            Console.Write(" [SUCCESS]");

                            // Copy into compiled folder to avoid second pass
                            string SubFolderName = Animation.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            if (islowLOD)
                            {
                                File.Copy(Path.Combine(fPathLowLOD, $"{Animation}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Animation}{LODPostFix}.w3x"), true);
                                File.Delete(Path.Combine(fPathLowLOD, $"{Animation}.w3x"));
                            }
                            else
                            {
                                File.Copy(Path.Combine(fPath, $"{Animation}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Animation}.w3x"), true);
                                File.Delete(Path.Combine(fPath, $"{Animation}.w3x"));
                            }

                        }
                    }
                    if (W3DSkeletons.Count != 0)
                    {
                        Console.Write($"\nProcessing W3X Skeleton: {w3xfile}");
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            foreach (XmlNode W3DHierarchy in W3DSkeletons)
                            {
                                XmlNamedNodeMap mapAttributes = W3DHierarchy.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Skeleton = xnodAttribute.Value;
                                    }
                                }

                                XmlNodeList FixupMatrices = xDoc.GetElementsByTagName("FixupMatrix");
                                foreach (XmlNode FixupMatrix in FixupMatrices)
                                {
                                    XmlAttributeCollection FixupMatrixAttributes = FixupMatrix.Attributes;
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M03"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M13"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M23"]);
                                    FixupMatrixAttributes.Remove(FixupMatrixAttributes["M33"]);
                                }

                                XmlNodeList W3DBones = xDoc.GetElementsByTagName("Pivot");
                                foreach (XmlNode Pivot in W3DBones)
                                {
                                    XmlAttributeCollection PivotAttributes = Pivot.Attributes;
                                    foreach (XmlNode xnodAttribure in PivotAttributes)
                                    {
                                        if (xnodAttribure.Name == "Name")
                                        {
                                            xnodAttribure.Value = xnodAttribure.Value.ToUpperInvariant();
                                        }
                                    }
                                }

                                xDoc.DocumentElement.InsertBefore(newIncludes, W3DHierarchy);
                                xDoc.Save(nXML);
                            }
                            // Remove Comments
                            XmlNodeList Comments = xDoc.SelectNodes("//comment()");
                            foreach (XmlNode Comment in Comments)
                            {
                                xDoc.PreserveWhitespace = false;
                                Comment.ParentNode.RemoveChild(Comment);
                                xDoc.Save(nXML);
                            }

                            Console.Write(" [SUCCESS]");

                            string SubFolderName = Skeleton.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            if (islowLOD)
                            {
                                if (File.Exists(Path.Combine(fPathLowLOD, $"{Skeleton}.w3x")))
                                {
                                    File.Copy(Path.Combine(fPathLowLOD, $"{Skeleton}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Skeleton}{LODPostFix}.w3x"), true);
                                    File.Delete(Path.Combine(fPathLowLOD, $"{Skeleton}.w3x"));
                                }
                                else
                                {
                                    File.Copy(Path.Combine(fPathLowLOD, $"{Skeleton}_HRC.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Skeleton}{LODPostFix}.w3x"), true);
                                    File.Delete(Path.Combine(fPathLowLOD, $"{Skeleton}_HRC.w3x"));
                                }
                            }
                            else
                            {
                                if (File.Exists(Path.Combine(fPath, $"{Skeleton}.w3x")))
                                {
                                    File.Copy(Path.Combine(fPath, $"{Skeleton}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Skeleton}.w3x"), true);
                                    File.Delete(Path.Combine(fPath, $"{Skeleton}.w3x"));
                                }
                                else
                                {
                                    File.Copy(Path.Combine(fPath, $"{Skeleton}_HRC.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Skeleton}.w3x"), true);
                                    File.Delete(Path.Combine(fPath, $"{Skeleton}_HRC.w3x"));
                                }
                            }
                        }
                    }
                    if (W3DMeshes.Count != 0)
                    {
                        Console.Write($"\nProcessing W3X Mesh: {w3xfile}");
                        ArrayList TexturesList = new ArrayList();
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            foreach (XmlNode W3DMesh in W3DMeshes)
                            {
                                XmlNamedNodeMap mapAttributes = W3DMesh.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Mesh = xnodAttribute.Value;
                                    }
                                }
                            }
                            // Remove Comments
                            XmlNodeList Comments = xDoc.SelectNodes("//comment()");
                            foreach (XmlNode Comment in Comments)
                            {
                                xDoc.PreserveWhitespace = false;
                                Comment.ParentNode.RemoveChild(Comment);
                                xDoc.Save(nXML);
                            }

                            // Include Textures
                            XmlNodeList Textures = xDoc.GetElementsByTagName("Texture");
                            foreach (XmlNode Texture in Textures)
                            {
                                string strTexture = (string)Texture.InnerText;
                                strTexture = strTexture.Trim();
                                if (!TexturesList.Contains(strTexture))
                                {
                                    TexturesList.Add(strTexture);
                                }
                            }
                            foreach (XmlNode W3DMesh in W3DMeshes)
                            {
                                xDoc.DocumentElement.InsertBefore(newIncludes, W3DMesh);
                            }
                            foreach (string strTex in TexturesList)
                            {
                                XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                                Include.SetAttribute("type", "all");
                                string strTex2 = strTex.ToLower();
                                Include.SetAttribute("source", $"ART:{strTex2}.xml");
                                newIncludes.AppendChild(Include);
                                xDoc.Save(nXML);
                            }

                            Console.Write(" [SUCCESS]");

                            string SubFolderName = Mesh.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            if (islowLOD)
                            {
                                File.Copy(Path.Combine(fPathLowLOD, $"{Mesh}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Mesh}{LODPostFix}.w3x"), true);
                                File.Delete(Path.Combine(fPathLowLOD, $"{Mesh}.w3x"));
                            }
                            else
                            {
                                File.Copy(Path.Combine(fPath, $"{Mesh}.w3x"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{Mesh}.w3x"), true);
                                File.Delete(Path.Combine(fPath, $"{Mesh}.w3x"));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write(" [ERROR]");
                    EmitError("Failed with exception: {0}" + Convert.ToString(e));
                }
            }
        }

        static void movetexturefiles(string texturefile, string fPath)
        {
            if (File.Exists(texturefile))
            {
                try
                {
                    Console.Write($"\nMoving Texture File: {texturefile}");
                    string texture = Path.GetFileNameWithoutExtension(texturefile);
                    // Packed and Individual Images are stored in the Image folder instead of SubFolder
                    if (!texture.StartsWith("Packed") && !texture.StartsWith("Individual"))
                    {
                        string SubFolderName = texture.Substring(0, 2);
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory(SubFolderName.ToUpperInvariant());

                        // Move Texture file
                        File.Copy(Path.Combine(fPath, $"{texture}.dds"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{texture}.dds"), true);
                        File.Delete(Path.Combine(fPath, $"{texture}.dds"));
                        // With Coresponding xml
                        File.Copy(Path.Combine(fPath, $"{texture}.xml"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}{SubFolderName}{Path.DirectorySeparatorChar}{texture}.xml"), true);
                        File.Delete(Path.Combine(fPath, $"{texture}.xml"));
                    }
                    else
                    {
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory("Images");
                        File.Copy(Path.Combine(fPath, $"{texture}.dds"), Path.Combine(fPath, $"Compiled{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}{texture}.dds"), true);
                        File.Delete(Path.Combine(fPath, $"{texture}.dds"));
                        // Packed and Individual Images are included in a single xml file each
                        File.Delete(Path.Combine(fPath, $"{texture}.xml"));
                    }
                }
                catch (Exception e)
                {
                    Console.Write(" [ERROR]\n");
                    EmitError("Failed with exception: {0}" + Convert.ToString(e));
                }
            }
        }
        static void buildskn(string fPath)
        {
            DirectoryInfo directory = new DirectoryInfo(fPath);
            string[] w3xfiles = Directory.GetFiles(fPath, "*.w3x");
            foreach (string w3xfile in w3xfiles)
            {
                // Medium LOD
                if (Directory.Exists(Path.Combine(fPath, "MediumLOD")))
                {
                    if (checkLOD(w3xfile, fPath, LevelOfDetail.Medium))
                    {
                        string fPathLowLOD = Path.Combine(fPath, "MediumLOD");
                        string fileName = Path.GetFileName(w3xfile);

                        buildfile(Path.Combine(fPathLowLOD, $"{fileName}"), fPath, LevelOfDetail.Medium);
                    }
                }
                // Low LOD
                if (Directory.Exists(Path.Combine(fPath, "LowLOD")))
                {
                    if (checkLOD(w3xfile, fPath, LevelOfDetail.Low))
                    {
                        string fPathLowLOD = Path.Combine(fPath, "LowLOD");
                        string fileName = Path.GetFileName(w3xfile);

                        buildfile(Path.Combine(fPathLowLOD, $"{fileName}"), fPath, LevelOfDetail.Low);
                    }
                }
                // if (!File.Exists(w3xfile))
                // {
                //     Console.Write($"\nMissing {w3xfile}");
                // }
                buildfile(w3xfile, fPath, LevelOfDetail.High);
            }
            // Move rest into compiled folder
            // Want to remove Bibbers skeleton extensions "HRC"
            Console.Write("\nMove Files\n");
            foreach (string w3xfile in w3xfiles)
            {
                movefiles(w3xfile, fPath, LevelOfDetail.High);
            }
            string[] texturefiles = Directory.GetFiles(fPath, "*.dds");
            foreach (string texturefile in texturefiles)
            {
                movetexturefiles(texturefile, fPath);
            }
            if (Directory.Exists(Path.Combine(fPath, "MediumLOD")))
            {
                Console.Write("\nMove Medium LOD Files\n");
                string[] w3xfiles_M = Directory.GetFiles(Path.Combine(fPath, "MediumLOD"), "*.w3x");
                foreach (string w3xfile in w3xfiles_M)
                {
                    movefiles(w3xfile, fPath, LevelOfDetail.Medium);
                }
            }
            if (Directory.Exists(Path.Combine(fPath, "LowLOD")))
            {
                Console.Write("\nMove Low LOD Files\n");
                string[] w3xfiles_L = Directory.GetFiles(Path.Combine(fPath, "LowLOD"), "*.w3x");
                foreach (string w3xfile in w3xfiles_L)
                {
                    movefiles(w3xfile, fPath, LevelOfDetail.Low);
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("MakeSKN will scan provided w3x containers and try to restore them.\nUsage: MakeSKN [Path]");
                }
                else
                {
                    if (args.Length != 1)
                    {
                        args = new string[1];
                        args[0] = "";
                    }
                    if (args[0].Contains(".w3x"))
                    {
                        Console.WriteLine("MakeSKN will process single file");
                        var directoryFullPath = Path.GetDirectoryName(args[0]);
                        buildfile(args[0], directoryFullPath, LevelOfDetail.High);
                    }
                    else
                    {
                        Console.WriteLine("MakeSKN will process directory"); 
                        buildskn(args[0]);
                    }
                }

                if (errors > 0)
                {
                    Console.WriteLine("Errors: {0}", errors);
                }

            }
            catch (Exception e)
            {
                EmitError("Failed with exception: {0}"+Convert.ToString(e));
            }
        }
    }
}
