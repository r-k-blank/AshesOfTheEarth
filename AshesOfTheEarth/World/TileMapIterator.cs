using System;



namespace AshesOfTheEarth.World
{
    public class TileMapIterator : ITileIterator // Implementează noua interfață
    {
        private readonly TileMap _tileMap;
        private int _currentX; // Schimbat pentru a începe de la 0 la primul HasMore/GetNext
        private int _currentY;
        private bool _initialCall = true; // Pentru a gestiona prima apelare a HasMore/GetNext

        public TileMapIterator(TileMap tileMap)
        {
            _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
            Reset(); // Setează starea inițială
        }

        // Nu mai avem proprietatea 'Current'
        // public Tile Current { get; private set; }

        public Tile GetNext()
        {
            if (!HasMore()) // Verifică întâi dacă există un element
            {
                // Sau gestionează cazul când se apelează GetNext fără HasMore
                throw new InvalidOperationException("No more tiles to iterate or iterator not advanced.");
            }

            Tile nextTile = _tileMap._tiles[_currentX, _currentY];

            // Avansăm iteratorul pentru următorul apel la GetNext/HasMore
            _currentX++;
            if (_currentX >= _tileMap.Width)
            {
                _currentX = 0;
                _currentY++;
            }
            _initialCall = false; // Am avansat cel puțin o dată
            return nextTile;
        }

        public bool HasMore()
        {
            // Dacă este prima apelare și harta nu e goală, sigur avem un element la 0,0
            if (_initialCall && _tileMap.Width > 0 && _tileMap.Height > 0)
            {
                return true;
            }
            // Verifică dacă poziția curentă (care indică *următorul* element de returnat)
            // este încă în limitele hărții.
            return _currentY < _tileMap.Height; // _currentX este gestionat de trecerea la rândul următor
        }

        public void Reset()
        {
            _currentX = 0;
            _currentY = 0;
            _initialCall = true;
            // Nu mai setăm 'Current' la null sau la un tile default aici
        }

        // Metoda Dispose nu mai este necesară de la IEnumerator
        // public void Dispose() { }

        // Metoda MoveNext nu mai este necesară
        // public bool MoveNext() { ... }
    }
}