using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.World
{
    public interface ITileIterator
    {
        Tile GetNext();    // Returnează următorul tile
        bool HasMore();   // Verifică dacă mai există tile-uri
        void Reset();     // Resetează iteratorul la început (opțional, dar util)
    }
}