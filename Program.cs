using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;

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
                    ArrayList OBBoxes = new ArrayList();
                    ArrayList Meshes = new ArrayList();
                    ArrayList TexturesList = new ArrayList();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(w3xfile);
                    string nXML = w3xfile;
                    XmlNodeList AssetDeclarations = xDoc.GetElementsByTagName("AssetDeclaration");
                    XmlNodeList W3DContainers = xDoc.GetElementsByTagName("W3DContainer");
                    XmlNodeList W3DSkeletons = xDoc.GetElementsByTagName("W3DHierarchy");
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

                                XmlNodeList CollisionBoxs = xDoc.GetElementsByTagName("CollisionBox");
                                foreach (XmlNode CollisionBox in CollisionBoxs)
                                {
                                    string strValue = (string)CollisionBox.InnerText;
                                    OBBoxes.Add(strValue);
                                    Console.Write(".");
                                }
                                XmlNodeList Meshs = xDoc.GetElementsByTagName("Mesh");
                                foreach (XmlNode SingleMesh in Meshs)
                                {
                                    string strValue = (string)SingleMesh.InnerText;
                                    Meshes.Add(strValue);
                                    Console.Write(".");
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
                                if (OBBoxes.Count != 0)
                                {
                                    foreach (string OBBox in OBBoxes)
                                    {
                                        string OBBoxFile = Path.Combine(fPath, (OBBox + ".w3x"));
                                        Console.Write(".");
                                        XmlTextReader reader = new XmlTextReader(OBBoxFile);
                                        while (reader.Read())
                                        {
                                            reader.ReadToFollowing("W3DCollisionBox");
                                            string strInner = reader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                XmlNode OBBoxNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(OBBoxNode, W3DContainer);
                                                xDoc.Save(nXML);
                                                Console.Write(".");
                                            }
                                        }
                                        reader.Close();
                                    }
                                    foreach (string OBBox in OBBoxes)
                                    {
                                        string OBBoxFile = Path.Combine(fPath, (OBBox + ".w3x"));
                                        if (File.Exists(OBBoxFile))
                                        {
                                            File.Delete(OBBoxFile);
                                            Console.Write(".");
                                        }
                                    }
                                }

                                if (Meshes.Count != 0)
                                {
                                    foreach (string Mesh in Meshes)
                                    {
                                        string MeshFile = Path.Combine(fPath, (Mesh + ".w3x"));
                                        Console.Write(".");
                                        XmlTextReader reader = new XmlTextReader(MeshFile);
                                        while (reader.Read())
                                        {
                                            reader.ReadToFollowing("W3DMesh");
                                            string strInner = reader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                XmlNode MeshNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(MeshNode, W3DContainer);
                                                xDoc.Save(nXML);
                                                Console.Write(".");
                                            }
                                        }
                                        reader.Close();
                                    }
                                    foreach (string Mesh in Meshes)
                                    {
                                        string MeshFile = Path.Combine(fPath, (Mesh + ".w3x"));
                                        if (File.Exists(MeshFile))
                                        {
                                            File.Delete(MeshFile);
                                            Console.Write(".");
                                        }
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
                            Comment.ParentNode.RemoveChild(Comment);
                            xDoc.Save(nXML);
                        }

                        Console.Write("[SUCCESS]\n");

                        if (!new DirectoryInfo(Path.Combine(fPath, "Compiled")).Exists)
                        {
                            new DirectoryInfo(fPath).CreateSubdirectory("Compiled");
                        }


                        if (File.Exists(Path.Combine(fPath, (Container + ".w3x"))))
                        {
                            System.IO.File.Copy(Path.Combine(fPath, (Container + ".w3x")), Path.Combine(fPath, ("Compiled\\" + Container + ".w3x")), true);
                            File.Delete(Path.Combine(fPath, (Container + ".w3x")));
                        }
                        else
                        {
                            System.IO.File.Copy(Path.Combine(fPath, (Container + "_CTR.w3x")), Path.Combine(fPath, ("Compiled\\" + Container + ".w3x")), true);
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

        static void buildanimationfile(string w3xfile, string fPath)
        {
            if (File.Exists(w3xfile))
            {
                try
                {
                    string Animation = "";
                    string Skeleton = "";
                    ArrayList OBBoxes = new ArrayList();
                    ArrayList Meshes = new ArrayList();
                    ArrayList TexturesList = new ArrayList();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(w3xfile);
                    string nXML = w3xfile;
                    XmlNodeList AssetDeclarations = xDoc.GetElementsByTagName("AssetDeclaration");
                    XmlNodeList W3DContainers = xDoc.GetElementsByTagName("W3DContainer");
                    XmlNodeList W3DAnimations = xDoc.GetElementsByTagName("W3DAnimation");
                    XmlNodeList W3DSkeletons = xDoc.GetElementsByTagName("W3DHierarchy");
                    XmlNodeList AssimilatorIncludes = xDoc.GetElementsByTagName("Includes");
                    XmlElement newIncludes = xDoc.CreateElement("Includes", "uri:ea.com:eala:asset");

                    if (W3DAnimations.Count != 0)
                    {
                        foreach (XmlNode AssetDeclaration in AssetDeclarations)
                        {
                            // Assimilator missing xsi
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
                                    if (xnodAttribute.Name == "Hierarchy")
                                    {
                                        Skeleton = xnodAttribute.Value;
                                        Console.Write(".");
                                    }
                                    if (xnodAttribute.Name == "id")
                                    {
                                        Animation = xnodAttribute.Value;
                                        Console.Write(".");
                                    }
                                }
                                if (Animation.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                                {
                                    Console.Write("\nProcessing W3X Container: " + w3xfile);
                                    if (!String.IsNullOrEmpty(Skeleton))
                                    {
                                        // Bibber's skeleton extension "_HRC". Not applied to SKL but normally the animations and containers won't match the name this case
                                        string SkeletonFile = Path.Combine(fPath, (Skeleton + "_HRC.w3x"));
                                        Console.Write(".");
                                        XmlTextReader Skeletonreader = new XmlTextReader(SkeletonFile);
                                        while (Skeletonreader.Read())
                                        {
                                            Skeletonreader.ReadToFollowing("W3DHierarchy");
                                            string strInner = Skeletonreader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                XmlNode SkeletonNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertBefore(SkeletonNode, W3DAnimation);
                                                xDoc.Save(nXML);
                                                Console.Write(".");
                                            }
                                        }
                                        // Delete Skeleton File
                                        Skeletonreader.Close();
                                        File.Delete(SkeletonFile);

                                        // Look for Container, if it has a skeleton and animations then the Container must exist with the same ID
                                        // Bibber's Container extension "_CTR". Not applied to SKN but normally the animations and skeleton won't match the name in this case
                                        string ContainerFile = Path.Combine(fPath, (Animation + "_CTR.w3x"));
                                        Console.Write(".");
                                        XmlTextReader Containerreader = new XmlTextReader(ContainerFile);
                                        while (Containerreader.Read())
                                        {
                                            Containerreader.ReadToFollowing("W3DContainer");
                                            string strInner = Containerreader.ReadOuterXml();
                                            if (strInner.Length != 0)
                                            {
                                                XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                XmlNode ContainerNode = xDoc.ReadNode(xmlReader);
                                                xDoc.DocumentElement.InsertAfter(ContainerNode, W3DAnimation);
                                                xDoc.Save(nXML);
                                                Console.Write(".");
                                            }
                                        }
                                        // Delete Skeleton File
                                        Containerreader.Close();
                                        File.Delete(ContainerFile);

                                        // Read ContainerTag
                                        foreach (XmlNode W3DContainer in W3DContainers)
                                        {

                                            XmlNodeList CollisionBoxs = xDoc.GetElementsByTagName("CollisionBox");
                                            foreach (XmlNode CollisionBox in CollisionBoxs)
                                            {
                                                string strValue = (string)CollisionBox.InnerText;
                                                OBBoxes.Add(strValue);
                                                Console.Write(".");
                                            }
                                            XmlNodeList Meshs = xDoc.GetElementsByTagName("Mesh");
                                            foreach (XmlNode SingleMesh in Meshs)
                                            {
                                                string strValue = (string)SingleMesh.InnerText;
                                                Meshes.Add(strValue);
                                                Console.Write(".");
                                            }

                                            if (OBBoxes.Count != 0)
                                            {
                                                foreach (string OBBox in OBBoxes)
                                                {
                                                    string OBBoxFile = Path.Combine(fPath, (OBBox + ".w3x"));
                                                    Console.Write(".");
                                                    XmlTextReader reader = new XmlTextReader(OBBoxFile);
                                                    while (reader.Read())
                                                    {
                                                        reader.ReadToFollowing("W3DCollisionBox");
                                                        string strInner = reader.ReadOuterXml();
                                                        if (strInner.Length != 0)
                                                        {
                                                            XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                            XmlNode OBBoxNode = xDoc.ReadNode(xmlReader);
                                                            xDoc.DocumentElement.InsertBefore(OBBoxNode, W3DContainer);
                                                            xDoc.Save(nXML);
                                                            Console.Write(".");
                                                        }
                                                    }
                                                    reader.Close();
                                                }
                                                foreach (string OBBox in OBBoxes)
                                                {
                                                    string OBBoxFile = Path.Combine(fPath, (OBBox + ".w3x"));
                                                    if (File.Exists(OBBoxFile))
                                                    {
                                                        File.Delete(OBBoxFile);
                                                        Console.Write(".");
                                                    }
                                                }
                                            }

                                            if (Meshes.Count != 0)
                                            {
                                                foreach (string Mesh in Meshes)
                                                {
                                                    string MeshFile = Path.Combine(fPath, (Mesh + ".w3x"));
                                                    Console.Write(".");
                                                    XmlTextReader reader = new XmlTextReader(MeshFile);
                                                    while (reader.Read())
                                                    {
                                                        reader.ReadToFollowing("W3DMesh");
                                                        string strInner = reader.ReadOuterXml();
                                                        if (strInner.Length != 0)
                                                        {
                                                            XmlTextReader xmlReader = new XmlTextReader(new StringReader(strInner));
                                                            XmlNode MeshNode = xDoc.ReadNode(xmlReader);
                                                            xDoc.DocumentElement.InsertBefore(MeshNode, W3DContainer);
                                                            xDoc.Save(nXML);
                                                            Console.Write(".");
                                                        }
                                                    }
                                                    reader.Close();
                                                }
                                                foreach (string Mesh in Meshes)
                                                {
                                                    string MeshFile = Path.Combine(fPath, (Mesh + ".w3x"));
                                                    if (File.Exists(MeshFile))
                                                    {
                                                        File.Delete(MeshFile);
                                                        Console.Write(".");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (Animation.ToLowerInvariant() == Skeleton.ToLowerInvariant())
                            {
                                // Remove Includes from Assimilator Animation which should be the first child
                                // This doesn't work for some dumb reason
                                // foreach (XmlNode Include in AssimilatorIncludes)
                                // {
                                //     Include.ParentNode.RemoveChild(Include);
                                //     xDoc.Save(nXML);
                                // }
                                if (AssimilatorIncludes.Count > 0)
                                {
                                    AssetDeclaration.RemoveChild(AssetDeclaration.FirstChild);
                                    xDoc.Save(nXML);
                                }
                                // Generate Texture list
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
                                // Insert Includes
                                foreach (XmlNode W3DHierarchy in W3DSkeletons)
                                {
                                    xDoc.DocumentElement.InsertBefore(newIncludes, W3DHierarchy);
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
                                foreach(XmlNode Comment in Comments)
                                {
                                    xDoc.PreserveWhitespace = false;
                                    Comment.ParentNode.RemoveChild(Comment);
                                    xDoc.Save(nXML);
                                }

                                Console.Write("[SUCCESS]\n");

                                // Copy into compiled folder to avoid second pass
                                if (!new DirectoryInfo(Path.Combine(fPath, "Compiled")).Exists)
                                {
                                    new DirectoryInfo(fPath).CreateSubdirectory("Compiled");
                                }

                                System.IO.File.Copy(Path.Combine(fPath, (Animation + ".w3x")), Path.Combine(fPath, ("Compiled\\" + Animation + ".w3x")), true);

                                File.Delete(Path.Combine(fPath, (Animation + ".w3x")));
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

                            if (!new DirectoryInfo(Path.Combine(fPath, "Compiled")).Exists)
                            {
                                new DirectoryInfo(fPath).CreateSubdirectory("Compiled");
                            }

                            System.IO.File.Copy(Path.Combine(fPath, (Animation + ".w3x")), Path.Combine(fPath, ("Compiled\\" + Animation + ".w3x")), true);

                            File.Delete(Path.Combine(fPath, (Animation + ".w3x")));
                            
                        }
                    }
                    if (W3DSkeletons.Count != 0)
                    {
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

                            if (!new DirectoryInfo(Path.Combine(fPath, "Compiled")).Exists)
                            {
                                new DirectoryInfo(fPath).CreateSubdirectory("Compiled");
                            }

                            if (File.Exists(Path.Combine(fPath, (Skeleton + ".w3x"))))
                            {
                                System.IO.File.Copy(Path.Combine(fPath, (Skeleton + ".w3x")), Path.Combine(fPath, ("Compiled\\" + Skeleton + ".w3x")), true);
                                File.Delete(Path.Combine(fPath, (Skeleton + ".w3x")));
                            }
                            else
                            {
                                System.IO.File.Copy(Path.Combine(fPath, (Skeleton + "_HRC.w3x")), Path.Combine(fPath, ("Compiled\\" + Skeleton + ".w3x")), true);
                                File.Delete(Path.Combine(fPath, (Skeleton + "_HRC.w3x")));
                            }
                        }
                    }
                    if (W3DMeshes.Count != 0)
                    {
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

                            Console.Write("[SUCCESS]\n");

                            if (!new DirectoryInfo(Path.Combine(fPath, "Compiled")).Exists)
                            {
                                new DirectoryInfo(fPath).CreateSubdirectory("Compiled");
                            }

                            System.IO.File.Copy(Path.Combine(fPath, (Mesh + ".w3x")), Path.Combine(fPath, ("Compiled\\" + Mesh + ".w3x")), true);

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
        static void buildskn(string fPath)
        {
            DirectoryInfo directory = new DirectoryInfo(fPath);
            string[] w3xfiles = Directory.GetFiles(fPath, "*.w3x");
            // Animation Pass
            foreach (string w3xfile in w3xfiles)
            {
                buildanimationfile(w3xfile, fPath);
            }
            // Container Pass
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
