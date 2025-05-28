using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using AshesOfTheEarth.Patterns.Flyweight;

namespace AshesOfTheEarth.Patterns.Flyweight
{
    public class TreeFlyweightFactory
    {
        private Dictionary<string, TreeFlyweight> _flyweights = new Dictionary<string, TreeFlyweight>();
        private ContentManager _content;
        private Texture2D _treeTileset;

        public TreeFlyweightFactory(ContentManager content)
        {
            _content = content;
            try
            {
                _treeTileset = _content.Load<Texture2D>("Sprites/Environment/Trees");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tree tileset: {ex.Message}");
                _treeTileset = null;
            }

            if (_treeTileset != null)
            {
                LoadTreeType("Oak", new Rectangle(0, 0, 64, 96), 100f);
                LoadTreeType("Pine", new Rectangle(64, 0, 48, 112), 80f);
            }
        }

        private void LoadTreeType(string name, Rectangle sourceRect, float maxHealth)
        {
            if (_treeTileset == null) return;

            if (!_flyweights.ContainsKey(name))
            {
                var treeData = new TreeTypeData(name, _treeTileset, sourceRect, maxHealth);
                _flyweights.Add(name, new TreeFlyweight(treeData));
                Console.WriteLine($"FlyweightFactory: Loaded tree type '{name}'.");
            }
        }

        public TreeFlyweight GetFlyweight(string treeTypeName)
        {
            if (_flyweights.TryGetValue(treeTypeName, out TreeFlyweight flyweight))
            {
                Console.WriteLine($"FlyweightFactory: Reusing existing flyweight for '{treeTypeName}'.");
                return flyweight;
            }
            else
            {
                Console.WriteLine($"FlyweightFactory: Flyweight for '{treeTypeName}' not found. Returning null or default.");
                return null;
            }
        }

        public void ListFlyweights()
        {
            Console.WriteLine($"\nFlyweightFactory: I have {_flyweights.Count} tree flyweights:");
            foreach (var key in _flyweights.Keys)
            {
                Console.WriteLine(key);
            }
        }
    }
}