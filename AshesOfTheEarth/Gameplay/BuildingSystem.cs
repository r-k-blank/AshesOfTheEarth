using Microsoft.Xna.Framework;
using System.Collections.Generic; // Pentru blueprint

namespace AshesOfTheEarth.Gameplay
{



    public class BuildingSystem
    {
        private List<StructureBlueprint> _availableBlueprints;
        private StructureBlueprint _selectedBlueprint = null; // Planul selectat pentru plasare
        private bool _isPlacementMode = false;

        public BuildingSystem()
        {
            _availableBlueprints = LoadBlueprints();
        }

        public void Update(GameTime gameTime)
        {
            // TODO: Implementează logica de construire
            // - Verifică inputul pentru a intra/ieși din modul de construire
            // - Selectează blueprint-uri
            // - Afișează preview-ul de plasare (valid/invalid)
            // - Plasează structura (verifică resurse, coliziuni, creează entitatea structurii)
            // - Anulează plasarea

            // Exemplu simplu: Intră/Iese din mod construcție cu B
            var input = Core.Services.ServiceLocator.Get<Core.Input.InputManager>();
            if (input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                _isPlacementMode = !_isPlacementMode;
                if (_isPlacementMode)
                {
                    // Selectează un blueprint default la intrare (ex: primul)
                    _selectedBlueprint = _availableBlueprints.Count > 0 ? _availableBlueprints[0] : null;
                    //System.Diagnostics.Debug.WriteLine($"Entered Building Mode. Selected: {_selectedBlueprint?.DisplayName ?? "None"}");
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Exited Building Mode.");
                    _selectedBlueprint = null;
                }
            }

            if (_isPlacementMode && _selectedBlueprint != null)
            {
                // TODO: Logica de afișare preview la poziția mouse-ului
                // Vector2 mouseWorldPos = Core.Services.ServiceLocator.Get<Graphics.Camera>().ScreenToWorld(input.MousePosition);
                // bool canPlace = CheckPlacementValidity(mouseWorldPos, _selectedBlueprint);
                // DrawPlacementPreview(mouseWorldPos, _selectedBlueprint, canPlace);

                if (input.IsLeftMouseButtonPressed() /* && canPlace */)
                {
                    // PlaceStructure(mouseWorldPos, _selectedBlueprint);
                    //System.Diagnostics.Debug.WriteLine($"Attempting to place {_selectedBlueprint.DisplayName} (Not Implemented)");
                }
            }
        }


        private List<StructureBlueprint> LoadBlueprints()
        {
            System.Diagnostics.Debug.WriteLine("Loading structure blueprints...");
            return new List<StructureBlueprint>
              {
                   new StructureBlueprint("WoodenWall", "Wooden Wall", new Dictionary<string, int> { {"WoodLog", 2} }, 1, 1),
                   new StructureBlueprint("Workbench", "Workbench", new Dictionary<string, int> { {"WoodLog", 5}, {"Stone", 2} }, 2, 1),
                   new StructureBlueprint("Tent", "Simple Tent", new Dictionary<string, int> { {"Cloth", 4}, {"Stick", 6} }, 2, 2),
                   // Adaugă mai multe...
              };
        }

        // Metode placeholder pentru validare și plasare
        private bool CheckPlacementValidity(Vector2 worldPos, StructureBlueprint blueprint)
        {
            // TODO: Verifică dacă tile-urile sunt libere și mersibile etc.
            return true; // Placeholder
        }
        private void DrawPlacementPreview(Vector2 worldPos, StructureBlueprint blueprint, bool isValid)
        {
            // TODO: Desenează un dreptunghi/sprite la poziția mouse-ului, colorat diferit dacă e valid/invalid
        }
        private void PlaceStructure(Vector2 worldPos, StructureBlueprint blueprint)
        {
            // TODO: Verifică resursele din inventar
            // TODO: Consumă resursele
            // TODO: Creează entitatea structurii folosind o fabrică sau direct aici
            // Entity structure = StructureFactory.Create(blueprint.StructureId, worldPos);
            // Core.Services.ServiceLocator.Get<Entities.EntityManager>().AddEntity(structure);
        }
    }
}