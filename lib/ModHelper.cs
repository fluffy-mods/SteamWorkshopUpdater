// ModHelper.cs
// Copyright Karel Kroeze, 2018-2018

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Steamworks;
using Version = System.Version;

namespace SteamWorkshopUploader
{
    public class Mod
    {
        public string Name { get; }
        public string Preview { get; }
        public string Description { get; set; }
        public bool OriginalUploader { get; set; }
        public List<string> Tags;

        private PublishedFileId_t _publishedFileId = PublishedFileId_t.Invalid;
        public PublishedFileId_t PublishedFileId
        {
            get => _publishedFileId;
            set
            {
                if ( _publishedFileId != value && value != PublishedFileId_t.Invalid )
                    File.WriteAllText( PathCombine( ContentFolder, "About", "PublishedFileId.txt" ), value.ToString() );
                _publishedFileId = value;
            }
        }

        public string ContentFolder { get; }

        public Mod ( string path, bool originalUploader = true )
        {
            if ( !Directory.Exists( path ) )
            {
                throw new Exception( $"path '{path}' not found." );
            }
            OriginalUploader = originalUploader;
            ContentFolder = path;
            Tags = new List<string>();
            Tags.Add("Mod");


            // open Manifest.xml to look for any extra targetVersions.
            var manifest = PathCombine(path, "About", "Manifest.xml");
            if (File.Exists(manifest))
            {
                var manifestXml = new XmlDocument();
                manifestXml.Load(manifest);
                for ( int i = 0; i < manifestXml.ChildNodes.Count; i++ )
                {
                    var node = manifestXml.ChildNodes[i];
                    if ( node.Name == "Manifest" )
                    {
                        for ( int j = 0; j < node.ChildNodes.Count; j++ )
                        {
                            var subnode = node.ChildNodes[j];
                            if ( subnode.Name.ToLower() == "targetversions" )
                            {
                                for ( int k = 0; k < subnode.ChildNodes.Count; k++ )
                                {
                                    var li = subnode.ChildNodes[k];
                                    try
                                    {
                                        var version = VersionFromString( li.InnerText );
                                        Tags.Add( version.Major + "." + version.Minor );
                                    }
                                    catch ( Exception e )
                                    {
                                        Console.WriteLine(
                                            $"Error reading targetVersions from Manifest.xml ({li.InnerText}): {e.Message}" );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // open About.xml
            var about = PathCombine( path, "About", "About.xml" );
            if ( !File.Exists( about ) )
            {
                throw new Exception( $"About.xml not found at ({about})");
            }
            var aboutXml = new XmlDocument();
            aboutXml.Load( about );
            for ( int i = 0; i < aboutXml.ChildNodes.Count; i++ )
            {
                var node = aboutXml.ChildNodes[i];
                if ( node.Name == "ModMetaData" )
                {
                    for ( int j = 0; j < node.ChildNodes.Count; j++ )
                    {
                        var meta = node.ChildNodes[j];
                        if ( meta.Name.ToLower() == "name" )
                            Name = meta.InnerText;
                        if ( meta.Name.ToLower() == "description" )
                            Description = meta.InnerText;
                        if ( meta.Name.ToLower() == "supportedversions" && Tags.Count == 1 )
                        {
                            for ( int k = 0; k < meta.ChildNodes.Count; k++ )
                            {
                                var version = VersionFromString( meta.ChildNodes[k].InnerText );
                                Tags.Add( version.Major + "." + version.Minor );
                            }
                        }
                    }
                }
            }

            // get preview image
            var preview = PathCombine( path, "About", "Preview.png" );
            if ( File.Exists( preview ) )
                Preview = preview;

            // get publishedFileId
            var pubfileIdPath = PathCombine( path, "About", "PublishedFileId.txt" );
            uint id;
            if ( File.Exists( pubfileIdPath ) && uint.TryParse( File.ReadAllText( pubfileIdPath ), out id ) )
            {
                PublishedFileId = new PublishedFileId_t( id );
            }
            else
            {
                PublishedFileId = PublishedFileId_t.Invalid;
            }
        }

        public override string ToString()
        {
            return $"Name: {Name}\nPreview: {Preview}\nPublishedFileId: {PublishedFileId}\nDescription: {Description}";
        }

        private static string PathCombine( params string[] parts )
        {
            return string.Join( Path.DirectorySeparatorChar.ToString(), parts );
        }

        // copy-pasta from RimWorld.VersionControl
        public static Version VersionFromString(string str)
        {
            if ( string.IsNullOrEmpty( str ) )
            {
                throw new ArgumentException("version empty");
            }
            string[] array = str.Split( '.' );
            if (array.Length > 2)
            {
                throw new ArgumentException("version more than 2 elements. What is this, B19?");
            }
            int major = 0;
            int minor = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!int.TryParse(array[i], out int num))
                {
                    throw new ArgumentException($"can't parse {i} of version {str}");
                }
                if (num < 0)
                {
                    throw new ArgumentException($"part of version {str} less than zero");
                }
                if (i != 0)
                {
                    if (i == 1)
                    {
                        minor = num;
                    }
                }
                else
                {
                    major = num;
                }
            }
            return new Version(major, minor);
        }

        public void TimeStamp()
        {
            File.WriteAllText( PathCombine( ContentFolder, "About", "timestamp.txt" ), DateTime.Now.ToString() );
        }
    }
}