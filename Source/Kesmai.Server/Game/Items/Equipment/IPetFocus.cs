using System;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public interface IPetFocus
{
    // This interface is used to mark items that can be used to focus a pet and improve its stats. 
    // It is not intended to be implemented by any specific class, but rather to be used as a marker interface.
    // This allows for easy identification of items that can be used to focus a pet without having to
    // implement any specific functionality.

    double FocusLevel { get; }
    
}