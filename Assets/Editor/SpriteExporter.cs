using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class SpriteExporter : EditorWindow
{
    private Sprite[] _spriteSheets;
    
    private const string defaultOutputFolder = "Assets/ExportedSprites"; // Default folder path
    private const string keyOutputFolder = "sprites_output_folder";

    [MenuItem("Tools/Export Sprite Sheets")]
    static void Init()
    {
        SpriteExporter window = GetWindow<SpriteExporter>("Export Sprite Sheets");
        window.Show();
    }

    [MenuItem("Assets/Export Selected Sprite Sheet")]
    static void ExportSelectedSpriteSheet()
    {
        Object[] selectedObjects = Selection.objects;
        Sprite[] selectedSprites = selectedObjects.OfType<Sprite>().ToArray();

        if (selectedSprites.Length > 0)
        {
            string newOutputFolder = EditorUtility.OpenFolderPanel("Select Output Folder", EditorPrefs.GetString(keyOutputFolder, defaultOutputFolder), "");
            if (!string.IsNullOrEmpty(newOutputFolder))
            {
                EditorPrefs.SetString(keyOutputFolder, newOutputFolder);
                ExportSpriteSheets(selectedSprites, newOutputFolder);
            }
           
            return;
        }

        Tile[] selectedTiles = selectedObjects.OfType<Tile>().ToArray();
        if (selectedTiles.Length > 0)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (var tile in selectedTiles)
            {
                if (tile.sprite != null)
                {
                    sprites.Add(tile.sprite);
                }
            }

            if (sprites.Count > 0)
            {
                string newOutputFolder = EditorUtility.OpenFolderPanel("Select Output Folder", EditorPrefs.GetString(keyOutputFolder, defaultOutputFolder), "");
                if (!string.IsNullOrEmpty(newOutputFolder))
                {
                    EditorPrefs.SetString(keyOutputFolder, newOutputFolder);
                    ExportSpriteSheets(sprites.ToArray(), newOutputFolder);
                }
            }
        }
        else
        {
            Debug.LogError("No valid sprites selected for export.");
        }
    }

    private void OnGUI()
    {
        Event evt = Event.current;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    _spriteSheets = new Sprite[DragAndDrop.objectReferences.Length];
                    for (int i = 0; i < _spriteSheets.Length; i++)
                    {
                        _spriteSheets[i] = DragAndDrop.objectReferences[i] as Sprite;
                    }
                }

                DragAndDrop.AcceptDrag();
            }
        }

        EditorGUILayout.LabelField("Sprite Sheets");

        EditorGUI.indentLevel++;

        int newSize = EditorGUILayout.IntField("Size", _spriteSheets != null ? _spriteSheets.Length : 0);
        if (newSize != (_spriteSheets != null ? _spriteSheets.Length : 0))
        {
            System.Array.Resize(ref _spriteSheets, newSize);
        }

        for (int i = 0; i < newSize; i++)
        {
            _spriteSheets[i] = EditorGUILayout.ObjectField("Element " + i, _spriteSheets[i], typeof(Sprite), true) as Sprite;
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.BeginHorizontal();
        string outputFolder = EditorGUILayout.TextField("Output Folder", EditorPrefs.GetString(keyOutputFolder, defaultOutputFolder));
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string newOutputFolder = EditorUtility.OpenFolderPanel("Select Output Folder", EditorPrefs.GetString(keyOutputFolder, defaultOutputFolder), "");
            if (!string.IsNullOrEmpty(newOutputFolder))
            {
                outputFolder = newOutputFolder;
                EditorPrefs.SetString(keyOutputFolder, outputFolder);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Export Sprite Sheets"))
        {
            ExportSpriteSheets(_spriteSheets, outputFolder);
        }
    }

    static void ExportSpriteSheets(Sprite[] sprites, string outputFolder)
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("Sprite Sheets must be selected.");
            return;
        }

        TextureImporter textureImporter = null;
        string spriteSheetPath = null;
        string lastSpritePath = "";
        bool nowModifTextureReadable = false;

        if (sprites.Length > 0)
        {
            spriteSheetPath = AssetDatabase.GetAssetPath(sprites[0]);
            textureImporter = AssetImporter.GetAtPath(spriteSheetPath) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(spriteSheetPath);
            AssetDatabase.Refresh();
            
            nowModifTextureReadable = true;
        }
        
        foreach (var spriteSheet in sprites)
        {
            if (spriteSheet == null)
            {
                Debug.LogError("One or more selected sprite sheets are null.");
                continue;
            }

            Rect rect = spriteSheet.rect;
            Texture2D spriteTexture = new Texture2D((int)rect.width, (int)rect.height);
            spriteTexture.SetPixels(spriteSheet.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            spriteTexture.Apply();

            byte[] bytes = spriteTexture.EncodeToPNG();
            string spriteName = $"{spriteSheet.name}.png";
            string spritePath = Path.Combine(outputFolder, spriteName);
            File.WriteAllBytes(spritePath, bytes);

            Debug.Log($"Sprite exported: {spritePath}");
        }
        
        if (nowModifTextureReadable)
        {
            textureImporter.isReadable = false;
            AssetDatabase.ImportAsset(spriteSheetPath);
            AssetDatabase.Refresh();
        }
    }
}
