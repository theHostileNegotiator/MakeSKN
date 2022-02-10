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

        static void buildfile(string w3xfile, string fPath)
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
                    if (W3DContainers.Count != 0)
                    {
                        Console.Write("\nProcessing W3X Container: " + w3xfile);
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            foreach (XmlNode W3DContainer in W3DContainers)
                            {
                                XmlNamedNodeMap mapAttributes = W3DContainer.Attributes;
                                foreach (XmlNode xnodAttribute in mapAttributes)
                                {
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        Skeleton = xnodAttribute.Value;
                                        Console.Write(".");
                                    }
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Container = xnodAttribute.Value;
                                        Console.Write(".");
                                    }
                                }

                                if (!String.IsNullOrEmpty(Skeleton))
                                {
                                    // If Container ID and Skeleton ID is the same, should be in same file
                                    if (Container.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                                    {
                                        // Bibber's skeleton extension "_HRC". Not applied to SKL but normally the containers won't match the name in this case
                                        string SkeletonFile = Path.Combine(fPath, (Skeleton + "_HRC.w3x"));
                                        Console.Write(".");
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
                                                Console.Write(".");
                                            }
                                        }
                                        // Delete Skeleton File
                                        reader.Close();
                                        File.Delete(SkeletonFile);
                                    }
                                    // Otherwise reference them to include
                                    else
                                    {
                                        XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                                        Include.SetAttribute("type", "all");
                                        Skeleton = Skeleton.ToLower();
                                        Include.SetAttribute("source", "ART:" + Skeleton + ".w3x");
                                        newIncludes.AppendChild(Include);
                                        xDoc.DocumentElement.InsertBefore(newIncludes, W3DContainer);
                                        xDoc.Save(nXML);
                                        Console.Write(".");
                                    }
                                }
                                // Search for animation files that have the same ID as the Container
                                if (File.Exists(Path.Combine(fPath, Container + ".w3x")))
                                {
                                    string AnimationFile = Path.Combine(fPath, (Container + ".w3x"));
                                    Console.Write(".");
                                    XmlTextReader reader = new XmlTextReader(AnimationFile);
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
                                            Console.Write(".");
                                        }
                                    }
                                    // Delete Animation File
                                    reader.Close();
                                    File.Delete(AnimationFile);
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
                                    Console.Write(".");
                                }

                                if (SubObjects.Count > 0)
                                {
                                    for (int i = 0; i < SubObjects.Count; i++)
                                    {
                                        string SubObjectFile = Path.Combine(fPath, (SubObjects[i] + ".w3x"));
                                        Console.Write(".");
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
                                                Console.Write(".");
                                            }
                                        }
                                        reader.Close();
                                        File.Delete(SubObjectFile);
                                        Console.Write(".");
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
                                Console.Write(".");
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
                                xDoc.Save(nXML);
                            }
                        }
                        foreach (string strTex in TexturesList)
                        {
                            XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                            Include.SetAttribute("type", "all");
                            string strTex2 = strTex.ToLower();
                            Include.SetAttribute("source", "ART:" + strTex2 + ".xml");
                            newIncludes.AppendChild(Include);
                            xDoc.Save(nXML);
                            Console.Write(".");
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
                            xDoc.Save(nXML);
                        }

                        Console.Write("[SUCCESS]\n");

                        // Set up to sort into sub folders
                        string SubFolderName = Container.Substring(0, 2);
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory(SubFolderName);

                        if (File.Exists(Path.Combine(fPath, (Container + ".w3x"))))
                        {
                            System.IO.File.Copy(Path.Combine(fPath, (Container + ".w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Container + ".w3x")), true);
                            File.Delete(Path.Combine(fPath, (Container + ".w3x")));
                        }
                        else
                        {
                            System.IO.File.Copy(Path.Combine(fPath, (Container + "_CTR.w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Container + ".w3x")), true);
                            File.Delete(Path.Combine(fPath, (Container + "_CTR.w3x")));
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.Write("[ERROR]\n");
                    EmitError("Failed with exception: {0}" + Convert.ToString(e));
                }
            }
        }
        static void movefiles(string w3xfile, string fPath)
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

                    if (W3DAnimations.Count != 0)
                    {
                        Console.Write("\nProcessing W3X Animation: " + w3xfile);
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
                                        Console.Write(".");
                                    }
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        Skeleton = xnodAttribute.Value;
                                        Console.Write(".");
                                    }
                                }

                                if (Includes.Count == 0)
                                {
                                    XmlElement Include = xDoc.CreateElement("Include", "uri:ea.com:eala:asset");
                                    Include.SetAttribute("type", "all");
                                    Skeleton = Skeleton.ToLower();
                                    Include.SetAttribute("source", "ART:" + Skeleton + ".w3x");
                                    newIncludes.AppendChild(Include);
                                    xDoc.DocumentElement.InsertBefore(newIncludes, W3DAnimation);
                                    xDoc.Save(nXML);
                                    Console.Write(".");
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

                            Console.Write("[SUCCESS]\n");

                            // Copy into compiled folder to avoid second pass
                            string SubFolderName = Animation.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            System.IO.File.Copy(Path.Combine(fPath, (Animation + ".w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Animation + ".w3x")), true);
                            File.Delete(Path.Combine(fPath, (Animation + ".w3x")));

                        }
                    }
                    if (W3DSkeletons.Count != 0)
                    {
                        Console.Write("\nProcessing W3X Skeleton: " + w3xfile);
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
                                        Console.Write(".");
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

                            Console.Write("[SUCCESS]\n");

                            string SubFolderName = Skeleton.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            if (File.Exists(Path.Combine(fPath, (Skeleton + ".w3x"))))
                            {
                                System.IO.File.Copy(Path.Combine(fPath, (Skeleton + ".w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Skeleton + ".w3x")), true);
                                File.Delete(Path.Combine(fPath, (Skeleton + ".w3x")));
                            }
                            else
                            {
                                System.IO.File.Copy(Path.Combine(fPath, (Skeleton + "_HRC.w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Skeleton + ".w3x")), true);
                                File.Delete(Path.Combine(fPath, (Skeleton + "_HRC.w3x")));
                            }
                        }
                    }
                    if (W3DMeshes.Count != 0)
                    {
                        Console.Write("\nProcessing W3X Mesh: " + w3xfile);
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
                                        Console.Write(".");
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
                                    Console.Write(".");
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
                                Include.SetAttribute("source", "ART:" + strTex2 + ".xml");
                                newIncludes.AppendChild(Include);
                                xDoc.Save(nXML);
                                Console.Write(".");
                            }

                            Console.Write("[SUCCESS]\n");

                            string SubFolderName = Mesh.Substring(0, 2);
                            DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                            CompiledFolder.CreateSubdirectory(SubFolderName);

                            System.IO.File.Copy(Path.Combine(fPath, (Mesh + ".w3x")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + Mesh + ".w3x")), true);
                            File.Delete(Path.Combine(fPath, (Mesh + ".w3x")));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write("[ERROR]\n");
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
                    Console.Write("\nMoving Texture File: " + texturefile);
                    string texture = Path.GetFileNameWithoutExtension(texturefile);
                    // Packed and Individual Images are stored in the Image folder instead of SubFolder
                    if (!texture.StartsWith("Packed") && !texture.StartsWith("Individual"))
                    {
                        string SubFolderName = texture.Substring(0, 2);
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory(SubFolderName.ToUpperInvariant());

                        // Move Texture file
                        System.IO.File.Copy(Path.Combine(fPath, (texture + ".dds")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + texture + ".dds")), true);
                        File.Delete(Path.Combine(fPath, (texture + ".dds")));
                        // With Coresponding xml
                        System.IO.File.Copy(Path.Combine(fPath, (texture + ".xml")), Path.Combine(fPath, ("Compiled\\" + SubFolderName + "\\" + texture + ".xml")), true);
                        File.Delete(Path.Combine(fPath, (texture + ".xml")));
                    }
                    else
                    {
                        DirectoryInfo CompiledFolder = new DirectoryInfo(Path.Combine(fPath, "Compiled"));
                        CompiledFolder.CreateSubdirectory("Images");
                        System.IO.File.Copy(Path.Combine(fPath, (texture + ".dds")), Path.Combine(fPath, ("Compiled\\Images\\" + texture + ".dds")), true);
                        File.Delete(Path.Combine(fPath, (texture + ".dds")));
                        // Packed and Individual Images are included in a single xml file each
                        File.Delete(Path.Combine(fPath, (texture + ".xml")));
                    }
                }
                catch (Exception e)
                {
                    Console.Write("[ERROR]\n");
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
                buildfile(w3xfile, fPath);
            }
            // Move rest into compiled folder
            // Want to remove Bibbers skeleton extensions "HRC"
            foreach (string w3xfile in w3xfiles)
            {
                movefiles(w3xfile, fPath);
            }
            string[] texturefiles = Directory.GetFiles(fPath, "*.dds");
            foreach (string texturefile in texturefiles)
            {
                movetexturefiles(texturefile, fPath);
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
                        buildfile(args[0], directoryFullPath);
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
