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

        public Mod ( string path )
        {
            if ( !Directory.Exists( path ) )
            {
                throw new Exception( $"path '{path}' not found." );
            }

            var about = PathCombine( path, "About", "About.xml" );
            if ( !File.Exists( about ) )
            {
                throw new Exception( $"About.xml not found at ({about})");
            }

            ContentFolder = path;
            Tags = new List<string>();
            Tags.Add( "Mod" );

            // open About.xml
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
                        if ( meta.Name.ToLower() == "targetversion" )
                        {
                            var version = VersionFromString( meta.InnerText );
                            Tags.Add( version.Major + "." + version.Minor );
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
                throw new ArgumentException("str");
            }
            string[] array = str.Split( '.' );
            if (array.Length > 3)
            {
                throw new ArgumentException("str");
            }
            int major = 0;
            int minor = 0;
            int build = 0;
            for (int i = 0; i < 3; i++)
            {
                int num;
                if (!int.TryParse(array[i], out num))
                {
                    throw new ArgumentException("str");
                }
                if (num < 0)
                {
                    throw new ArgumentException("str");
                }
                if (i != 0)
                {
                    if (i != 1)
                    {
                        if (i == 2)
                        {
                            build = num;
                        }
                    }
                    else
                    {
                        minor = num;
                    }
                }
                else
                {
                    major = num;
                }
            }
            return new Version(major, minor, build);
        }

        public void TimeStamp()
        {
            File.WriteAllText( PathCombine( ContentFolder, "About", "timestamp.txt" ), DateTime.Now.ToString() );
        }
    }
}